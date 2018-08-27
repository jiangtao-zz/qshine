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
	public class SqlDDLSyntaxProvider : ISqlDDLSyntaxProvider
	{
		public ISqlDDLSyntax GetInstance(string dbConnectionString)
		{
			return new SqlDDLSyntax(dbConnectionString);
		}
	}

	public class SqlDDLSyntax : SqlDDLSyntaxBase
    {
		string _dataSource;
		const string _sqliteProviderName = "System.Data.SQLite";

		public SqlDDLSyntax(string connectionStringName)
            :base(connectionStringName)
		{
			var builder = new SQLiteConnectionStringBuilder(Database.ConnectionString);
			_dataSource = builder.DataSource;
		}

		/// <summary>
		/// Creates a new database instance.
		/// </summary>
		/// <returns><c>true</c>, if database was created, <c>false</c> otherwise.</returns>
		public override bool CreateDatabase()
		{
			try
			{
				SQLiteConnection.CreateFile(_dataSource);
				return true;
			}
			catch (Exception ex)
			{
				LastErrorMessage = ex.Message;
				//_logger.Error("Failed to create database {0}. Exception: {1}", _connectionString, ex.Message);
				return false;
			}
		}

		/// <summary>
		/// Check whether the database exists.
		/// </summary>
		/// <returns><c>true</c>, if database exists, <c>false</c> otherwise.</returns>
		public override bool IsDatabaseExists()
		{
			if (string.IsNullOrEmpty(_dataSource))
			{
				return false;
			}
			return File.Exists(_dataSource);
		}

		/// <summary>
		/// Gets the name of the provider.
		/// </summary>
		/// <value>The name of the provider.</value>
		public override string ProviderName
		{
			get
			{
				return _sqliteProviderName;
			}
		}

        /// <summary>
        /// Get table name check statement
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override string GetTableNameStatement(string tableName)
        {
			return
                string.Format(@"select name from sqlite_master where type = 'table' and name = '{0}'", tableName);
		}

        /// <summary>
        /// sqlite recomments do not use AUTOINCREMENT attribute for PK. it uses row id for PK column.
        /// </summary>
        public override string ColumnAutoIncrementKeyword
        {
            get { return ""; }
        }

        public override string ColumnDefaultKeyword(string defaultValue)
        {
            return string.Format("DEFAULT({0})", defaultValue);
        }

		public override string ToNativeValue(object value)
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

		public override string ToNativeDBType(string dbType, int size)
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

        public override string GetUpdateTableStatement(SqlDDLTable table)
        {
            var statement = base.GetUpdateTableStatement(table);
            if (string.IsNullOrEmpty(statement))
            {
                return "";
            }

            if (table.Columns.SingleOrDefault(x=>x.IsDirty && x.PreviousColumn!=null)!=null)
            {
                //found column attribute change, we have to rebuild the table and copy the data from previous table
                var builder = new StringBuilder();
                //Sqlite do not support many ALTER TABLE modify column action. To modify table column follow below steps
                //1. Disable Foreign key
                builder.Append("PRAGMA foreign_keys=off;");
                //2. Start transaction
                builder.Append("BEGIN TRANSACTION;");
                //3. Rename table to a temp table
                var tempTable = "qtmp" + table.GetHashCode();
                builder.AppendFormat("ALTER TABLE {0} RENAME TO {1};", table.TableName, tempTable);

                //4. create a new table statement
                builder.Append(base.GetCreateTableStatement(table));

                var newColumns = String.Join(",", table.Columns.Select(x => x.Name));
                var prevColumns = String.Join(",", table.Columns.Select(x => x.PreviousColumn == null ? x.Name : x.PreviousColumn.ColumnName));

                //5. copy data
                builder.AppendFormat("insert into {0} ({1}) select {2} from {3};", table.TableName, newColumns, prevColumns, tempTable);

                builder.AppendFormat("drop table {0};", tempTable);
                builder.Append("commit;");
                builder.Append("PRAGMA foreign_keys = on;");

                statement = builder.ToString();
            }
            return statement;
        }

        void GetMatchColumns(SqlDDLTable table, out string newColumns, out string prevColumns)
        {
            newColumns = String.Join(",", table.Columns.Select(x => x.Name));
            prevColumns = String.Join(",", table.Columns.Select(x => x.PreviousColumn==null?x.Name:x.PreviousColumn.ColumnName));
        }
	}
}
