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
	public class SqlDialectProvider
        : ISqlDialectProvider
	{
		public ISqlDialect GetSqlDialect(string dbConnectionString)
		{
			return new SqlDialect(dbConnectionString);
		}
	}

	public class SqlDialect : SqlDialectStandard
    {
		string _dataSource;
        string _connectionString;
		const string _sqliteProviderName = "System.Data.SQLite";

		public SqlDialect(string connectionString)
            :base(connectionString)
		{
			var builder = new SQLiteConnectionStringBuilder(connectionString);
			_dataSource = builder.DataSource;
            _connectionString = connectionString;

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
		public override bool DatabaseExists()
		{
			if (string.IsNullOrEmpty(_dataSource))
			{
				return false;
			}
			return File.Exists(_dataSource);
		}

        /// <summary>
        /// Gets a value indicating whether a database can be created.
        /// Some database only can be created by DBA.
        /// </summary>
        /// <value><c>true</c> if can create; otherwise, <c>false</c>.</value>
        public override bool CanCreate
        {
            get { return true; }
        }

        /// <summary>
        /// Get a SQL statement to check table exists.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <returns></returns>
        public override string TableExistSql(string tableName)
        {
			return
                string.Format(@"select name from sqlite_master where type = 'table' and name = '{0}'", tableName);
		}

        /// <summary>
        /// Get a SQL statement to rename a table 
        /// </summary>
        /// <param name="oldTableName">table name to be changed</param>
        /// <param name="newTableName">new table name</param>
        /// <returns>return rename table statement ex:"rename table [oldtable] to [newtable]"</returns>
        public override string TableRenameClause(string oldTableName, string newTableName)
        {
            return string.Format("alter table {0} rename to {1}", oldTableName, newTableName);
        }

        /// <summary>
        /// sqlite recomments do not use AUTOINCREMENT attribute for PK. it uses row id for PK column.
        /// </summary>
        public override string ColumnAutoIncrementKeyword
        {
            get { return ""; }
        }

        /// <summary>
        /// Get a keyword to set column default value
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override string ColumnDefaultKeyword(string defaultValue)
        {
            return string.Format("default({0})", defaultValue);
        }

        ///// <summary>
        ///// Get a keyword to set column Foreign key.
        ///// </summary>
        ///// <param name="referenceTable">foreign key table</param>
        ///// <param name="referenceColumn">foreign key table column</param>
        ///// <returns></returns>
        //public override string ColumnReferenceKeyword(string referenceTable, string referenceColumn)
        //{
        //    return string.Format("references {0}({1})", referenceTable, referenceColumn);
        //}

        public override string ToNativeValue(object value)
		{
			if (value == null) return "null";

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

        /// <summary>
        /// Transfer C# DbType to native database column type name.
        /// </summary>
        /// <param name="dbType">DbType name</param>
        /// <param name="size">size of character or number precision (total number of digits)</param>
        /// <param name="scale">number scale (digits to the right of the decimal point)</param>
        /// <returns>Native database column type name.</returns>
        public override string ToNativeDBType(string dbType, int size, int scale)
        {
			switch (dbType)
			{
				case "AnsiString":
				case "String":
				case "StringFixedLength":
				case "AnsiStringFixedLength":
                case "Xml":
					return "TEXT";
				case "Int64":
				case "Int32":
				case "Int16":
				case "UInt64":
				case "UInt32":
				case "UInt16":
				case "Boolean":
				case "SByte":
				case "Byte":
					return "INTEGER";
				case "Binary":
				case "Object":
					return "BLOB";
				case "Guid":
					return "GUID";
				case "Double":
                case "VarNumeric":
                case "Single":
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

        private void BuildDropTableIndexes(StringBuilder builder, string tableName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string sqlCom = string.Format("select 'drop index ' || name || ';' from sqlite_master  where type = 'index' and tbl_name = '{0}' and sql is not null;", tableName);
                using (SQLiteCommand scdCommand = new SQLiteCommand(sqlCom, conn))
                {
                    using (SQLiteDataReader reader = scdCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            builder.Append(reader.GetString(0));
                        }

                        reader.Close();
                    }
                    conn.Close();
                }
            }

        }

        public override List<string> TableUpdateSqls(SqlDDLTable table)
        {
            var statements = base.TableUpdateSqls(table);
            if (statements.Count==0)
            {
                return statements;
            }
            statements = new List<string>();
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

                BuildDropTableIndexes(builder, table.TableName);

                //4. create a new table statement
                builder.Append(string.Join(";",base.TableCreateSqls(table)));

                //var newColumns = String.Join(",", table.Columns.Select(x => x.Name));
                //var prevColumns = String.Join(",", table.Columns.Select(x => x.PreviousColumn == null ? x.Name : x.PreviousColumn.ColumnName));

                string newColumns="";
                string preColumns="";
                foreach(var c in table.Columns)
                {
                    if (c.PreviousColumn == null && c.IsDirty) continue;
                    if (string.IsNullOrEmpty(newColumns))
                    {
                        newColumns = c.Name;
                        preColumns = c.PreviousColumn == null ? c.Name : c.PreviousColumn.ColumnName;
                    }
                    else
                    {
                        newColumns += ","+c.Name;
                        preColumns += ","+(c.PreviousColumn == null ? c.Name : c.PreviousColumn.ColumnName);
                    }
                }

                //5. copy data
                builder.AppendFormat("insert into {0} ({1}) select {2} from {3};", table.TableName, newColumns, preColumns, tempTable);

                builder.AppendFormat("drop table {0};", tempTable);
                builder.Append("commit;");
                builder.Append("PRAGMA foreign_keys = on;");

                statements.Add(builder.ToString());
            }
            return statements;
        }

        void GetMatchColumns(SqlDDLTable table, out string newColumns, out string prevColumns)
        {
            newColumns = String.Join(",", table.Columns.Select(x => x.Name));
            prevColumns = String.Join(",", table.Columns.Select(x => x.PreviousColumn==null?x.Name:x.PreviousColumn.ColumnName));
        }
	}
}
