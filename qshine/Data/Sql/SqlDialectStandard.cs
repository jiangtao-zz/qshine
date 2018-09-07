using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using qshine.Configuration;

namespace qshine.database
{
    /// <summary>
    /// Implement Sql statement syntax provider.
    /// </summary>
    public abstract class SqlDialectStandard : ISqlDialect
    {
        /// <summary>
        /// Construct a database standard SQL dialect.
        /// Other database SQL dialect could inhert from this Dialect and override the non-standard SQL statements.
        /// </summary>
        public SqlDialectStandard(string connectionString)
        {
        }

        /// <summary>
        /// Gets .NET ADO provider name.
        /// </summary>
        /// <value>The name of the provider.</value>
        public abstract string ProviderName
        {
            get;
        }


        /// <summary>
        /// standard named parameter prefix symbol
        /// </summary>
        public virtual string ParameterPrefix
        {
            get
            {
                return "@";
            }
        }

        public virtual string SqlCommandSeparator
        {
            get
            {
                return ";";
            }
        }

        public virtual List<string> ParseBatchSql(string sql)
        {
            return new List<string>() { sql };
        }

        /// <summary>
        /// Creates a database based on given connection string.
        /// </summary>
        /// <returns><c>true</c>, if database was created, <c>false</c> otherwise.</returns>
        public virtual bool CreateDatabase()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Check database instance exists.
        /// </summary>
        /// <returns><c>true</c>, if database exists, <c>false</c> otherwise.</returns>
        public abstract bool DatabaseExists();

        /// <summary>
        /// Gets a value indicating whether a database can be created.
        /// Some database only can be created by DBA.
        /// </summary>
        /// <value><c>true</c> if can create; otherwise, <c>false</c>.</value>
        public abstract bool CanCreate
        {
            get;
        }

        /// <summary>
        /// Indicates an unique index created automatically if the column has unique constraint
        /// </summary>
        public virtual bool AutoUniqueIndex { get { return false; } }

        /// <summary>
        /// Get a SQL statement to check table exists.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <returns></returns>
        public abstract string TableExistSql(string tableName);

        /// <summary>
        /// Get a SQL statement to rename a table 
        /// </summary>
        /// <param name="oldTableName">table name to be changed</param>
        /// <param name="newTableName">new table name</param>
        /// <returns>return rename table statement ex:"rename table [oldtable] to [newtable]"</returns>
        public virtual string TableRenameSql(string oldTableName, string newTableName)
        {
            return string.Format("rename table {0} to {1}{2}", 
                oldTableName, newTableName, SqlCommandSeparator);
        }

        /// <summary>
        /// Get a keyword to set Auto-increment column
        /// </summary>
        public virtual string ColumnAutoIncrementKeyword
        {
            get { return "generated always as identity"; }
        }

        /// <summary>
        /// Get a keyword to set column default value
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public virtual string ColumnDefaultKeyword(string defaultValue)
        {
            return string.Format("default ({0})", defaultValue);
        }

        /// <summary>
        /// Get a keyword to set column Foreign key.
        /// </summary>
        /// <param name="referenceTable">foreign key table</param>
        /// <param name="referenceColumn">foreign key table column</param>
        /// <returns></returns>
        public virtual string ColumnReferenceKeyword(string referenceTable, string referenceColumn)
        {
            return string.Format("references {0}({1})", referenceTable, referenceColumn);
        }

        /// <summary>
        /// Get a sql statement to rename a column and set new column definition
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="oldColumnName">old column name</param>
        /// <param name="newColumnName">new column name</param>
        /// <param name="column">column definition</param>
        /// <returns></returns>
        public virtual string ColumnRenameSql(string tableName, string oldColumnName, string newColumnName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} change column {1} {2} {3}{4}", 
                tableName, oldColumnName, newColumnName, ColumnDefinition(column)
                , SqlCommandSeparator);
        }

        /// <summary>
        /// Get a sql statement to reset column definition
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="columnName">column name</param>
        /// <param name="column">Column new definition</param>
        /// <returns></returns>
        public virtual string ColumnModifySql(string tableName, string columnName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} modify column {1} {2}{3}", 
                tableName, columnName, ColumnDefinition(column)
                , SqlCommandSeparator);
        }

        /// <summary>
        /// Get a sql statement to add a new column
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="columnName">new column name</param>
        /// <param name="column">column definition</param>
        /// <returns></returns>
        public virtual string ColumnAddSql(string tableName, string columnName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} add column {1} {2}{3}", 
                tableName, columnName, ColumnDefinition(column)
                , SqlCommandSeparator);
        }

        /// <summary>
        /// Convert an object value to database native literals.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract string ToNativeValue(object value);

        /// <summary>
        /// Transfer C# DbType to native database column type name.
        /// </summary>
        /// <param name="dbType">DbType name</param>
        /// <param name="size">size of character or number precision (total number of digits)</param>
        /// <param name="scale">number scale (digits to the right of the decimal point)</param>
        /// <returns>Native database column type name.</returns>
        public abstract string ToNativeDBType(string dbType, int size, int scale);

        public virtual string ToSqlCondition(string columnName, string op, object value)
        {
            return string.Format("{0} {1} {2}", columnName, op, ToNativeValue(value));
        }

        /// <summary>
        /// Create a new table schema
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public virtual string TableCreateSql(SqlDDLTable table)
        {
            var builder = new StringBuilder();

            //Build creating table statement
            builder.Append("create table ");
            builder.Append(table.TableName);
            builder.AppendLine(" (");
            int totalCount = table.Columns.Count;
            for (int i = 0; i < totalCount; i++)
            {
                var column = table.Columns[i];
                //build column
                builder.AppendFormat(" {0} {1}", column.Name, ColumnDefinition(column));

                if (i != totalCount - 1)
                {
                    builder.AppendLine(",");
                }
                else
                {
                    //It is a last column:
                }
            }
            builder.Append(TableCreateSqlAddition(table));//add aditional clause
            builder.AppendFormat("){0}\n", SqlCommandSeparator);

            //Build index creation statements
            foreach (var index in table.Indexes)
            {
                bool skipIndex = false;
                bool isUniqueIndex = false;

                var column = table.Columns.SingleOrDefault(x => x.Name == index.Key);
                if (column != null && column.IsUnique)
                {
                    if (AutoUniqueIndex)
                    {
                        skipIndex = true;
                    }
                    isUniqueIndex = true;
                }


                if(!skipIndex)
                {
                    builder.AppendFormat(string.Format("{0}", CreateIndexSql(index.Key, table.TableName, index.Value, isUniqueIndex)));
                }
            }

            //Build additional table creation statements
            builder.Append(TableCreateSqlAfter(table));
            return builder.ToString();
        }

        /// <summary>
        /// Get create index sql statement
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="tableName"></param>
        /// <param name="indexValue"></param>
        /// <param name="isUnique"></param>
        /// <returns></returns>
        public virtual string CreateIndexSql(string indexName, string tableName, string indexValue, bool isUnique=false)
        {
            if (isUnique)
            {
                return string.Format("create unique index {0} on {1} ({2}){3}\n", indexName, tableName, indexValue
                    , SqlCommandSeparator);
            }
            return string.Format("create index {0} on {1} ({2}){3}\n", indexName, tableName, indexValue
                ,SqlCommandSeparator);
        }

        /// <summary>
        /// Add additional clause in end of table creation statement
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        /// <remarks>It is useful to add additional statements in end of table creation sql statement</remarks>
        public virtual string TableCreateSqlAddition(SqlDDLTable table)
        {
            return "";
        }

        /// <summary>
        /// Add additional sql statements after table creation statement
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        /// <remarks>
        /// It is useful to create a trigger for oracle PK column auto_increment
        /// </remarks>
        public virtual string TableCreateSqlAfter(SqlDDLTable table)
        {
            return "";
        }


        /// <summary>
        /// Get table update statement.
        /// </summary>
        /// <returns>table update statement</returns>
        /// <param name="table">Table.</param>
        public virtual string TableUpdateSql(SqlDDLTable table)
        {
            var builder = new StringBuilder();

            foreach (var column in table.Columns)
            {
                if (column.IsDirty)
                {
                    if (column.PreviousColumn == null)
                    {
                        var dbType = ToNativeDBType(column.DbType, column.Size, column.Scale);
                        //add new column
                        builder.Append(ColumnAddSql(table.TableName, column.Name, column));
                    }
                    else if (column.Name == column.PreviousColumn.ColumnName)
                    {
                        //modify column
                        builder.Append(ColumnModifySql(table.TableName, column.Name, column));
                    }
                    else
                    {
                        //rename
                        builder.Append(ColumnRenameSql(table.TableName, column.PreviousColumn.ColumnName, column.Name, column));
                    }
                }
            }

            return builder.ToString();
        }


        /// <summary>
        /// Build column definition statement
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public virtual string ColumnDefinition(SqlDDLColumn column)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat(" {0}", ToNativeDBType(column.DbType, column.Size, column.Scale));


            if (column.DefaultValue != null && column.DefaultValue.ToString() != "")
            {
                builder.AppendFormat(" {0}", ColumnDefaultKeyword(ToNativeValue(column.DefaultValue)));
            }

            if (!column.AllowNull)
            {
                builder.Append(" not null");
            }

            if (column.IsUnique)
            {
                builder.Append(" unique");
            }

            if (column.IsPK)
            {
                builder.Append(" primary key");
            }


            //Add constraint
            if (!string.IsNullOrEmpty(column.CheckConstraint))
            {
                builder.Append(" ");
                builder.Append(column.CheckConstraint);
            }

            if (column.AutoIncrease)
            {
                builder.AppendFormat(" {0}", ColumnAutoIncrementKeyword);
            }

            if (!string.IsNullOrEmpty(column.Reference))
            {
                var foreignKeyReference = column.Reference.Split(':');
                if (foreignKeyReference.Length != 2)
                {
                    throw new FormatException(string.Format("Invalid foreign key reference {0} for column {1}", column.Reference, column.Name));
                }
                builder.AppendFormat(" {0}", ColumnReferenceKeyword(foreignKeyReference[0], foreignKeyReference[1]));
            }
            return builder.ToString();
        }




        /// <summary>
        /// Transfer C# DbType to native database column type name.
        /// </summary>
        /// <param name="dbType">DbType name</param>
        /// <param name="size">size of character or number precision (total number of digits)</param>
        /// <param name="scale">number scale (digits to the right of the decimal point)</param>
        /// <returns>Native database column type name.</returns>
        string ToNativeDBType(System.Data.DbType dbType, int size, int scale)
        {
            return ToNativeDBType(dbType.ToString(), size, scale);
        }

        /// <summary>
        /// Get last non faltal error message
        /// </summary>
        public string LastErrorMessage
        {
            get; set;
        }

    }
}

