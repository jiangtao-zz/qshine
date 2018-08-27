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
    public abstract class SqlDDLSyntaxBase : ISqlDDLSyntax
    {
        /// <summary>
        /// Construct a database instance by connectionStrings setting
        /// </summary>
        /// <param name="connectionStringName"></param>
        public SqlDDLSyntaxBase(string connectionStringName)
        {
            //_logger = Log.GetLogger("database");

            Database = new Database(connectionStringName);

            if (Database.ProviderName != ProviderName)
            {
                LastErrorMessage = String.Format("The sql provider {0} doesn't support connection string {1}", ProviderName, Database.ProviderName);

                throw new InvalidProviderException(LastErrorMessage);
            }
        }

        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        /// <value>The name of the provider.</value>
        public abstract string ProviderName
        {
            get;
        }

        /// <summary>
        /// Get connection string
        /// </summary>
        public Database Database
        {
            get;
            private set;
        }


        /// <summary>
        /// named parameter prefix symbol
        /// </summary>
        public virtual string ParameterPrefix
        {
            get
            {
                return "@";
            }
        }

        /// <summary>
        /// Get last non faltal error message
        /// </summary>
        public string LastErrorMessage
        {
            get; set;
        }

        /// <summary>
        /// Creates a new database instance.
        /// </summary>
        /// <returns><c>true</c>, if database was created, <c>false</c> otherwise.</returns>
        public abstract bool CreateDatabase();

        /// <summary>
        /// Check whether the database instance exists.
        /// </summary>
        /// <returns><c>true</c>, if database exists, <c>false</c> otherwise.</returns>
        public abstract bool IsDatabaseExists();

        /// <summary>
        /// Can a new database instance be created.
        /// Defualt is true
        /// </summary>
        public virtual bool CanCreate
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Sql statement for table name check
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public abstract string GetTableNameStatement(string tableName);

        /// <summary>
        /// Get Sql statement to rename a table
        /// </summary>
        /// <param name="oldTableName">old table name</param>
        /// <param name="newTableName">new table name</param>
        /// <returns></returns>
        public virtual string GetRenameTableStatement(string oldTableName, string newTableName)
        {
            return string.Format("rename table {0} to {1}", oldTableName, newTableName);
        }


        /// <summary>
        /// Get a keyword of auto increase in create table statement 
        /// </summary>
        public virtual string ColumnAutoIncrementKeyword
        {
            get { return "auto_increment"; }
        }

        /// <summary>
        /// Get a keyword of default 
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public virtual string ColumnDefaultKeyword(string defaultValue)
        {
            return string.Format("default ({0})", defaultValue);
        }

        /// <summary>
        /// Get a keyword of Foreign key in create table statement.
        /// </summary>
        /// <param name="referenceTable"></param>
        /// <param name="referenceColumn"></param>
        /// <returns></returns>
        public virtual string ColumnReferenceKeyword(string referenceTable, string referenceColumn)
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
        public virtual string GetRenameColumnNameStatement(string tableName, string oldColumnName, string newColumnName, SqlDDLColumn column)
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
        public virtual string GetModifyColumnStatement(string tableName, string columnName, SqlDDLColumn column)
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
        public virtual string GetAddColumnStatement(string tableName, string columnName, SqlDDLColumn column)
        {
            return string.Format("alter table {0} add column {1} {2};", tableName, columnName, ColumnDefinition(column));
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
        /// <param name="size">size of character</param>
        /// <returns>Native database column type name.</returns>
        public abstract string ToNativeDBType(string dbType, int size);

        /// <summary>
        /// Create a new table schema
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public virtual string GetCreateTableStatement(SqlDDLTable table)
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
            builder.AppendLine(");");

            //Build index creation statements
            foreach (var index in table.Indexes)
            {
                builder.AppendFormat(string.Format("create index {0} on {1} ({2});\n", index.Key, table.TableName, index.Value));
            }
            return builder.ToString();
        }

        /// <summary>
        /// Get table update statement.
        /// </summary>
        /// <returns>table update statement</returns>
        /// <param name="table">Table.</param>
        public virtual string GetUpdateTableStatement(SqlDDLTable table)
        {
            var builder = new StringBuilder();

            foreach (var column in table.Columns)
            {
                if (column.IsDirty)
                {
                    if (column.PreviousColumn == null)
                    {
                        var dbType = ToNativeDBType(column.DbType, column.Size);
                        //add new column
                        builder.Append(GetAddColumnStatement(table.TableName, column.Name, column));
                    }
                    else if (column.Name == column.PreviousColumn.ColumnName)
                    {
                        //modify column
                        builder.Append(GetModifyColumnStatement(table.TableName, column.Name, column));
                    }
                    else
                    {
                        //rename
                        builder.Append(GetRenameColumnNameStatement(table.TableName, column.PreviousColumn.ColumnName, column.Name, column));
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

            builder.AppendFormat(" {0}", ToNativeDBType(column.DbType, column.Size));

            if (column.IsPK)
            {
                builder.Append(" primary key");
            }

            if (!column.AllowNull)
            {
                builder.Append(" not null");
            }

            if (column.IsUnique)
            {
                builder.Append(" unique");
            }

            if (column.DefaultValue != null && column.DefaultValue.ToString() != "")
            {
                builder.AppendFormat(" {0}", ColumnDefaultKeyword(ToNativeValue(column.DefaultValue)));
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
        /// <param name="size">size of character</param>
        /// <returns>Native database column type name.</returns>
        string ToNativeDBType(System.Data.DbType dbType, int size)
        {
            return ToNativeDBType(dbType.ToString(), size);
        }

    }
}

