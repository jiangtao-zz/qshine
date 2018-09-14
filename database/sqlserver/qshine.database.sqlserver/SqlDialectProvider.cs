﻿using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using qshine.Configuration;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace qshine.database.sqlserver
{
    /// <summary>
    /// Implement SQL Server dialect provider.
    /// </summary>
    public class SqlDialectProvider : ISqlDialectProvider
    {
        public ISqlDialect GetSqlDialect(string dbConnectionString)
        {
            return new SqlDialect(dbConnectionString);
        }
    }

    /// <summary>
    /// SQL Server Database dialect class
    /// </summary>
    public class SqlDialect : SqlDialectStandard
    {
        //ILogger _logger;
        string _dataSource;
        string _connectionString;
        SqlConnectionStringBuilder _connectionBuilder;


        /// <summary>
        /// Construct a database instance by connectionStrings setting
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlDialect(string connectionString)
            : base(connectionString)
        {
            _connectionString = connectionString;
            _connectionBuilder = new SqlConnectionStringBuilder(connectionString);

            _dataSource = _connectionBuilder.DataSource;
        }

        /// <summary>
        /// Check database instance exists.
        /// SQL SERVER database need be created by DBA.
        /// </summary>
        /// <returns><c>true</c>, if database exists, <c>false</c> otherwise.</returns>
        public override bool DatabaseExists()
        {
            //Just check the connection to valify the database exists.
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch(Exception ex)
            {
                LastErrorMessage = String.Format("Failed to connect to database {0}. Exception: {1}", _dataSource, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a database can be created.
        /// SQL SERVER database need be created by DBA.
        /// </summary>
        /// <value><c>true</c> if can create; otherwise, <c>false</c>.</value>
        public override bool CanCreate
        {
            get { return false; }
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
        /// Get a keyword of auto increase in create table statement 
        /// </summary>
        public override string ColumnAutoIncrementKeyword
        {
            get { return "identity(1,1)"; }
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

        public override bool EnableOutlineCheckConstraint
        {
            get { return true; }
        }

        public override bool EnableInlineUniqueConstraint
        {
            get { return true; }
        }

        public override bool EnableDefaultConstraint
        {
            get { return true; }
        }


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
            return string.Format("exec sp_rename '{0}.{1}', '{2}', 'COLUMN'", tableName, oldColumnName, newColumnName);
        }

        /// <summary>
        /// Get a sql statement to rename a table
        /// </summary>
        /// <param name="oldTableName">old table name</param>
        /// <param name="newTableName">new table name</param>
        /// <returns></returns>
        public override string TableRenameClause(string oldTableName, string newTableName)
        {
            return string.Format("exec sp_rename '{0}', '{1}'", oldTableName, newTableName);
        }

        /// <summary>
        /// Get add new column statement
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public override string ColumnAddClause(string tableName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} add {1} {2}", tableName, column.Name, ColumnDefinition(column));
        }

        /// <summary>
        /// Change column default value
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column</param>
        /// <returns></returns>
        public override string ColumnModifyDefaultClause(string tableName, SqlDDLColumn column)
        {
            return ColumnRemoveDefaultClause(tableName, column)+";"+
                ColumnAddDefaultClause(tableName, column);
        }

        /// <summary>
        /// Add column default value
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public override string ColumnAddDefaultClause(string tableName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} add constraint {1} {2} for {3}",
                tableName,
                SqlDDLTable.GetDefaultConstraintName(column.TableName, column.InternalId),
                ColumnDefaultKeyword(ToNativeValue(column.DefaultValue)),
                column.Name);
        }

        /// <summary>
        /// Remove column default value
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public override string ColumnRemoveDefaultClause(string tableName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} drop constraint {1}",
                tableName,
                SqlDDLTable.GetDefaultConstraintName(column.TableName, column.InternalId)
                );
        }



        public override string ColumnNotNullClause(string tableName, SqlDDLColumn column)
        {
            return ColumnModifyClause(tableName, column.Name, "not null");
        }

        public override string ColumnNullClause(string tableName, SqlDDLColumn column)
        {
            return ColumnDropClause(tableName, column.Name, "not null");
        }

        private string ColumnModifyClause(string tableName, string columnName, string value)
        {
            return string.Format("alter table {0} alter column {1} set {2}",
                tableName, columnName, value);
        }

        private string ColumnDropClause(string tableName, string columnName, string value)
        {
            return string.Format("alter table {0} alter column {1} drop {2}",
                tableName, columnName, value);
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
                return string.Format("convert('{0,4}-{1,2}-{2,2} {3,2}:{4,2}:{5,2}', 120)", datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, datetime.Second);
            }

            var reservedWord = value as SqlReservedWord;
            if (reservedWord != null)
            {
                if (reservedWord.IsSysDate)
                {
                    return "CURRENT_TIMESTAMP";
                }
            }
            if (value is bool)
            {
                return ((bool)value) ? "1" : "0";
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
                    return string.Format("NCHAR({0})", size);

                case "AnsiStringFixedLength":
                    return string.Format("CHAR({0})", size);

                case "AnsiString":
                case "String":
                    if (size > 8000)
                    {
                        return "VARCHAR(MAX)";
                    }
                    return string.Format("VARCHAR({0})", size);

                case "Int64":
                    return "BIGINT";

                case "UInt64":
                    return "BIGINT";

                case "Int32":
                    return "INT";

                case "UInt32":
                    return "INT";

                case "Int16":
                    return "SMALLINT";

                case "UInt16":
                    return "SMALLINT";

                case "Boolean":
                    return "BIT";

                case "SByte":
                    return "TINYINT";

                case "Single":
                    return "REAL";

                case "Byte":
                    return "TINYINT";

                case "Binary":
                    return string.Format("BINARY({0})", size);

                case "Object":
                    if (size == 0)
                    {
                        return "IMAGE";
                    }
                    return string.Format("VARBINARY({0})", size);

                case "Guid":
                    return "UNIQUEIDENTIFIER";

                case "Double":
                    if (size == 0)
                    {
                        return "FLOAT";
                    }
                    else
                    {
                        return string.Format("FLOAT({0})", size);
                    }

                case "VarNumeric":
                case "Decimal":
                    if (size == 0)
                    {
                        if (scale == 0)
                        {
                            return "DECIMAL(18,6)";
                        }
                    }
                    return string.Format("DECIMAL({0},{1})", size, scale);

                case "Currency":
                    return "MONEY";

                case "DateTime":
                    return "DATETIME";

                case "DateTimeOffset":
                    return "DATETIMEOFFSET";

                case "DateTime2"://Timestamp
                    return "DATETIME2";

                case "Time":
                    return "TIME";

                case "Date":
                    return "DATE";

                case "Xml":
                    return "XML";

                default:
                    throw new NotSupportedException(String.Format("Doesn't support DbType {0}", dbType));
            }
        }
    }
}
