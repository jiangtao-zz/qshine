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
        public SqlDialectStandard(string connectionString){}

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
        /// Get a keyword from column for Foreign key references.
        /// </summary>
        /// <param name="reference">format of column reference constraint.
        /// The format could be:
        ///     otherTableName:Column.
        /// or 
        ///     otherTableName(column)
        /// or
        ///     references otherTableName(column)
        /// </param>
        /// <param name="referenceColumn">foreign key table column</param>
        /// <returns></returns>
        public virtual string ColumnReferenceKeyword(string reference)
        {
            var formattedReference = reference;
            var foreignKeyReference = reference.Split(':');
            if (foreignKeyReference.Length == 2)
            {
                formattedReference = string.Format("references {0}({1})", foreignKeyReference[0], foreignKeyReference[1]);
            }
            else if(reference.StartsWith("references",StringComparison.InvariantCultureIgnoreCase))
            {
                formattedReference = reference;
            }
            else
            {
                formattedReference = string.Format("references {0}", reference);
            }


            return formattedReference;
        }

        /// <summary>
        /// Get a SQL clause to rename a table 
        /// </summary>
        /// <param name="oldTableName">table name to be changed</param>
        /// <param name="newTableName">new table name</param>
        /// <returns>return rename table statement ex:"rename table [oldtable] to [newtable]"</returns>
        public virtual string TableRenameClause(string oldTableName, string newTableName)
        {
            return FormatCommandSqlLine("rename table {0} to {1}",
                oldTableName, newTableName);
        }

        /// <summary>
        /// Get a sql clause to rename a column and set new column definition
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="oldColumnName">old column name</param>
        /// <param name="newColumnName">new column name</param>
        /// <param name="column">column definition</param>
        /// <returns></returns>
        public virtual string ColumnRenameClause(string tableName, string oldColumnName, string newColumnName, SqlDDLColumn column)
        {
            return FormatCommandSqlLine("alter table {0} rename column {1} {2}", 
                tableName, oldColumnName, newColumnName);
        }

        /// <summary>
        /// Get a sql clause to add a new column
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="columnName">new column name</param>
        /// <param name="column">column definition</param>
        /// <returns></returns>
        public virtual string ColumnAddClause(string tableName, string columnName, SqlDDLColumn column)
        {
            return FormatCommandSqlLine("alter table {0} add column {1} {2}", 
                tableName, columnName, ColumnDefinition(column));
        }

        /// <summary>
        /// Get a sql clause to change column data type
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public virtual string ColumnChangeTypeClause(string tableName, string columnName, SqlDDLColumn column)
        {
            return ColumnModifyClause(tableName, columnName, ToNativeDBType(column.DbType, column.Size, column.Scale));
        }

        public virtual string ColumnAddPKClause(string tableName, string name, SqlDDLColumn column)
        {
            return
                FormatCommandSqlLine("alter table {0} add primary key ({1})",
                tableName,
                name
                );
        }

        public virtual string ColumnRemovePKClause(string tableName, string name, SqlDDLColumn column)
        {
            return
                FormatCommandSqlLine("alter table {0} drop primary key",
                tableName
                );
        }

        public virtual string ColumnAddUniqueClause(string tableName, string name, SqlDDLColumn column)
        {
            return
                FormatCommandSqlLine("alter table {0} add unique({1})",
                tableName,
                name
                );
        }

        public virtual string ColumnRemoveUniqueClause(string tableName, string name, SqlDDLColumn column)
        {
            return
                FormatCommandSqlLine("alter table {0} drop unique({1})",
                tableName,
                name
                );
        }

        public virtual string ColumnAddIndexClause(string tableName, string name, SqlDDLColumn column)
        {
            return CreateIndexClause(new SqlDDLIndex
            {
                IndexColumns = name,
                IndexName = SqlDDLTable.GetIndexName(tableName, column),
                IsUnique = column.IsUnique,
                TableName = tableName
            });
        }

        public virtual string ColumnRemoveIndexClause(string tableName, string name, SqlDDLColumn column)
        {
            var indexName = SqlDDLTable.GetIndexName(tableName, column);

            return
                FormatCommandSqlLine("drop index {0}",
                indexName);
        }

        public virtual string ColumnModifyReferenceClause(string tableName, string name, SqlDDLColumn column)
        {
            var removeSql = ColumnRemoveReferenceClause(tableName, name, column);
            var addSql = ColumnAddReferenceClause(tableName, name, column);

            return removeSql + addSql;

        }

        public virtual string ColumnAddReferenceClause(string tableName, string name, SqlDDLColumn column)
        {
            var foreignKey = SqlDDLTable.GetforeignKeyName(tableName, column.InternalId);

            return
                FormatCommandSqlLine("alter table {0} add constraint {1} foreign key({2}) references {3}",
                tableName, foreignKey, name, column.Reference);
        }

        public virtual string ColumnRemoveReferenceClause(string tableName, string name, SqlDDLColumn column)
        {
            var foreignKey = SqlDDLTable.GetforeignKeyName(tableName, column.InternalId);

            return
                FormatCommandSqlLine("alter table {0} drop constraint {1}",
                tableName, foreignKey);
        }

        public virtual string ColumnModifyConstraintClause(string tableName, string name, SqlDDLColumn column)
        {
            var removeSql = ColumnRemoveConstraintClause(tableName, name, column);
            var addSql = ColumnAddConstraintClause(tableName, name, column);

            return removeSql + addSql;
        }

        public virtual string ColumnAddConstraintClause(string tableName, string name, SqlDDLColumn column)
        {
            var checkConstraintName = SqlDDLTable.GetCheckConstraintName(tableName, column.InternalId);

            return
                FormatCommandSqlLine("alter table {0} add constraint check({1})",
                tableName, checkConstraintName, column.CheckConstraint);
        }

        public virtual string ColumnRemoveConstraintClause(string tableName, string name, SqlDDLColumn column)
        {
            var checkConstraintName = SqlDDLTable.GetCheckConstraintName(tableName, column.InternalId);

            return
                FormatCommandSqlLine("alter table {0} drop constraint {1}",
                tableName, checkConstraintName);
        }

        public virtual string ColumnNotNullClause(string tableName, string name, SqlDDLColumn column)
        {
            return ColumnModifyClause(tableName, name, "not null");
        }

        public virtual string ColumnNullClause(string tableName, string name, SqlDDLColumn column)
        {
            return ColumnModifyClause(tableName, name, "null");
        }

        public virtual string ColumnModifyDefaultClause(string tableName, string name, SqlDDLColumn column)
        {
            return ColumnModifyClause(tableName, name, ColumnDefaultKeyword(column.DefaultValue.ToString()));
        }

        public virtual string ColumnAddDefaultClause(string tableName, string name, SqlDDLColumn column)
        {
            return ColumnModifyClause(tableName, name, ColumnDefaultKeyword(column.DefaultValue.ToString()));
        }

        public virtual string ColumnRemoveDefaultClause(string tableName, string name, SqlDDLColumn column)
        {
            return ColumnModifyClause(tableName, name, ColumnDefaultKeyword("null"));
        }

        public virtual string ColumnAddAutoIncrementClause(string tableName, string name, SqlDDLColumn column)
        {
            throw new NotImplementedException();
        }

        public virtual string ColumnRemoveAutoIncrementClause(string tableName, string name, SqlDDLColumn column)
        {
            throw new NotImplementedException();
        }

        private string ColumnModifyClause(string tableName, string columnName, string columnProperties)
        {
            return FormatCommandSqlLine("alter table {0} modify ({1} {2})",
                tableName, columnName, columnProperties);
        }

        /// <summary>
        /// Get create index sql statement
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="tableName"></param>
        /// <param name="indexValue"></param>
        /// <param name="isUnique"></param>
        /// <returns></returns>
        public virtual string CreateIndexClause(SqlDDLIndex index)
        {
            if (index.IsUnique)
            {
                return FormatCommandSqlLine("create unique index {0} on {1} ({2})", index.IndexName, index.TableName, index.IndexColumns);
            }
            return string.Format("create index {0} on {1} ({2})", index.IndexName, index.TableName, index.IndexColumns);
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
        public virtual List<string> TableCreateSqls(SqlDDLTable table)
        {
            List<string> sqls = new List<string>();

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
            builder.Append(")");

            sqls.Add(builder.ToString());
            builder.Clear();

            //Build index creation statements
            foreach (var index in table.Indexes)
            {
                bool skipIndex = false;

                if (index.Value.IsUnique && AutoUniqueIndex)
                {
                        skipIndex = true;
                }

                if(!skipIndex)
                {
                    sqls.Add(CreateIndexClause(index.Value));
                }
            }

            //Build additional table creation statements
            var sql = TableCreateSqlAfter(table);
            if (!string.IsNullOrEmpty(sql))
            {
                sqls.Add(sql);
            }
            return sqls;
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


        string GetTempColumnName()
        {
            long n = new Random().Next(100000, 100000000);
            return "TMP" + n;
        }

        /// <summary>
        /// Get table update statement.
        /// </summary>
        /// <returns>table update statement</returns>
        /// <param name="table">Table.</param>
        public virtual List<string> TableUpdateSqls(SqlDDLTable table)
        {
            var sqls = new List<string>();

            //Add new column first
            foreach (var column in table.Columns)
            {
                if (column.IsDirty)
                {
                    if (column.IsNew)
                    {
                        var dbType = ToNativeDBType(column.DbType, column.Size, column.Scale);
                        //add new column
                        sqls.Add(ColumnAddClause(table.TableName, column.Name, column));
                    }
                }
            }

            //var builder = new StringBuilder();
            //Rename columns
            //if there is column name conflict we need resolve it first before rename it back.
            var renameColumns = table.Columns.Where(x => x.IsDirty && !x.IsNew && x.NeedRename);
            if(renameColumns.Count()> 0)
            {
                foreach(var column in renameColumns)
                {
                    var conflictColumn = renameColumns.SingleOrDefault(x => x.PreviousColumn.ColumnName == column.Name);
                    //If rename column already exists, rename it to temp name first
                    if (conflictColumn != null)
                    {
                        var tempName = GetTempColumnName();
                        column.TempColumnName = tempName;
                        sqls.Add(ColumnRenameClause(table.TableName, column.PreviousColumn.ColumnName, tempName, column));
                    }
                    else
                    {
                        sqls.Add(ColumnRenameClause(table.TableName, column.PreviousColumn.ColumnName, column.Name, column));
                    }
                }

                //If temp column name existing rename to actual column name
                foreach (var column in renameColumns)
                {
                    if(!string.IsNullOrEmpty(column.TempColumnName))
                    {
                        sqls.Add(ColumnRenameClause(table.TableName, column.TempColumnName, column.Name, column));
                    }
                }
            }

            //Update attribute
            foreach (var column in table.Columns)
            {
                if (column.IsDirty && !column.IsNew)
                {
                    if(column.NeedModifyType)
                    {
                        sqls.Add(ColumnChangeTypeClause(table.TableName, column.Name, column));
                    }

                    if(column.NeedRemoveAutoIncrease)
                    {
                        sqls.Add(ColumnRemoveAutoIncrementClause(table.TableName, column.Name, column));
                    }
                    else if (column.NeedAddAutoIncrease)
                    {
                        sqls.Add(ColumnAddAutoIncrementClause(table.TableName, column.Name, column));
                    }

                    if (column.NeedRemoveDefault)
                    {
                        sqls.Add(ColumnRemoveDefaultClause(table.TableName, column.Name, column));
                    }
                    else if (column.NeedAddDefault)
                    {
                        sqls.Add(ColumnAddDefaultClause(table.TableName, column.Name, column));
                    }
                    else if (column.NeedModifyDefault)
                    {
                        sqls.Add(ColumnModifyDefaultClause(table.TableName, column.Name, column));
                    }


                    if (column.NeedNull)
                    {
                        sqls.Add(ColumnNullClause(table.TableName, column.Name, column));
                    }
                    else if (column.NeedNotNull)
                    {
                        sqls.Add(ColumnNotNullClause(table.TableName, column.Name, column));
                    }


                    if (column.NeedRemoveConstraint)
                    {
                        sqls.Add(ColumnRemoveConstraintClause(table.TableName, column.Name, column));
                    }
                    else if (column.NeedAddConstraint)
                    {
                        sqls.Add(ColumnAddConstraintClause(table.TableName, column.Name, column));
                    }
                    else if (column.NeedModifyConstraint)
                    {
                        sqls.Add(ColumnModifyConstraintClause(table.TableName, column.Name, column));
                    }

                    if (column.NeedRemoveReference)
                    {
                        sqls.Add(ColumnRemoveReferenceClause(table.TableName, column.Name, column));
                    }
                    else if (column.NeedAddReference)
                    {
                        sqls.Add(ColumnAddReferenceClause(table.TableName, column.Name, column));
                    }
                    else if (column.NeedModifyReference)
                    {
                        sqls.Add(ColumnModifyReferenceClause(table.TableName, column.Name, column));
                    }

                    if (column.NeedRemoveIndex)
                    {
                        sqls.Add(ColumnRemoveIndexClause(table.TableName, column.Name, column));
                    }
                    else if (column.NeedAddIndex)
                    {
                        sqls.Add(ColumnAddIndexClause(table.TableName, column.Name, column));
                    }

                    if (column.NeedRemoveUnique && !column.IsPK)
                    {
                        sqls.Add(ColumnRemoveUniqueClause(table.TableName, column.Name, column));
                    }
                    else if (column.NeedAddUnique && !column.IsPK)
                    {
                        sqls.Add(ColumnAddUniqueClause(table.TableName, column.Name, column));
                    }

                    if (column.NeedRemovePK)
                    {
                        sqls.Add(ColumnRemovePKClause(table.TableName, column.Name, column));
                    }
                    else if (column.NeedAddPK)
                    {
                        sqls.Add(ColumnAddPKClause(table.TableName, column.Name, column));
                    }

                }
            }

            return sqls;
        }

        public virtual string GetPKConstraintName(string tableName, string columnName)
        {
            return string.Format("{0}_PK", tableName);
        }

        public string FormatCommandSqlLine(string format, params object[] parameters)
        {
            return string.Format(format, parameters);
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
                builder.AppendFormat(" {0}", ColumnReferenceKeyword(column.Reference));
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

        /// <summary>
        /// Analyse the table structure and get table and column modified information
        /// </summary>
        /// <returns>true, if the table structure changed</returns>
        /// <param name="table">Table structure.</param>
        /// <param name="trackingTable">Tracking table information.</param>
        public virtual bool AnalyseTableChange(SqlDDLTable table, TrackingTable trackingTable)
        {
            if (trackingTable == null || trackingTable.Columns == null)
            {
                //indicates no tracking information found
                return false;
            }

            List<SqlDDLColumn> newColumns = new List<SqlDDLColumn>();
            Dictionary<string, string> mapColumns = new Dictionary<string, string>();

            bool isTableChanged = false;

            foreach (var column in table.Columns)
            {
                var trackingColumn = trackingTable.Columns.SingleOrDefault(x => x.ColumnName == column.Name);
                if (trackingColumn == null)
                {
                    if (column.ColumnNameHistory == null || column.ColumnNameHistory.Count == 0)
                    {
                        //add new column, most case
                        column.IsDirty = true;
                        column.IsNew = true;
                        isTableChanged = true;
                    }
                    else //rename
                    {
                        //found previous column from tracking table.
                        //Note: The rename statement need resolve column name conflict in later process
                        var previousColumn = trackingTable.Columns.SingleOrDefault(x => column.ColumnNameHistory.Contains(x.ColumnName));
                        if (previousColumn != null)
                        {
                            trackingColumn = previousColumn;
                            column.IsDirty = true;
                            column.NeedRename = true;
                            isTableChanged = true;
                            column.PreviousColumn = trackingColumn;
                        }
                        else
                        {
                            //unexpected, previous column is not in tracking list
                            throw new Exception(String.Format("Rename column name {0}-{1} is not in column tracking table", column.Name, column.ColumnNameHistory));
                        }
                    }
                }
                if (trackingColumn != null)
                {
                    if (AnalyseColumn(column, trackingColumn))
                    {
                        isTableChanged = true;
                        column.PreviousColumn = trackingColumn;
                    }
                }
            }
            return isTableChanged;
        }

        bool AnalyseColumn(SqlDDLColumn column, TrackingColumn trackingColumn)
        {
            if (column.Version > trackingColumn.Version)
            {
                if (column.AutoIncrease != trackingColumn.AutoIncrease)
                {
                    if (column.AutoIncrease)
                    {
                        column.NeedAddAutoIncrease = true;
                    }
                    else
                    {
                        column.NeedRemoveAutoIncrease = true;
                    }
                    column.IsDirty = true;
                }

                if(column.AllowNull != trackingColumn.AllowNull)
                {
                    if (column.AllowNull)
                    {
                        column.NeedNull = true;
                    }
                    else
                    {
                        column.NeedNotNull = true;
                    }
                    column.IsDirty = true;

                }

                if (column.CheckConstraint != trackingColumn.CheckConstraint)
                {
                    if(string.IsNullOrEmpty(column.CheckConstraint))
                    {
                        column.NeedRemoveConstraint = true;
                    }
                    else if (string.IsNullOrEmpty(trackingColumn.CheckConstraint))
                    {
                        column.NeedAddConstraint = true;
                    }
                    else
                    {
                        column.NeedModifyConstraint = true;
                    }
                    column.IsDirty = true;

                }

                if (column.Reference != trackingColumn.Reference)
                {
                    if (string.IsNullOrEmpty(column.Reference))
                    {
                        column.NeedRemoveReference = true;
                    }
                    else if (string.IsNullOrEmpty(trackingColumn.Reference))
                    {
                        column.NeedAddReference = true;
                    }
                    else
                    {
                        column.NeedModifyReference = true;
                    }
                    column.IsDirty = true;

                }

                if (column.IsIndex != trackingColumn.IsIndex)
                {
                    if (column.IsIndex)
                    {
                        column.NeedAddIndex = true;
                    }
                    else
                    {
                        column.NeedRemoveIndex = true;
                    }
                    column.IsDirty = true;
                }

                if (column.IsUnique != trackingColumn.IsUnique)
                {
                    if (column.IsUnique)
                    {
                        column.NeedAddUnique = true;
                    }
                    else
                    {
                        column.NeedRemoveUnique = true;
                    }
                    column.IsDirty = true;
                }

                if (column.IsPK != trackingColumn.IsPK)
                {
                    if (column.IsPK)
                    {
                        //change column to PK. Only single column can be PK.
                        //It should only add PK to the table which doesn't have PK original
                        column.NeedAddPK = true;
                        //The primary key always be a unique key
                        column.NeedAddUnique = false;
                        column.NeedRemoveUnique = false;
                    }
                    else
                    {
                        //Remove PK constaint
                        //Take caution to remove PK. PK constaint will automatically create unique index.
                        //Remove PK will not remove index automatically.
                        column.NeedRemovePK = true;
                    }
                    column.IsDirty = true;
                }

                string stringValue = Convert.ToString(column.DefaultValue);
                if (!stringValue.AreEqual(trackingColumn.DefaultValue))
                {
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        column.NeedRemoveDefault = true;
                    }
                    else if (string.IsNullOrEmpty(trackingColumn.DefaultValue))
                    {
                        column.NeedAddDefault = true;
                    }
                    else
                    {
                        column.NeedModifyDefault = true;
                    }
                    column.IsDirty = true;
                }

                if (ToNativeDBType(column.DbType.ToString(), column.Size, column.Scale) != ToNativeDBType(trackingColumn.ColumnType, column.Size, column.Scale))
                {
                    column.NeedModifyType = true;
                    column.IsDirty = true;
                }

                return column.IsDirty;
            }
            return false;
        }

    }
}

