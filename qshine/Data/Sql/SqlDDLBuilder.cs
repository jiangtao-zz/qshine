using System;
using System.Collections.Generic;
using System.Linq;
using qshine.Configuration;

namespace qshine.database
{
	/// <summary>
	/// Sql database structure management class.
	/// </summary>
	public class SqlDDLBuilder:IDisposable
	{
		string _lastErrorMessage = "";
		List<SqlDDLTable> _tables;
        ISqlDialectProvider _sqlDialectProvider;
		ISqlDialect _sqlDialect;
        Database _database;
        SqlDDLTracking _trackingTable;


        /// <summary>
        /// Create a new SqlDDLBuilder instance for given database using specific sql dialect provider
        /// </summary>
        /// <param name="database">Specifies a database instance to be built</param>
        /// <param name="sqlDialectProvider">Specifies a SQL dialect provider</param>
        public SqlDDLBuilder(Database database, ISqlDialectProvider sqlDialectProvider)
        {
            _sqlDialectProvider = sqlDialectProvider;
            if (_sqlDialectProvider == null)
            {
                _sqlDialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
                Check.HaveValue(_sqlDialectProvider);
            }
            _database = database;

            _tables = new List<SqlDDLTable>();
            _sqlDialect = _sqlDialectProvider.GetSqlDialect(_database.ConnectionString);
        }

        /// <summary>
        /// Create a new SqlDDLBuilder instance for given database connection (configure name) using default sql dialect provider
        /// </summary>
        /// <param name="connectionStringName">Specifies a database connection string configure name. 
        /// The configed database connectionstring contains DB provider name and connection string.</param>
        public SqlDDLBuilder(string connectionStringName)
            :this(new Database(connectionStringName),null)
		{
        }

        /// <summary>
        /// Create a new SqlDDLBuilder instance for given database using default sql dialect provider
        /// </summary>
        /// <param name="database">Database connection and provider</param>
        public SqlDDLBuilder(Database database)
            :this(database,null)
        {
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_dbClient != null)
                {
                    _dbClient.Dispose();
                    _dbClient = null;
                }
            }
        }
        #endregion

		#region Properties

		/// <summary>
		/// Gets database connection string.
		/// </summary>
		/// <value>The name of the database.</value>
		public string ConnectionStringName { get; private set; }

		#endregion

		/// <summary>
		/// Registers the table structure.
		/// </summary>
		/// <returns>The table.</returns>
		/// <param name="table">Table.</param>
		public SqlDDLBuilder RegisterTable(SqlDDLTable table)
		{
			_tables.Add(table);
			return this;
		}

		/// <summary>
		/// Build database instance from registered tables.
		/// The builder will analyze the table structure to generate and perform table structure DDL statements.
		/// </summary>
		/// <returns>It returns true if database build sucessfully. Otherwise, it returns false with LastErrorMessage.
		/// A full action log will be generated in Log information level through Log configuration.
		/// </returns>
		/// <param name="createNewDatabase">Indicates whether a new database should be created. If the flag is true it will only create new database if the given database is not existing.</param>
		public bool Build(bool createNewDatabase = false)
		{
			//Create a new database instance if the database is not existing.
			bool isDatabaseExists = _sqlDialect.DatabaseExists();

			//Create a new database instance if the database not exists when run Build(true)
			if (!isDatabaseExists && createNewDatabase)
			{
				if (_sqlDialect.CanCreate)
				{
					isDatabaseExists = _sqlDialect.CreateDatabase();
					if (!isDatabaseExists)
					{
						_lastErrorMessage = string.Format("Failed to create a database {0} instance. You need create database instance manually.", ConnectionStringName);
						return false;
					}
				}
			}

			//Database should exist to build/update database structure;
			if (!isDatabaseExists)
			{
				_lastErrorMessage = string.Format("Database {0} is not found.", ConnectionStringName);
				return false;
			}


            _trackingTable = new SqlDDLTracking(_sqlDialect,DBClient);
            //Build database
            //Ensure internal tracking table created
            EnsureTrackingTableExists();

            //Load tracking table information
            _trackingTable.Load();

			//Go through each register table
			foreach (var table in _tables)
			{
				if (!IsTableExists(table.TableName))
				{
                    //if table not exists, create a new table
                    if (CreateTable(table))
                    {
                        //add to tracking table
                        _trackingTable.AddToTrackingTable(table);
                    }
				}
				else
				{
					//table already exists, try to update table structure
					TryUpdateTable(table);
				}
			}
			return true;
		}

        bool IsTableExists(string tableName)
        {
            var statement = _sqlDialect.TableExistSql(tableName);
            var name = DBClient.SqlSelect(statement);
            if (name == null) return false;
            return true;
        }

        void BatchSql(List<string> sqls)
        {
            DBClient.Sql(true, sqls);
        }


        DbClient _dbClient;
        /// <summary>
        /// Get database client instance for database access.
        /// DBClient access database through .NET ADO.
        /// </summary>
        DbClient DBClient
        {
            get
            {
                if (_dbClient == null)
                {
                    _dbClient = new DbClient(_database);
                }
                return _dbClient;
            }
        }

        /// <summary>
        /// Try to update table structure if it has any change.
        /// </summary>
        /// <param name="table">Table.</param>
        void TryUpdateTable(SqlDDLTable table)
		{
			var trackingTable = _trackingTable.GetTrackingTable(table.TableName);
			if (trackingTable != null)
			{
				//Table name changed?
				if (table.TableNameHistory.ContainsKey(trackingTable.Version))
				{
					//Match old tableName from current tracking table
					if (trackingTable.ObjectName != table.TableName)
					{
						//rename table
						RenameTableName(trackingTable.ObjectName, table.TableName);

                        //update the tracking table
                        _trackingTable.UpdateTrackingTable(trackingTable, table);
					}
				}

				UpdateTable(table, trackingTable);
                _trackingTable.UpdateTrackingTableColumns(trackingTable, table);
			}
		}

        /// <summary>
        /// Renames the name of the table.
        /// </summary>
        /// <param name="oldTableName">Old table name.</param>
        /// <param name="newTableName">New table name.</param>
        bool RenameTableName(string oldTableName, string newTableName)
        {
            try
            {
                //_logger.Debug("Rename table {0} to {1}.", oldTableName, newTableName);

                DBClient.Sql(_sqlDialect.TableRenameClause(oldTableName, newTableName));

                //_logger.Debug("Rename table completed.");

                return true;
            }
            catch (Exception ex)
            {
                LastErrorMessage = string.Format("Error to rename table {0} to {1}. Ex:{2}", oldTableName, newTableName, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Create a table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        bool CreateTable(SqlDDLTable table)
        {
            try
            {
                //_logger.Debug("Create a table {0}.", table.TableName);
                var statement = _sqlDialect.TableCreateSqls(table);

                //comments are not support
                BatchSql(statement);

                //_logger.Debug("Table {0} created.", table.TableName);

                return true;
            }
            catch (Exception ex)
            {
                LastErrorMessage = String.Format("Failed to create table {0}. Exception: {1}", table.TableName, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Update table schema
        /// </summary>
        /// <param name="table">table definition</param>
        /// <param name="trackingTable">tracking table information</param>
        /// <returns></returns>
        bool UpdateTable(SqlDDLTable table, TrackingTable trackingTable)
        {
            try
            {
                if (_sqlDialect.AnalyseTableChange(table, trackingTable))
                {
                    var sqls = _sqlDialect.TableUpdateSqls(table);
                    if (sqls!=null && sqls.Count>0)
                    {
                        //_logger.Debug("Update table {0}.", table.TableName);

                        BatchSql(sqls);

                        //_logger.Debug("Table {0} updated.", table.TableName);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                LastErrorMessage = String.Format("Failed to update table {0} schema. Exception: {1}", table.TableName, ex.Message);
                return false;
            }
        }


        /// <summary>
        /// Gets the last error message.
        /// </summary>
        /// <value>The last error message.</value>
        public string LastErrorMessage
		{
            get;set;
		}

		static bool _internalTableExists = false;
		static object lockObject = new object();
		void EnsureTrackingTableExists()
		{
			if (!_internalTableExists)
			{
				lock (lockObject)
				{
					if (!_internalTableExists)
					{
						if (!IsTableExists(SqlDDLTracking.TrackingTableName))
						{
							CreateTable(_trackingTable.TrackingTable);
						}
						if (!IsTableExists(SqlDDLTracking.TrackingColumnTableName))
						{
							CreateTable(_trackingTable.TrackingColumnTable);
						}

						_internalTableExists = true;
					}
				}
			}
		}

	}
}
