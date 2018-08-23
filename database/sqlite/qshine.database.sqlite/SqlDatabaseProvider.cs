using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using qshine.Configuration;

namespace qshine.database.sqlite
{
	public class SqlDatabaseProvider : ISqlDatabaseProvider
	{
		public ISqlDatabase GetInstance(string dbConnectionString)
		{
			return new SqlDatabase(dbConnectionString);
		}
	}

	public class SqlDatabase:ISqlDatabase
	{
		ILogger _logger;
		string _connectionString;
		string _dataSource;
		DbClient _dbClient;
		Database _database;
		const string _sqliteProviderName = "System.Data.SQLite";

		public SqlDatabase(string connectionStringName)
		{
			_database = new Database(connectionStringName);

			_connectionString = _database.ConnectionString;
			var builder = new SQLiteConnectionStringBuilder(_connectionString);
			_dataSource = builder.DataSource;
			_logger = Log.GetLogger("database");
		}

		public string LastErrorMessage
		{
			get;set;
		}

		/*
		public bool Build(IList<SqlDDLTable> tables)
		{
			foreach (var table in tables)
			{
				StringBuilder builder;
				if (IsTableExists(table.TableName))
				{
					builder = CreateTable(table);
				}
				else
				{
					//builder = UpdateTable(table);
				}
			}
			throw new NotImplementedException();
		}
		*/

		/// <summary>
		/// Creates a new database instance.
		/// </summary>
		/// <returns><c>true</c>, if database was created, <c>false</c> otherwise.</returns>
		public bool CreateDatabase()
		{
			try
			{
				SQLiteConnection.CreateFile(_dataSource);
				return true;
			}
			catch (Exception ex)
			{
				LastErrorMessage = ex.Message;
				_logger.Error("Failed to create database {0}. Exception: {1}", _connectionString, ex.Message);
				return false;
			}
		}

		/// <summary>
		/// Check whether the database exists.
		/// </summary>
		/// <returns><c>true</c>, if database exists, <c>false</c> otherwise.</returns>
		public bool IsDatabaseExists()
		{
			if (string.IsNullOrEmpty(_dataSource))
			{
				return false;
			}
			return File.Exists(_dataSource);
		}

		public bool CanCreate
		{
			get
			{
				return true;
			}
		}

		public DbClient DBClient
		{
			get
			{
				if (_dbClient == null)
				{
					_dbClient = new DbClient(_sqliteProviderName, _connectionString);
				}
				return _dbClient;
			}
		}

		/// <summary>
		/// Gets the name of the provider.
		/// </summary>
		/// <value>The name of the provider.</value>
		public string ProviderName
		{
			get
			{
				return _sqliteProviderName;
			}
		}

		/// <summary>
		/// Gets or sets the connection string.
		/// </summary>
		/// <value>The connection string.</value>
		public string ConnectionString
		{
			get
			{
				return _connectionString;
			}

			set
			{
				_connectionString = value;
			}
		}

		public bool IsTableExists(string tableName)
		{
			var name = DBClient.SqlSelect(
			@"select name from sqlite_master where type = 'table' and name = @name",
				DbParameters.New.Input("name",tableName));
			if (name == null) return false;
			return true;
		}

		private StringBuilder BuildCreateTableStatement(StringBuilder builder, SqlDDLTable table)
		{
			//Create DDL
			builder.Append("create table ");
			builder.Append(table.TableName);
			builder.AppendLine(" (");
			int totalCount = table.Columns.Count;
			for(int i = 0; i<totalCount;i++)
			{
				var column = table.Columns[i];
				if (table.PkColumn != null && table.PkColumn.Name == column.Name)
				{

					BuildPkColumn(builder, column);
				}
				else
				{
					BuildColumn(builder, column);
				}
				if (i != totalCount - 1)
				{
					builder.AppendLine(",");
				}
				else
				{
					//It is a last column:
				}
			}
			builder.AppendLine(");");
			return builder;
		}

		private StringBuilder BuildCreateTableIndexeStatements(StringBuilder builder, SqlDDLTable table)
		{
			foreach (var index in table.Indexes)
			{
				builder.AppendFormat("create index {0} on {1} ({2});\n", index.Key, table.TableName, index.Value);
			}
			return builder;
		}


		public bool CreateTable(SqlDDLTable table)
		{
			var builder = new StringBuilder();

			BuildCreateTableStatement(builder, table);
			BuildCreateTableIndexeStatements(builder, table);

			//comments are not support
			DBClient.Sql(builder.ToString());

			return true;
		}

		public void BuildPkColumn(StringBuilder builder, SqlDDLColumn column)
		{
			//Primary key alway be auto increase.
			builder.AppendFormat("{0} {1} primary key", column.Name, ToNativeDBType(column.DbType, column.Size));

			if (!column.AllowNull)
			{
				builder.Append(" not null");
			}

			if (column.IsUnique)
			{
				builder.Append(" unique");
			}
		}

		public void BuildColumn(StringBuilder builder, SqlDDLColumn column)
		{
			builder.AppendFormat(" {0} {1}",column.Name, ToNativeDBType(column.DbType, column.Size));

			if (!column.AllowNull)
			{
				builder.Append(" not null");
			}

			if (column.IsUnique)
			{
				builder.Append(" unique");
			}

			if (column.DefaultValue!=null && column.DefaultValue.ToString()!="")
			{
				builder.AppendFormat(" DEFAULT ({0})",ToNativeValue(column.DefaultValue));
			}

			//Add constraint
			if (!string.IsNullOrEmpty(column.CheckConstraint))
			{
				builder.Append(" ");
				builder.Append(column.CheckConstraint);
			}

			if (!string.IsNullOrEmpty(column.Reference))
			{
				var foreignKeyReference = column.Reference.Split(':');
				if (foreignKeyReference.Length != 2)
				{
					throw new FormatException(string.Format("Invalid foreign key reference {0} for column {1}",column.Reference, column.Name));
				}
				builder.AppendFormat(" references {0}({1})", foreignKeyReference[0], foreignKeyReference[1]);
			}
		}

		public string ToNativeValue(object value)
		{
			if (value == null) return "NULL";

			if(value is String){
				return "'" + ((string)value).Replace("'", "''")+"'";
			}

			if(value is DateTime){
				var datetime = (DateTime)value;
				string dateTimeFormat = "'{0}-{1}-{2} {3}:{4}:{5}.{6}'";
      			return string.Format(dateTimeFormat, datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, datetime.Second, datetime.Millisecond);
			}

			var reservedWord = value as SqlReservedWord;
			if (reservedWord != null)
			{
				if (reservedWord.IsSysDate)
				{
					return "datetime('now','localtime')";
				}
			}
			return string.Format("{0}",value);
		}

		public string ToNativeDBType(System.Data.DbType dbType, int size)
		{
			return ToNativeDBType(dbType.ToString(), size);
		}

		public string ToNativeDBType(string dbType, int size)
		{
			switch (dbType)
			{
				case "AnsiString":
				case "String":
				case "StringFixedLength":
				case "AnsiStringFixedLength":
					return "TEXT";
				case "Int64":
				case "Int32":
				case "Int16":
				case "UInt64":
				case "UInt32":
				case "UInt16":
				case "Boolean":
				case "SByte":
				case "Single":
				case "Byte":
					return "INTEGER";
				case "Binary":
				case "Object":
					return "BLOB";
				case "Guid":
					return "GUID";
				case "Double":
					return "REAL";
				case "Decimal":
				case "Currency":
					return "NUMERIC";
				case "DateTime":
				case "DateTimeOffset":
					return "DATETIME";
				case "DateTime2"://Timestamp
					return "TIMESTAMP";
				case "Time":
					return "TIME";
				case "Date":
					return "DATE";
				default:
					throw new NotSupportedException(String.Format("Doesn't support DbType {0}",dbType));
			}
		}

		public bool UpdateTable(SqlDDLTable table, TrackingTable trackingTable)
		{
			if (trackingTable == null || trackingTable.Columns == null)
			{
				//unexpected, if someone delete the tracking table
				return true;
			}

			List<SqlDDLColumn> newColumns = new List<SqlDDLColumn>();
			Dictionary<string, string> mapColumns = new Dictionary<string, string>();

			var builder = new StringBuilder();
			bool isTableChanged = false;

			if (table.Version > trackingTable.Version)
			{
				isTableChanged = true;
			}
			else
			{
				foreach (var column in table.Columns)
				{
					var trackingColumn = trackingTable.Columns.SingleOrDefault(x => x.ColumnName == column.Name);
					if (trackingColumn == null)
					{
						if (column.ColumnNameHistory == null || column.ColumnNameHistory.Count == 0)
						{
							//add new column, most case
							newColumns.Add(column);
							isTableChanged = true;
						}
						else //rename
						{
							var previousColumn = trackingTable.Columns.SingleOrDefault(x => column.ColumnNameHistory.Contains(x.ColumnName));
							if (previousColumn != null)
							{
								mapColumns.Add(column.Name, previousColumn.ColumnName);
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
							isTableChanged = true;
						}
						mapColumns.Add(column.Name, column.Name);
					}
				}
			}

			if (isTableChanged)
			{
				//Sqlite do not support many ALTER TABLE modify column action. To modify table column follow below steps
				//1. Disable Foreign key
				builder.Append("PRAGMA foreign_keys=off;");
				//2. Start transaction
				builder.Append("BEGIN TRANSACTION;");
				//3. Rename table to a temp table
				var tempTable = "qtmp" + table.GetHashCode();
				builder.AppendFormat("ALTER TABLE {0} RENAME TO {1};", table.TableName, tempTable);

				//4. create a new table statement
				BuildCreateTableStatement(builder, table);

				//5. copy data
				builder.AppendFormat("insert into {0} ({1}) select {2} from {3};",table.TableName, 
				                     string.Join(",", from item in mapColumns.Keys select item),
				                     string.Join(",", from item in mapColumns.Values select item), tempTable);

				builder.AppendFormat("drop table {0};",tempTable);
				builder.Append("commit;");
				builder.Append("PRAGMA foreign_keys = on;");
			}
			else if (newColumns.Count > 0)
			{
				foreach (var column in newColumns)
				{
					var dbType = ToNativeDBType(column.DbType, column.Size);
					//add new column
					builder.AppendFormat("alter table {0} add {1} {2};", table.TableName, column.Name, dbType);
				}
			}

			var sql = builder.ToString();
			if (!string.IsNullOrEmpty(sql))
			{
				DBClient.Sql(sql);
				return true;
			}
			return false;
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
				if (ToNativeDBType(column.DbType, column.Size) != ToNativeDBType(trackingColumn.ColumnType,column.Size))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Renames the name of the table.
		/// </summary>
		/// <param name="oldTableName">Old table name.</param>
		/// <param name="newTableName">New table name.</param>
		public void RenameTableName(string oldTableName, string newTableName)
		{
			try
			{
				using (var dbClient = new DbClient(ProviderName, ConnectionString))
				{
					dbClient.Sql(string.Format("rename table {0} to {1}", oldTableName, newTableName));
				}
			}
			catch (Exception ex)
			{
				//throw error message
				var msg = string.Format("Error to rename table {0} to {1}. Ex:{2}", oldTableName, newTableName, ex.Message);
				throw new Exception(msg);
			}
		}

	}
}
