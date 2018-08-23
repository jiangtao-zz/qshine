using System;
using System.Collections.Generic;
using System.Linq;
using qshine.Configuration;

namespace qshine.database
{
	/// <summary>
	/// Sql database structure management class.
	/// </summary>
	public class SqlDatabase
	{
		string _lastErrorMessage = "";
		List<SqlDDLTable> _tables;
		ISqlDatabaseProvider _nativeDatabaseProvider;
		ISqlDatabase _nativeDatabase;
		SqlDDLTracking _trackingTable;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:qshine.database.SqlDatabase"/> class.
		/// </summary>
		/// <param name="connectionStringName">Database name.</param>
		public SqlDatabase(string connectionStringName)
		{
			ConnectionStringName = connectionStringName;
			_tables = new List<SqlDDLTable>();
			_nativeDatabase = GetDatabaseInstance(connectionStringName);
			_trackingTable = new SqlDDLTracking(_nativeDatabase);
		}
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
		public SqlDatabase RegisterTable(SqlDDLTable table)
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
			bool isDatabaseExists = _nativeDatabase.IsDatabaseExists();

			//Create a new database instance if the database not exists when run Build(true)
			if (!isDatabaseExists && createNewDatabase)
			{
				if (_nativeDatabase.CanCreate)
				{
					isDatabaseExists = _nativeDatabase.CreateDatabase();
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
			_trackingTable.Load();

			//Go through each register table
			foreach (var table in _tables)
			{
				if (!_nativeDatabase.IsTableExists(table.TableName))
				{
					//if table not exists, create a new table
					_nativeDatabase.CreateTable(table);
					//add to tracking table
					_trackingTable.AddToTrackingTable(table);
				}
				else
				{
					//table already exists, try to update table structure
					TryUpdateTable(table);
				}
			}
			return true;
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
						_nativeDatabase.RenameTableName(trackingTable.ObjectName, table.TableName);
						//update the tracking table
						_trackingTable.UpdateTrackingTable(trackingTable, table);
					}
				}

				_nativeDatabase.UpdateTable(table, trackingTable);
				_trackingTable.UpdateTrackingTableColumns(trackingTable, table);
			}
		}


		/// <summary>
		/// Gets the last error message.
		/// </summary>
		/// <value>The last error message.</value>
		public string LastErrorMessage
		{
			get
			{
				return _lastErrorMessage;
			}
		}

		/// <summary>
		/// Gets the database instance.
		/// </summary>
		/// <returns>The database instance.</returns>
		/// <param name="connectionStringName">Connection string name.</param>
		public ISqlDatabase GetDatabaseInstance(string connectionStringName)
		{
			if (_nativeDatabaseProvider == null)
			{
				_nativeDatabaseProvider = EnvironmentManager.GetProvider<ISqlDatabaseProvider>();
				if (_nativeDatabaseProvider == null)
				{
					throw new NotImplementedException("Couldn't load Sql database provider from environment configuration.");
				}				
			}
			return _nativeDatabaseProvider.GetInstance(connectionStringName);
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
						if (!_nativeDatabase.IsTableExists(SqlDDLTracking.TrackingTableName))
						{
							_nativeDatabase.CreateTable(_trackingTable.TrackingTable);
						}
						if (!_nativeDatabase.IsTableExists(SqlDDLTracking.TrackingColumnTableName))
						{
							_nativeDatabase.CreateTable(_trackingTable.TrackingColumnTable);
						}

						_internalTableExists = true;
					}
				}
			}
		}

	}
}
