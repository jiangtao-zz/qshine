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
        static SqlDialectProvider()
        {
            //Register Oracle specific DbType mapper
            Database.RegisterDbTypeMapper(new DbParameterMapper());
        }
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
        //const string _sqlProviderName = "Oracle.ManagedDataAccess.Client";
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
        /// Do not create index if the column is unique column
        /// </summary>
        public override bool AutoUniqueIndex { get { return true; } }

        /// <summary>
        /// Using outline check constraint
        /// </summary>
        public override bool EnableOutlineCheckConstraint { get { return true; } }

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
            return string.Format(@"select table_name from all_tables where table_name = '{0}'", tableName.ToUpper());
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
        /// Get a keyword of auto increase in create table statement.
        /// Oracle do not support auto increment
        /// </summary>
        public override string ColumnAutoIncrementKeyword
        {
            get { return ""; }
        }


        /// <summary>
        /// Get add new column statement
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public override ConditionalSql ColumnAddClause(string tableName, SqlDDLColumn column)
        {
            return new ConditionalSql(
                string.Format("alter table {0} add {1} {2}", tableName, column.Name, ColumnDefinition(column)));
        }

        /// <summary>
        /// Get table PK constraint name
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string GetConstraintPkName(string tableName)
        {
            return tableName + "_pk";
        }

        public string GetAutoIncreaseSequenceName (string tableName)
        {
            return string.Format("{0}_aid", tableName);
        }

        public string GetAutoIncreaseTriggerName (string tableName)
        {
            return string.Format("{0}_aid", tableName);
        }

        string CreateSequenceSql(string sequenceName, long startValue)
        {
            return string.Format("create sequence {0} start with {1}", sequenceName, startValue);
        }

        string CreateAutoIncrementTriggerSql(string tableName, string columnName, string sequenceName)
        {
            return string.Format(@"
create or replace trigger {0} 
before insert on {1} 
for each row
begin
    if :new.{2} is null then
        select {3}.nextval into :new.{2} from dual;
    end if;
end;", GetAutoIncreaseTriggerName(tableName), tableName, columnName, sequenceName);
        }

        public override List<ConditionalSql> ColumnRemoveAutoIncrementClauses(string tableName, SqlDDLColumn column)
        {
            //Remove the trigger only
            //keep the auto increment sequence for future re-use.
            return new List<ConditionalSql> {
                new ConditionalSql(
                    new DbSqlStatement(string.Format(@"drop trigger {0}", GetAutoIncreaseTriggerName(tableName))),
                    new DbSqlStatement(
                        @"select 1 from user_triggers where table_name =:p1 and trigger_name=:p2",
                        DbParameters.New
                        .Input("p1",tableName.ToUpper())
                        .Input("p2",GetAutoIncreaseTriggerName(tableName).ToUpper())),
                    (result)=>result=="1"
                    )
            };
        }

        public override List<ConditionalSql> ColumnAddAutoIncrementClauses(string tableName,  SqlDDLColumn column)
        {
            var sequenceName = GetAutoIncreaseSequenceName(tableName);

            return new List<ConditionalSql> {
                new ConditionalSql(
                    new DbSqlStatement(CreateSequenceSql(sequenceName, 1000)),
                    new DbSqlStatement(
                        @"select 1 from user_sequences where sequence_name =:p1",
                        DbParameters.New
                        .Input("p1",sequenceName.ToUpper())),
                        (result)=>result!="1"),
                new ConditionalSql(
                    new DbSqlStatement(CreateAutoIncrementTriggerSql(tableName, column.Name, sequenceName)),
                    new DbSqlStatement(
                        @"select 1 from user_triggers where table_name =:p1 and trigger_name=:p2",
                        DbParameters.New
                        .Input("p1",tableName.ToUpper())
                        .Input("p2",GetAutoIncreaseTriggerName(tableName).ToUpper())),
                    (result)=>result!="1"
                    )
            };
        }

        public override List<ConditionalSql> TableCreateAdditionSqls(SqlDDLTable table)
        {
            if (table.PkColumn != null && table.PkColumn.AutoIncrease)
            {
                return ColumnAddAutoIncrementClauses(table.TableName, table.PkColumn);
            }
            return null;
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

            if(value is bool)
            {
                return ((bool)value) ? "1" : "0";
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
                    if (size > 2000 ||size<=0)
                    {
                        return "CLOB";
                    }
                    return string.Format("VARCHAR2({0})", size);

                case "Int64":
                    return "NUMBER(18,0)";

                case "UInt64":
                    return "NUMBER(18,0)";

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
                    return "BLOB";

                case "Guid"://Oracle doesn't have a GUID data type support. the GUID string may contain {}
                    return "CHAR(40)";

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
                case "VarNumeric":
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
                    //Oracle support this type after version 4.122.
                    return "XMLTYPE";

                default:
                    throw new NotSupportedException(String.Format("Doesn't support DbType {0}", dbType));
            }
        }
    }
}
