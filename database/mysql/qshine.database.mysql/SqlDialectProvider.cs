using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace qshine.database.mysql
{
    /// <summary>
    /// Implement MySql DDL provider.
    /// </summary>
    public class SqlDialectProvider : ISqlDialectProvider
    {
        public ISqlDialect GetSqlDialect(string dbConnectionString)
        {
            return new SqlDialect(dbConnectionString);
        }
    }

    /// <summary>
    /// MySql DDL Database instance class
    /// </summary>
    public class SqlDialect : SqlDialectStandard
    {
        //ILogger _logger;
        string _dataSource;
        string _connectionString;
        MySqlConnectionStringBuilder _connectionBuilder;


        /// <summary>
        /// Construct a database instance by connectionStrings setting
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlDialect(string connectionString)
            :base(connectionString)
        {
            _connectionString = connectionString;
            _connectionBuilder = new MySqlConnectionStringBuilder(connectionString);

            _dataSource = _connectionBuilder.Database;
        }

        /// <summary>
        /// Get database server connection string without database instance.
        /// </summary>
        /// <returns></returns>
        private string GetSystemDatabaseConnectionString()
        {
            return string.Format("Server={0};Uid={1};Port={2};Pwd={3};SslMode={4}",
                _connectionBuilder.Server,
                _connectionBuilder.UserID,
                _connectionBuilder.Port,
                _connectionBuilder.Password,
                _connectionBuilder.SslMode);
        }

        /// <summary>
        /// Check database instance exists.
        /// </summary>
        /// <returns><c>true</c>, if database exists, <c>false</c> otherwise.</returns>
        public override bool DatabaseExists()
        {
            try
            {
                using (var conn = new MySqlConnection(GetSystemDatabaseConnectionString()))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = string.Format("select count(*) from information_schema.schemata where schema_name = '{0}'", _dataSource);
                        var count = cmd.ExecuteScalar();
                        if (count == null || Convert.ToInt32(count) == 0)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LastErrorMessage = String.Format("Failed to determine database instance {0}. Exception: {1}", _dataSource, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Creates a database based on given connection string.
        /// </summary>
        /// <returns><c>true</c>, if database was created, <c>false</c> otherwise.</returns>
        public override bool CreateDatabase()
        {
            try
            {
                using (var conn = new MySqlConnection(GetSystemDatabaseConnectionString()))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = string.Format("create database if not exists {0};", _dataSource);
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                LastErrorMessage = String.Format("Failed to create database {0}. Exception: {1}", _connectionString, ex.Message);
                return false;
            }
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
            return string.Format(@"select table_name from information_schema.tables where table_name = '{0}'", tableName);
        }

        /// <summary>
        /// Get a SQL statement to rename a table 
        /// </summary>
        /// <param name="oldTableName">table name to be changed</param>
        /// <param name="newTableName">new table name</param>
        /// <returns>return rename table statement ex:"rename table [oldtable] to [newtable]"</returns>
        //public override string TableRenameClause(string oldTableName, string newTableName)
        //{
        //    return string.Format("rename table {0} to {1}", oldTableName, newTableName);
        //}


        /// <summary>
        /// Get a keyword of auto increase in create table statement 
        /// </summary>
        public override string ColumnAutoIncrementKeyword
        {
            get { return "auto_increment"; }
        }

        /// <summary>
        /// Get column default value keyword
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override string ColumnDefaultKeyword(string defaultValue)
        {
            if(defaultValue == ToNativeValue(SqlReservedWord.SysDate))
            {
                //MySql doesn't support DATETIME default value. 
                //We use a trigger to
                return "";
            }
            return string.Format("default {0}", defaultValue);
        }

        /// <summary>
        /// Add additional sql statements after table creation statement
        /// </summary>
        /// <param name="table"></param>
        /// <returns>Return additional sqls for table creation</returns>
        /// <remarks>
        /// It is useful to create a trigger for oracle PK column auto_increment and others
        /// </remarks>
        public override List<string> TableCreateAdditionSqls(SqlDDLTable table)
        {
            var sqls = new List<string>();

            //workaround for default SYSDATE()
            foreach(var column in table.Columns)
            {
                SqlReservedWord reservedValue = column.DefaultValue as SqlReservedWord;
                if (reservedValue!=null && reservedValue.IsSysDate)
                {
                    sqls.Add(SetDefaultSysdateColumn(table.TableName, column.Name, column.AllowNull));
                }
            }

            return sqls;
        }

        string SetDefaultSysdateColumn(string tableName, string columnName, bool isNull)
        {
            var sql = string.Format(
                "alter table {0} modify column {1} datetime {2} default now()",
                tableName, columnName, isNull?"":"not null");
            return sql;
        }

        public override string ColumnModifyDefaultClause(string tableName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} alter column {1} set default {2}", tableName, column.Name, ToNativeValue(column.DefaultValue));
        }

        public override string ColumnRemoveDefaultClause(string tableName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} alter column {1} set default {2}", tableName, column.Name, "null");
        }

        public override string ColumnAddDefaultClause(string tableName, SqlDDLColumn column)
        {
            return ColumnModifyDefaultClause(tableName, column);
        }

        public override string ColumnRemoveConstraintClause(string tableName, SqlDDLColumn column)
        {
            var checkConstraintName = SqlDDLTable.GetCheckConstraintName(tableName, column.InternalId);

            return 
                FormatCommandSqlLine("alter table {0} add constraint {1} check(null)",
                            tableName, checkConstraintName, column.CheckConstraint);
        }

        public override List<string> ColumnRemoveAutoIncrementClauses(string tableName, SqlDDLColumn column)
        {
            //Note: auto increment is only for PK
            return  new List<string>{
                string.Format("alter table {0} drop primary key, add primary key ({1})",tableName,column.Name)
            };
        }

        public override List<string> ColumnAddAutoIncrementClauses(string tableName, SqlDDLColumn column)
        {
            //Note: auto increment is only for PK
            return new List<string>{
                string.Format("alter table {0} modify {1} {2} not null auto_increment",tableName,column.Name, 
                ToNativeDBType(column.DbType.ToString(),column.Size,column.Scale))
            };
        }

        public override string ColumnRemoveIndexClause(string tableName, SqlDDLColumn column)
        {
            var indexName = SqlDDLTable.GetIndexName(tableName, column);

            return
                string.Format("alter table {0} drop index {1}", tableName, indexName);
        }

        public override string ColumnNotNullClause(string tableName, SqlDDLColumn column)
        {
            return FormatCommandSqlLine("alter table {0} modify column {1} {2}",
                tableName, column.Name, ColumnDefinition(column));
        }

        public override string ColumnNullClause(string tableName, SqlDDLColumn column)
        {
            return FormatCommandSqlLine("alter table {0} modify column {1} {2}",
                tableName, column.Name, ColumnDefinition(column));
        }


        /// <summary>
        /// Get a keyword to set column Foreign key.
        /// </summary>
        /// <param name="referenceTable">foreign key table</param>
        /// <param name="referenceColumn">foreign key table column</param>
        /// <returns></returns>
        //public override string ColumnReferenceKeyword(string referenceTable, string referenceColumn)
        //{
        //    return string.Format("references {0}({1})", referenceTable, referenceColumn);
        //}

        /// <summary>
        /// Get a sql statement to rename a column and set new column definition
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="oldColumnName">old column name</param>
        /// <param name="newColumnName">new column name</param>
        /// <param name="column">column definition</param>
        /// <returns></returns>
        public override string ColumnRenameClause(string tableName, string oldColumnName, string newColumnName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} change column {1} {2} {3};", tableName, oldColumnName, newColumnName, ColumnDefinition(column));
        }

        /// <summary>
        /// Indicates whether an inline Foreign key reference constraint used for table column creation.
        /// If it is enabled the actual FK constraint need be implemented in TableInlineConstraintClause
        /// </summary>
        public override bool EnableInlineFKConstraint { get { return true; } }

        public override string ColumnRemoveReferenceClause(string tableName, SqlDDLColumn column)
        {
            var foreignKey = SqlDDLTable.GetForeignKeyName(column.Name, column.InternalId);

            return
                string.Format("alter table {0} drop foreign key {1}",
                tableName, foreignKey);
        }

        public override bool EnableInlineUniqueConstraint
        {
            get { return true; }
        }

        public override string ColumnRemoveUniqueClause(string tableName, SqlDDLColumn column)
        {
            string sql="";
            if (column.IsUnique)
            {
                string uniqeIndexName = (column.IsIndex)
                ? SqlDDLTable.GetIndexName(tableName, column)
                : SqlDDLTable.GetUniqueKeyName(tableName, column.InternalId);

                sql = string.Format("alter table {0} drop index {1}",
                    tableName,
                    uniqeIndexName
                    );

                if (column.IsIndex)
                {
                    sql += ColumnAddIndexClause(tableName, column);
                }
            }
            return sql;
        }


        /// <summary>
        /// Convert an object value to database native literals.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override string ToNativeValue(object value)
        {
            if (value == null) return "NULL";

            if (value is String)
            {
                return "'" + ((string)value).Replace("'", "''") + "'";
            }

            if (value is DateTime)
            {
                var datetime = (DateTime)value;
                return string.Format("STR_TO_DATE('{0}-{1}-{2} {3}:{4}:{5}', '%c-%e-%Y %T')", datetime.Month, datetime.Day, datetime.Year, datetime.Hour, datetime.Minute, datetime.Second);
            }

            var reservedWord = value as SqlReservedWord;
            if (reservedWord != null)
            {
                if (reservedWord.IsSysDate)
                {
                    return "now()";
                }
            }
            return string.Format("{0}", value);
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
                case "StringFixedLength":
                case "AnsiStringFixedLength":
                    return string.Format("CHAR({0})", size);

                case "AnsiString":
                case "String":
                    if (size> 65530 && size<16*1024*1024)
                    {
                        return "MEDIUMTEXT";
                    }
                    else if (size <= 0)
                    {
                        return "LONGTEXT";
                    }
                    return string.Format("VARCHAR({0})", size);

                case "Xml":
                    return "MEDIUMTEXT";

                case "Int64":
                    return "BIGINT";

                case "UInt64":
                    return "BIGINT UNSIGNED";

                case "Int32":
                    return "MEDIUMINT";

                case "UInt32":
                    return "MEDIUMINT UNSIGNED";

                case "Int16":
                    return "SMALLINT";

                case "UInt16":
                    return "SMALLINT UNSIGNED";

                case "Boolean":
                    return "BOOLEAN";

                case "SByte":
                    return "TINYINT";

                case "Single":
                    return "FLOAT";

                case "Byte":
                    return "TINYINT UNSIGNED";

                case "Binary":
                case "Object":
                    if (size > 0)
                    {
                        return string.Format("VARBINARY({0})", size);
                    }
                    return "BLOB";

                case "Guid":
                    return "CHAR(36)";

                case "Double":
                    return "DOUBLE";

                case "Decimal":
                case "VarNumeric":
                    if (size == 0)
                    {
                        size = 18;
                        if (scale == 0)
                        {
                            scale = 10;
                        }
                    }
                    
                    return string.Format("NUMERIC({0},{1})",size,scale);
                case "Currency":
                    return "NUMERIC(18,2)";

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
                    throw new NotSupportedException(String.Format("Doesn't support DbType {0}", dbType));
            }
        }
    }
}
