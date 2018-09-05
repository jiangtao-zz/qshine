using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace qshine.database.oracle
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
        //string _dataSource;
        string _connectionString;
        const string _sqlProviderName = "Oracle.ManagedDataAccess.Client";
        OracleConnectionStringBuilder _connectionBuilder;

        /// <summary>
        /// Construct a database instance by connectionStrings setting
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlDialect(string connectionString)
            : base(connectionString)
        {
            _connectionString = connectionString;
            _connectionBuilder = new OracleConnectionStringBuilder(connectionString);

        }

        public override string ParameterPrefix
        {
            get
            {
                return ":";
            }
        }

        /// <summary>
        /// Using control char as a line separator and parse the command through ParseBatchSql
        /// </summary>
        public override string SqlCommandSeparator
        {
            get
            {
                return "\x6";
            }
        }

        /// <summary>
        /// Oracle execute command one by one
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public override List<string> ParseBatchSql(string sql)
        {
            var batchCommands = sql.Split(this.SqlCommandSeparator[0]);
            List<string> result = new List<string>();
            foreach (var cmd in batchCommands)
            {
                if (!string.IsNullOrWhiteSpace(cmd))
                {
                    result.Add(cmd);
                }
            }

            return result; ;
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
        /// Check database instance exists.
        /// SQL SERVER database need be created by DBA.
        /// </summary>
        /// <returns><c>true</c>, if database exists, <c>false</c> otherwise.</returns>
        public override bool DatabaseExists()
        {
            //Just check the connection to valify the database exists.
            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LastErrorMessage = String.Format("Failed to connect to database {0}. Exception: {1}", _connectionBuilder.DataSource, ex.Message);
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
            return string.Format(@"select table_name from all_tables where table_name = '{0}'", tableName);
        }

        /// <summary>
        /// Get a SQL statement to rename a table 
        /// </summary>
        /// <param name="oldTableName">table name to be changed</param>
        /// <param name="newTableName">new table name</param>
        /// <returns>return rename table statement ex:"rename table [oldtable] to [newtable]"</returns>
        //public override string TableRenameSql(string oldTableName, string newTableName)
        //{
        //    return string.Format("rename table {0} to {1}", oldTableName, newTableName);
        //}


        /// <summary>
        /// Get a keyword of auto increase in create table statement.
        /// Oracle do not support auto increment
        /// </summary>
        public override string ColumnAutoIncrementKeyword
        {
            get { return ""; }
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
        /// Get a keyword to set column Foreign key.
        /// </summary>
        /// <param name="referenceTable">foreign key table</param>
        /// <param name="referenceColumn">foreign key table column</param>
        /// <returns></returns>
        //public override string ColumnReferenceKeyword(string referenceTable, string referenceColumn)
        //{
        //    return string.Format("references {0}({1})", referenceTable, referenceColumn);
        //}

        public override string CreateIndex(string indexName, string tableName, string indexValue)
        {
            return base.CreateIndex(indexName, tableName, indexValue);
        }

        /// <summary>
        /// Get a sql statement to rename a column and set new column definition
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="oldColumnName">old column name</param>
        /// <param name="newColumnName">new column name</param>
        /// <param name="column">column definition</param>
        /// <returns></returns>
        public override string ColumnRenameSql(string tableName, string oldColumnName, string newColumnName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} rename column {1} to {2}{3}", tableName, oldColumnName, newColumnName
                , SqlCommandSeparator);
        }

        /// <summary>
        /// Get a sql statement to rename a table
        /// </summary>
        /// <param name="oldTableName">old table name</param>
        /// <param name="newTableName">new table name</param>
        /// <returns></returns>
        public override string TableRenameSql(string oldTableName, string newTableName)
        {
            return string.Format("alter table {0} rename to {1}{2}", oldTableName, newTableName
                , SqlCommandSeparator);
        }

        
        /// <summary>
        /// Get a sql statement to reset column definition
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="columnName">column name</param>
        /// <param name="column">Column new definition</param>
        /// <returns></returns>
        public override string ColumnModifySql(string tableName, string columnName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} alter column {1} {2}{3}", tableName, columnName, ColumnDefinition(column)
                , SqlCommandSeparator);
        }

        /// <summary>
        /// Get add new column statement
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public override string ColumnAddSql(string tableName, string columnName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} add column {1} {2}{3}", tableName, columnName, ColumnDefinition(column)
                , SqlCommandSeparator);
        }

        /// <summary>
        /// Add additional clause in end of table creation statement
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        /// <remarks>It is useful to add additional statements in end of table creation sql statement</remarks>
        public override string TableCreateSqlAddition(SqlDDLTable table)
        {
            var additionalClause = base.TableCreateSqlAddition(table);
            if (table.PkColumn != null)
            {
               // additionalClause+=string.Format(",constraint {0} primary key({1})", GetConstraintPkName(table.TableName), table.PkColumn.Name);
            }
            return additionalClause;
        }

        /// <summary>
        /// Get table PK constraint name
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        string GetConstraintPkName(string tableName)
        {
            return tableName + "_pk";
        }

        string CreateSequenceSql(string sequenceName, long startValue)
        {
            return string.Format("create sequence {0} start with {1}{2}", sequenceName, startValue
                , SqlCommandSeparator);
        }

        string CreateAutoIncrementTriggerSql(string tableName, string columnName, string sequenceName)
        {
            return string.Format(@"
create or replace trigger {0}_aid 
before insert on {1} 
for each row
begin
    if :new.{2} is null then
        select {3}.nextval into :new.{2} from dual;
    end if;
end;{4}", tableName, tableName, columnName, sequenceName
, SqlCommandSeparator);
        }

        /// <summary>
        /// Add additional sql statements after table creation statement
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        /// <remarks>
        /// It is useful to create a trigger for oracle PK column auto_increment
        /// </remarks>
        public override string TableCreateSqlAfter(SqlDDLTable table)
        {
            var additionalSql = base.TableCreateSqlAfter(table);
            if (table.PkColumn != null && table.PkColumn.AutoIncrease)
            {
                var sequenceName = string.Format("{0}_seq", table.TableName);

               additionalSql += CreateSequenceSql(sequenceName,1000);
               additionalSql += CreateAutoIncrementTriggerSql(table.TableName,table.PkColumn.Name,sequenceName);
            }
            return additionalSql;
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
                if(string.IsNullOrEmpty(value as string))
                {
                    return "NULL";
                }
                return "'" + ((string)value).Replace("'", "''") + "'";
            }

            if (value is DateTime)
            {
                var datetime = (DateTime)value;
                return string.Format("to_date('{0,4}-{1,2}-{2,2} {3,2}:{4,2}:{5,2}', 'YYYY-MM-DD HH24:MI:SS')", datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, datetime.Second);
            }

            var reservedWord = value as SqlReservedWord;
            if (reservedWord != null)
            {
                if (reservedWord.IsSysDate)
                {
                    return "SYSDATE";
                }
            }
            return string.Format("{0}", value);
        }

        public override string ToSqlCondition(string columnName, string op, object value)
        {
            var v = ToNativeValue(value);
            if (op == "=" && v=="NULL")
            {
                return string.Format("{0} is null", columnName);
            }
            return string.Format("{0} {1} {2}", columnName, op, v);
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
                    if (size > 2000)
                    {
                        return "CLOB";
                    }
                    return string.Format("VARCHAR2({0})", size);

                case "Int64":
                    return "NUMBER(19)";

                case "UInt64":
                    return "NUMBER(19)";

                case "Int32":
                    return "NUMBER(10)";

                case "UInt32":
                    return "NUMBER(10)";

                case "Int16":
                    return "NUMBER(5)";

                case "UInt16":
                    return "NUMBER(5)";

                case "Boolean":
                    return "NUMBER(1)";

                case "SByte":
                    return "NUMBER(3)";

                case "Single":
                    return "BINARY_FLOAT";

                case "Byte":
                    return "NUMBER(3)";

                case "Binary":
                case "Object":
                    if (size == 0)
                    {
                        return "BLOB";
                    }
                    return string.Format("VARBINARY({0})", size);

                case "Guid":
                    return "RAW(32)";

                case "Double":
                    if (size == 0)
                    {
                        return "BINARY_DOUBLE";
                    }
                    else
                    {
                        return string.Format("NUMBER({0},{1})", size,scale);
                    }

                case "Decimal":
                    if (size == 0)
                    {
                        return "NUMBER";
                    }
                    return string.Format("NUMBER({0},{1})", size, scale);

                case "Currency":
                    return "NUMBER(19,4)";

                case "DateTime":
                    return "DATE";

                case "DateTimeOffset":
                    return "TIMESTAMP WITH TIME ZONE";

                case "DateTime2"://Timestamp
                    return "TIMESTAMP";

                case "Time":
                    return "DATE";

                case "Date":
                    return "DATE";

                case "Xml":
                    return "XMLTYPE";

                default:
                    throw new NotSupportedException(String.Format("Doesn't support DbType {0}", dbType));
            }
        }
    }
}
