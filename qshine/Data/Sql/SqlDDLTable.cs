using System.Linq;
using System.Collections.Generic;
using System.Data;
using System;

namespace qshine.database
{
	/// <summary>
	/// Sql DDL Table class.
	/// </summary>
	public class SqlDDLTable
	{
		string _tableName;
		string _tableSpace;
		string _schemaName;
		string _indexTableSpace;
		string _tableComments;
		string _tableCategory;
		List<SqlDDLColumn> _columns;
		SqlDDLColumn _pkColumn;
		Dictionary<string, SqlDDLIndex> _indexes;
		int _internalIdSequence = 1;
		int _version;
		public SqlDDLTable(string tableName, string category, string comments, string tableSpace="",string indexTableSpace="", int version=1, string schemaName="")
		{
			_tableName = tableName;
			_schemaName = schemaName;
			_tableSpace = tableSpace;
			_indexTableSpace = indexTableSpace;
			_tableComments = comments;
			_tableCategory = category;
			_columns = new List<SqlDDLColumn>();
			_indexes = new Dictionary<string, SqlDDLIndex>();
			_version = version;
		}

		/// <summary>
		/// Adds the PK Column.
		/// </summary>
		/// <param name="columnName">Column name.</param>
		/// <param name="dbType">Db type.</param>
		/// <param name="size">Size.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <param name="allowNull">If set to <c>true</c> allow null.</param>
		/// <param name="autoIncrease">Auto increase number. -1 means not auto increase</param>
		/// <returns></returns>
		public SqlDDLTable AddPKColumn(string columnName, DbType dbType, int size=1, bool allowNull = false, 
            string defaultValue="", bool autoIncrease=true, int version=1, 
            params string[] oldColumnNames)
        {
            return AddPKColumn(0, columnName, dbType, size, allowNull, defaultValue, autoIncrease, version, oldColumnNames);
        }

        public SqlDDLTable AddPKColumn(int internalId, string columnName, DbType dbType, int size = 1, bool allowNull = false,
            string defaultValue = "", bool autoIncrease = true, int version = 1,
            params string[] oldColumnNames)
        {
            return AddColumn(0, columnName, dbType, size, 0, allowNull, defaultValue, "PK", 
                                     version:version, isPK: true, autoIncrease: autoIncrease, oldColumnNames:oldColumnNames);
		}

		public SqlDDLTable AddColumn(string columnName, DbType dbType, int size, int scale=0, bool allowNull = true, object defaultValue = null, string comments = "",
		                             string checkConstraint = "", bool isUnique = false, string reference = "", bool isIndex = false, int version=1,
                                     bool isPK=false, bool autoIncrease = false, params string[] oldColumnNames)
        {
            return AddColumn(0, columnName, dbType, size, scale, allowNull, defaultValue, comments,
                                     checkConstraint, isUnique, reference, isIndex, version, isPK, autoIncrease, oldColumnNames);
        }


        public SqlDDLTable AddColumn(int internalId, string columnName, DbType dbType, int size, int scale = 0, bool allowNull = true, object defaultValue = null, string comments = "",
                                     string checkConstraint = "", bool isUnique = false, string reference = "", bool isIndex = false, int version = 1, 
                                     bool isPK=false, bool autoIncrease=false, params string[] oldColumnNames)
        {
            if (internalId == 0)
            {
                internalId = _internalIdSequence;
                _internalIdSequence++;
            }

            var c = _columns.SingleOrDefault(x => x.InternalId == internalId);
            if (c != null)
            {
                throw new InvalidExpressionException(string.Format(
                    "Defined internal id conflicts with the id of an existing column [{0}].", c.Name));
            }

            if ((isPK||isUnique) && size > 4000)
            {
                throw new InvalidConstraintException("Cannot define a big size column as a unique or primary key column.");
            }

            if (isPK && _columns.Any(x => x.IsPK))
            {
                throw new InvalidExpressionException("Only one PRIMARY key column is allow in the table.");
            }

            var column = new SqlDDLColumn(TableName)
            {
                Name = columnName,
                DbType = dbType,
                Size = size,
                Scale = scale,
                DefaultValue = defaultValue,
                IsUnique = isUnique,
                AllowNull = allowNull,
                IsPK = isPK,
                AutoIncrease = autoIncrease,
                Comments = comments,
                CheckConstraint = checkConstraint,
                Reference = reference,
                IsIndex = isIndex,
                Version = version,
                InternalId = internalId
            };

            foreach (var oldColumn in oldColumnNames)
			{
				_pkColumn.ColumnNameHistory.Add(oldColumn);
			}

			_columns.Add(column);

            if (isPK)
            {
                _pkColumn = column;
                //No not create index for PK column. It should created automatically by the database system.
            }
            else
            {
                if (column.IsIndex)
                {
                    AddIndex(columnName, GetIndexName(_tableName, column) ,isUnique);
                }
            }

            return this;			
		}

		/// <summary>
		/// Adds the audit columns.
		/// </summary>
		/// <returns>Current table instance.</returns>
		public SqlDDLTable AddAuditColumn()
		{
			AddColumn("created_by", DbType.String, 100);
			AddColumn("created_on", DbType.DateTime, 0,0,false, SqlReservedWord.SysDate);
            AddColumn("updated_by", DbType.String, 100);
			AddColumn("updated_on", DbType.DateTime, 0,0, false, SqlReservedWord.SysDate);
			AddIndex("created_by", GetName("ixa", _tableName, 1));
			AddIndex("updated_by", GetName("ixa", _tableName, 2));
            AddIndex("created_on", GetName("ixa", _tableName, 3));
			AddIndex("updated_on", GetName("ixa", _tableName, 4));

			return this;
		}

        /// <summary>
        /// Add index for given column names. The given column name
        /// </summary>
        /// <param name="columnNames">index column names separated by comma</param>
        /// <param name="indexName">index name. default index name generated by system based on the column order</param>
        /// <param name="isUnique">Indicates a unique index</param>
        /// <returns></returns>
		public SqlDDLTable AddIndex(string columnNames, string indexName, bool isUnique=false)
		{
            Check.HaveValue(indexName, "indexName");

			if (!_indexes.ContainsKey(indexName))
			{
                _indexes.Add(indexName, new SqlDDLIndex
                {
                    TableName = _tableName,
                    IndexName = indexName,
                    IndexColumns = columnNames,
                    IsUnique = isUnique
                });
			}
			return this;
		}

		Dictionary<int, string> _tableNameHistory = new Dictionary<int, string>();
		/// <summary>
		/// Gets the table name history.
		/// </summary>
		/// <value>The table name history.</value>
		public Dictionary<int, string> TableNameHistory
		{
			get
			{
				return _tableNameHistory;
			}
		}

		/// <summary>
		/// Track table name history if a table name got changed.
		/// To rename a table we need use this method to add all previous table name.
		/// </summary>
		/// <returns>current instance</returns>
		/// <param name="tableName">One of old table name.</param>
		/// <param name="version">One of old table version.</param>
		public SqlDDLTable AddOldTableName(string tableName, int version)
		{
			if (!_tableNameHistory.ContainsKey(version))
			{
				_tableNameHistory.Add(version, tableName);
			}
			return this;
		}

        /// <summary>
        /// Get possible index name
        /// </summary>
        /// <param name="column">column</param>
        /// <returns>Return a column potential index name.</returns>
        public static string GetIndexName(string tableName, SqlDDLColumn column)
        {
            return GetName("inx", tableName, column.InternalId);
        }

        /// <summary>
        /// Get possible table column unique key name.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="internalId">internal unique column id</param>
        /// <returns></returns>
        public static string GetForeignKeyName(string tableName, int internalId)
        {
            return GetName("fk", tableName, internalId); ;
        }
        
        /// <summary>
        /// Get possible table column Check constraint name.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="internalId">internal unique column id</param>
        /// <returns></returns>
        public static string GetCheckConstraintName(string tableName, int internalId)
        {
            return GetName("chk", tableName, internalId); ;
        }
        
        /// <summary>
        /// Get possible table column Unique constraint name.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="internalId">internal unique column id</param>
        /// <returns></returns>
        public static string GetUniqueKeyName(string tableName, int internalId)
        {
            return GetName("uk", tableName, internalId);
        }

        public static string GetDefaultConstraintName(string tableName , int internalId)
        {
            return GetName("df", tableName, internalId);
        }


        public void Create()
		{
		}

		public string TableName
		{
			get
			{
				return _tableName;
			}
		}

		public SqlDDLColumn PkColumn
		{
			get
			{
				return _pkColumn;
			}
		}

		public IList<SqlDDLColumn> Columns
		{
			get
			{
				return _columns;
			}
		}

		public Dictionary<string, SqlDDLIndex> Indexes
		{
			get
			{
				return _indexes;
			}
		}

		public int Version
		{
            get
			{
				return _version;
			}
		}

		public string SchemaName
		{
			get
			{
				return _schemaName;
			}
		}

		public string Comments
		{
			get
			{
				return _tableComments;
			}
		}

		public string Category
		{
			get
			{
				return _tableCategory;
			}
		}


		private static string GetName(string suffix, string tableName, int sequence)
		{
			var name = string.Format("{0}_{1}{2}", tableName, suffix,sequence);
			if (name.Length > 29)
			{
				var end = string.Format("_{0}{1}", suffix,sequence);
				int n = 29 - end.Length;
				name = name.Substring(0, n);
			}
			return name;
		}
    }
}
