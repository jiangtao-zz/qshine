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
		readonly List<SqlDDLTable> _tables;
        readonly ISqlDialectProvider _sqlDialectProvider;
        readonly ISqlDialect _sqlDialect;
        SqlDDLTracking _trackingTable;

        BatchException _batchException;

        #region Ctor
        /// <summary>
        /// Create a new SqlDDLBuilder instance for given database using specific sql dialect provider
        /// </summary>
        /// <param name="database">Specifies a database instance to be built</param>
        /// <param name="sqlDialectProvider">Specifies a SQL dialect provider</param>
        public SqlDDLBuilder(SqlDDLDatabase database, ISqlDialectProvider sqlDialectProvider)
        {
            _sqlDialectProvider = sqlDialectProvider;
            if (_sqlDialectProvider == null)
            {
                _sqlDialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
                Check.HaveValue(_sqlDialectProvider);
            }
            Database = database;

            _tables = Database.Tables;
            _sqlDialect = _sqlDialectProvider.GetSqlDialect(Database.Database.ConnectionString);
        }

        /// <summary>
        /// Create a new SqlDDLBuilder instance for given database connection (configure name) using default sql dialect provider
        /// </summary>
        /// <param name="connectionStringName">Specifies a database connection string configure name. 
        /// The configed database connectionstring contains DB provider name and connection string.</param>
        public SqlDDLBuilder(string connectionStringName)
            :this(new SqlDDLDatabase(new Database(connectionStringName)),null)
		{
        }

        /// <summary>
        /// Create a new SqlDDLBuilder instance for given database using default sql dialect provider
        /// </summary>
        /// <param name="database">Database connection and provider</param>
        public SqlDDLBuilder(Database database)
            :this(new SqlDDLDatabase(database),null)
        {
        }

        #endregion

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
        /// Sql DDL Database
        /// </summary>
        public SqlDDLDatabase Database { get; private set; }


        /// <summary>
        /// Gets database connection string.
        /// </summary>
        /// <value>The name of the database.</value>
        public string ConnectionStringName { get; private set; }

        public SqlDDLTracking TrackingTable
        {
            get
            {
                return _trackingTable;
            }
        }

		#endregion

		/// <summary>
		/// Build database instance from registered tables.
		/// The builder will analyze the table structure to generate and perform table structure DDL statements.
		/// </summary>
		/// <returns>It returns true if database build sucessfully. Otherwise, it returns false with LastErrorMessage.
		/// A full action log will be generated in Log information level through Log configuration.
		/// </returns>
        /// <param name="batchException">BatchException to hold error messages</param>
		/// <param name="createNewDatabase">Indicates whether a new database should be created. If the flag is true it will only create new database if the given database is not existing.</param>
		public bool Build(BatchException batchException, bool createNewDatabase = false)
		{
            _batchException = batchException;

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
                        var errorMessage = string.Format("Failed to create a database {0} instance. You need create database instance manually.", ConnectionStringName);
                        if (batchException != null)
                        {
                            batchException.Exceptions.Add(new Exception(errorMessage));
                            batchException.TryThrow();
                        }
                        return false;
                    }
                }
			}

			//Database should exist to build/update database structure;
			if (!isDatabaseExists)
			{
				var errorMessage = string.Format("Database {0} is not found.", ConnectionStringName);
                if (batchException != null)
                {
                    batchException.Exceptions.Add(new Exception(errorMessage));
                    batchException.TryThrow();
                }
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
                //Skip table update for those have higher version table already created in the database
                var currentTrackingTable = _trackingTable.FindSameTrackingTable(table);
                if (currentTrackingTable != null)
                {
                    if (IsTableExists(currentTrackingTable.ObjectName))
                    {
                        continue;
                    }
                    else
                    {
                        //Current tracking table is deleted manually out of the system control.
                        //Remove the tracking record and recreate it again.
                        _trackingTable.RemoveTrackingTable(currentTrackingTable.Id);
                    }
                }

                //New table
                if (!IsTableExists(table.TableName))
				{
                    if (table.IsTableRenamed)
                    {
                        //try to rename
                        var trackingTable = _trackingTable.GetTrackingTable(table.Id);
                        //It may require table name change.
                        if (trackingTable != null && IsTableExists(trackingTable.ObjectName))
                        {
                            //rename table and update the info
                            RenameAndUpdateTable(table, trackingTable);
                            continue;
                        }
                    }

                    if (table.Id > 0)
                    {
                        var trackingTable = _trackingTable.GetTrackingTable(table.Id);
                        if(trackingTable != null)
                        {
                            if (IsTableExists(trackingTable.ObjectName) && table.Version <= trackingTable.Version)
                            {
                                //ignore the table creation if table Id presented explicitly and high version table already exists
                                continue;
                            }
                        }
                    }

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
            if (batchException != null)
            {
                batchException.TryThrow();
                if (batchException.Exceptions.Count > 0)
                {
                    return false;
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
            DBClient.Sql(sqls, _batchException);
        }

        void BatchSql(List<ConditionalSql> sqls)
        {
            DBClient.Sql(sqls, _batchException);
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
                    _dbClient = new DbClient(Database.Database);
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
				UpdateTable(table, trackingTable);
                _trackingTable.UpdateTrackingTableColumns(trackingTable, table);
			}
		}

        /// <summary>
        /// Rename and update table structure if it has any change.
        /// </summary>
        /// <param name="table">Table.</param>
        void RenameAndUpdateTable(SqlDDLTable table, TrackingTable trackingTable)
        {
            if (trackingTable != null)
            {
                //Match old tableName from current tracking table
                if (trackingTable.ObjectName != table.TableName)
                {
                    //rename table
                    RenameTableName(trackingTable.ObjectName, table.TableName);

                    //update the tracking table
                    _trackingTable.UpdateTrackingTableForTableRename(trackingTable, table);
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
                var errorMessage = string.Format("Error to rename table {0} to {1}. Ex:{2}", oldTableName, newTableName, ex.Message);
                if (_batchException != null)
                {
                    _batchException.Exceptions.Add(new Exception(errorMessage));
                    _batchException.TryThrow();
                }
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
            //_logger.Debug("Create a table {0}.", table.TableName);
            var statement = _sqlDialect.TableCreateSqls(table);

            //comments are not support
            BatchSql(statement);
            return true;
        }

        /// <summary>
        /// Update table schema
        /// </summary>
        /// <param name="table">table definition</param>
        /// <param name="trackingTable">tracking table information</param>
        /// <returns></returns>
        bool UpdateTable(SqlDDLTable table, TrackingTable trackingTable)
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


		static bool _internalTableExists = false;
		static readonly object lockObject = new object();
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

                        if (!IsTableExists(SqlDDLTracking.TrackingNameTableName))
                        {
                            CreateTable(_trackingTable.TrackingRenameTable);
                        }

                        _internalTableExists = true;
					}
				}
			}
		}
	}
}
