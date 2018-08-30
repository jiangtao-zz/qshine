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


        /// <summary>
        /// Initializes a new instance of the <see cref="T:qshine.database.SqlDDLBuilder"/> class.
        /// </summary>
        /// <param name="connectionStringName">Database name.</param>
        public SqlDDLBuilder(string connectionStringName)
		{
            _database = new Database(connectionStringName);

			//ConnectionStringName = connectionStringName;
			_tables = new List<SqlDDLTable>();
			LoadSqlDialect(_database.ConnectionString);
		}

        SqlDDLTracking _trackingTable;
        SqlDDLTracking TrackingTable
        {
            get
            {
                if (_trackingTable == null)
                {
                    _trackingTable = new SqlDDLTracking(_sqlDialect, DBClient);
                }
                return _trackingTable;
            }
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



			//Build database
			//Ensure internal tracking table created
			EnsureTrackingTableExists();

            //Load tracking table information
            TrackingTable.Load();

			//Go through each register table
			foreach (var table in _tables)
			{
				if (!IsTableExists(table.TableName))
				{
                    //if table not exists, create a new table
                    if (CreateTable(table))
                    {
                        //add to tracking table
                        TrackingTable.AddToTrackingTable(table);
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


        DbClient _dbClient;
        /// <summary>
        /// Get database client instance for database access.
        /// DBClient access database through .NET ADO.
        /// </summary>
        public DbClient DBClient
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
			var trackingTable = TrackingTable.GetTrackingTable(table.TableName);
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
                        TrackingTable.UpdateTrackingTable(trackingTable, table);
					}
				}

				UpdateTable(table, trackingTable);
                TrackingTable.UpdateTrackingTableColumns(trackingTable, table);
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

                DBClient.Sql(_sqlDialect.TableRenameSql(oldTableName, newTableName));

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
                var statement = _sqlDialect.TableCreateSql(table);

                //comments are not support
                DBClient.Sql(statement);

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
                if (AnalyseTable(table, trackingTable))
                {
                    var sql = _sqlDialect.TableUpdateSql(table);
                    if (!string.IsNullOrEmpty(sql))
                    {
                        //_logger.Debug("Update table {0}.", table.TableName);

                        DBClient.Sql(sql);

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
        /// Analyse the table structure and get table and column modified information
        /// </summary>
        /// <returns>true, if the table structure changed</returns>
        /// <param name="table">Table structure.</param>
        /// <param name="trackingTable">Tracking table information.</param>
        public bool AnalyseTable(SqlDDLTable table, TrackingTable trackingTable)
        {
            if (trackingTable == null || trackingTable.Columns == null)
            {
                //indicates no tracking information found
                return false;
            }

            List<SqlDDLColumn> newColumns = new List<SqlDDLColumn>();
            Dictionary<string, string> mapColumns = new Dictionary<string, string>();

            bool isTableChanged = false;

            foreach (var column in table.Columns)
            {
                var trackingColumn = trackingTable.Columns.SingleOrDefault(x => x.ColumnName == column.Name);
                if (trackingColumn == null)
                {
                    if (column.ColumnNameHistory == null || column.ColumnNameHistory.Count == 0)
                    {
                        //add new column, most case
                        column.IsDirty = true;
                        isTableChanged = true;
                    }
                    else //rename
                    {
                        var previousColumn = trackingTable.Columns.SingleOrDefault(x => column.ColumnNameHistory.Contains(x.ColumnName));
                        if (previousColumn != null)
                        {
                            column.IsDirty = true;
                            isTableChanged = true;
                            column.PreviousColumn = previousColumn;
                        }
                        else
                        {
                            //unexpected, previous column is not in tracking list
                            throw new Exception(String.Format("Rename column name {0}-{1} is not in column tracking table", column.Name, column.ColumnNameHistory));
                        }
                    }
                }
                else
                {
                    if (IsColumnPropertyChanged(column, trackingColumn))
                    {
                        column.IsDirty = true;
                        isTableChanged = true;
                        column.PreviousColumn = trackingColumn;
                    }
                }
            }
            return isTableChanged;
        }

        bool IsColumnPropertyChanged(SqlDDLColumn column, TrackingColumn trackingColumn)
        {
            if (column.Version > trackingColumn.Version)
            {
                if (column.AllowNull != trackingColumn.AllowNull ||
                   column.AutoIncrease != trackingColumn.AutoIncrease ||
                   column.CheckConstraint != trackingColumn.CheckConstraint ||
                   column.IsIndex != trackingColumn.IsIndex ||
                   column.IsPK != trackingColumn.IsPK ||
                   column.IsUnique != trackingColumn.IsUnique ||
                    column.Reference != trackingColumn.Reference)
                {
                    return true;
                }
                string stringValue = Convert.ToString(column.DefaultValue);
                if (!stringValue.AreEqual(trackingColumn.DefaultValue))
                {
                    return true;
                }

                if (_sqlDialect.ToNativeDBType(column.DbType.ToString(), column.Size) != _sqlDialect.ToNativeDBType(trackingColumn.ColumnType, column.Size))
                {
                    return true;
                }
            }
            return false;
        }



        /// <summary>
        /// Gets the last error message.
        /// </summary>
        /// <value>The last error message.</value>
        public string LastErrorMessage
		{
            get;set;
		}

		/// <summary>
		/// Gets the database instance.
		/// </summary>
		/// <returns>The database instance.</returns>
		/// <param name="connectionString">Connection string name.</param>
		public void LoadSqlDialect(string connectionString)
		{
			if (_sqlDialectProvider == null)
			{
				_sqlDialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
				if (_sqlDialectProvider == null)
				{
					throw new NotImplementedException("Couldn't load Sql database provider from environment configuration.");
				}				
			}
			_sqlDialect = _sqlDialectProvider.GetSqlDialect(connectionString);
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
							CreateTable(TrackingTable.TrackingTable);
						}
						if (!IsTableExists(SqlDDLTracking.TrackingColumnTableName))
						{
							CreateTable(TrackingTable.TrackingColumnTable);
						}

						_internalTableExists = true;
					}
				}
			}
		}

	}
}
