using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using qshine.Configuration;
using MySql.Data.MySqlClient;

namespace qshine.database.mysql
{
    /// <summary>
    /// Implement MySql DDL provider.
    /// </summary>
    public class SqlDDLSyntaxProvider : ISqlDDLSyntaxProvider
    {
        public ISqlDDLSyntax GetInstance(string dbConnectionString)
        {
            return new SqlDDLSyntax(dbConnectionString);
        }
    }

    /// <summary>
    /// MySql DDL Database instance class
    /// </summary>
    public class SqlDDLSyntax : SqlDDLSyntaxBase
    {
        //ILogger _logger;
        string _dataSource;
        const string _sqlProviderName = "MySql.Data.MySqlClient";
        MySqlConnectionStringBuilder _connectionBuilder;

        /// <summary>
        /// Construct a database instance by connectionStrings setting
        /// </summary>
        /// <param name="connectionStringName"></param>
        public SqlDDLSyntax(string connectionStringName)
            :base(connectionStringName)
        {
            _connectionBuilder = new MySqlConnectionStringBuilder(Database.ConnectionString);

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
        /// Creates a new database instance.
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
                        cmd.CommandText = string.Format("CREATE DATABASE IF NOT EXISTS {0};", _dataSource);
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                LastErrorMessage = String.Format("Failed to create database {0}. Exception: {1}", Database.ConnectionString, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Check whether the database instance exists.
        /// </summary>
        /// <returns><c>true</c>, if database exists, <c>false</c> otherwise.</returns>
        public override bool IsDatabaseExists()
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
        /// Gets the name of the provider.
        /// </summary>
        /// <value>The name of the provider.</value>
        public override string ProviderName
        {
            get
            {
                return _sqlProviderName;
            }
        }

        /// <summary>
        /// Sql statement for table name check
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override string GetTableNameStatement(string tableName)
        {
            return string.Format(@"select table_name from information_schema.tables where table_name = '{0}'", tableName);
        }

        /// <summary>
        /// Get Sql statement to rename a table
        /// </summary>
        /// <param name="oldTableName">old table name</param>
        /// <param name="newTableName">new table name</param>
        /// <returns></returns>
        public override string GetRenameTableStatement(string oldTableName, string newTableName)
        {
            return string.Format("rename table {0} to {1}", oldTableName, newTableName);
        }


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
            return string.Format("default {0}", defaultValue);
        }

        /// <summary>
        /// Get a keyword of Foreign key in create table statement.
        /// </summary>
        /// <param name="referenceTable"></param>
        /// <param name="referenceColumn"></param>
        /// <returns></returns>
        public override string ColumnReferenceKeyword(string referenceTable, string referenceColumn)
        {
            return string.Format("references {0}({1})", referenceTable, referenceColumn);
        }

        /// <summary>
        /// Get column rename statement with new column definition
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="oldColumnName"></param>
        /// <param name="newColumnName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public override string GetRenameColumnNameStatement(string tableName, string oldColumnName, string newColumnName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} change column {1} {2} {3};", tableName, oldColumnName, newColumnName, ColumnDefinition(column));
        }

        /// <summary>
        /// Get column definition change statement
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="columnName">column name</param>
        /// <param name="column">Column new definition</param>
        /// <returns></returns>
        public override string GetModifyColumnStatement(string tableName, string columnName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} modify column {1} {2};", tableName, columnName, ColumnDefinition(column));
        }

        /// <summary>
        /// Get add new column statement
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public override string GetAddColumnStatement(string tableName, string columnName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} add column {1} {2};", tableName, columnName, ColumnDefinition(column));
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
                    return "CURRENT_TIMESTAMP";
                }
            }
            return string.Format("{0}", value);
        }

        /// <summary>
        /// Transfer C# DbType to native database column type name.
        /// </summary>
        /// <param name="dbType">DbType name</param>
        /// <param name="size">size of character</param>
        /// <returns>Native database column type name.</returns>
        public override string ToNativeDBType(string dbType, int size)
        {
            switch (dbType)
            {
                case "StringFixedLength":
                case "AnsiStringFixedLength":
                    return string.Format("CHAR({0})", size);

                case "AnsiString":
                case "String":
                    return string.Format("VARCHAR({0})", size);

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
                    throw new NotSupportedException(String.Format("Doesn't support DbType {0}", dbType));
            }
        }
    }
}
