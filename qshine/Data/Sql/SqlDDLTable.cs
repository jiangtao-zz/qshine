﻿using System.Linq;
using System.Collections.Generic;
using System.Data;
using System;
using qshine.Utility;

namespace qshine.database
{
	/// <summary>
	/// Sql DDL Table class.
	/// </summary>
	public class SqlDDLTable
	{
        const int MaxNameLength = 30;
        int _internalIdSequence = 1; 
		public SqlDDLTable(string tableName, string category, string comments, string tableSpace="",string indexTableSpace="", int version=1, string schemaName="")
		{
            Check.Assert<ArgumentException>(!string.IsNullOrEmpty(tableName) && tableName.Length <= MaxNameLength,
                "tableName is mandatory and length must be less than {0}", MaxNameLength);

			TableName = tableName;
            SchemaName = schemaName;
			TableSpace = tableSpace;
			IndexTableSpace = indexTableSpace;
            Comments = comments;
			Category = category;
            Columns = new List<SqlDDLColumn>();
			Indexes = new Dictionary<string, SqlDDLIndex>();
			Version = version;
            HistoryTableNames = new Dictionary<int, string>();

        }
        /// <summary>
        /// The has code identifiy table uniqueness
        /// </summary>
        /// <returns></returns>
        public long HashCode
        {
            get
            {
                long hashcode = FastHash.GetHashCode(
                    TableName,
                    SchemaName,
                    TableSpace,
                    IndexTableSpace);
                foreach(var c in Columns)
                {
                    hashcode += c.HashCode;
                }
                foreach (var c in Indexes)
                {
                    if (c.Value != null)
                    {
                        hashcode += c.Value.HashCode;
                    }
                }
                return hashcode;
            }
        }

        /// <summary>
        /// Database instance which contains all tables
        /// </summary>
        public SqlDDLDatabase Database { get; set; }
        /// <summary>
        /// Table internal unique id
        /// </summary>
        public long Id { get; set; }

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
		                             string checkConstraint = "", bool isUnique = false, SqlDDLColumn reference = null, bool isIndex = false, int version=1,
                                     bool isPK=false, bool autoIncrease = false, params string[] oldColumnNames)
        {
            return AddColumn(0, columnName, dbType, size, scale, allowNull, defaultValue, comments,
                                     checkConstraint, isUnique, reference, isIndex, version, isPK, autoIncrease, oldColumnNames);
        }


        public SqlDDLTable AddColumn(int internalId, string columnName, DbType dbType, int size, int scale = 0, bool allowNull = true, object defaultValue = null, string comments = "",
                                     string checkConstraint = "", bool isUnique = false, SqlDDLColumn reference = null, bool isIndex = false, int version = 1, 
                                     bool isPK=false, bool autoIncrease=false, params string[] oldColumnNames)
        {
            if (internalId == 0)
            {
                internalId = _internalIdSequence;
                _internalIdSequence++;
            }

            var c = Columns.SingleOrDefault(x => x.InternalId == internalId);
            if (c != null)
            {
                throw new InvalidExpressionException(string.Format(
                    "Defined internal id conflicts with the id of an existing column [{0}].", c.Name));
            }

            if ((isPK||isUnique) && size > 4000)
            {
                throw new InvalidConstraintException("Cannot define a big size column as a unique or primary key column.");
            }

            if (isPK && Columns.Any(x => x.IsPK))
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
                InternalId = internalId,
                Table = this
            };

            if (PkColumn != null)
            {
                foreach (var oldColumn in oldColumnNames)
                {
                    PkColumn.ColumnNameHistory.Add(oldColumn);
                }
            }

            Columns.Add(column);

            if (isPK)
            {
                PkColumn = column;
                //No not create index for PK column. It should created automatically by the database system.
            }
            else
            {
                if (column.IsIndex)
                {
                    AddIndex(columnName, GetIndexName(TableName, column) ,isUnique);
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
			AddIndex("created_by", GetName("ixa", TableName, 1));
			AddIndex("updated_by", GetName("ixa", TableName, 2));
            AddIndex("created_on", GetName("ixa", TableName, 3));
			AddIndex("updated_on", GetName("ixa", TableName, 4));

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

			if (!Indexes.ContainsKey(indexName))
			{
                Indexes.Add(indexName, new SqlDDLIndex
                {
                    TableName = this.TableName,
                    IndexName = indexName,
                    IndexColumns = columnNames,
                    IsUnique = isUnique
                });
			}
			return this;
		}

        Dictionary<int, string> HistoryTableNames
        {
            get;set;
        }

        /// <summary>
        /// Method to indicate table rename action for specific version.
        /// To rename a table you must call this method to specify table internal Id, previous table name and version number.
        /// </summary>
        /// <returns>current instance</returns>
        /// <param name="tableId">Internal unique table Id.</param>
        /// <param name="oldTableName">old table name.</param>
        /// <param name="version">version of old table name.</param>
        public SqlDDLTable RenameTable(long tableId, string oldTableName, int version)
		{
            Check.Assert<ArgumentException>(tableId==0 || Id == 0 || Id == tableId,
                "The argument tableId [{0}] must match to table tracking id [{1}].", tableId, Id);

            Check.Assert<ArgumentException>(version<Version,
                "The argument version [{0}] must be a history version number (<{1}).", version, Version);

            Check.Assert<ArgumentException>(!HistoryTableNames.ContainsKey(version),
                "The argument version [{0}] cannot be same as previous renamed name version.", version);

            Check.Assert<ArgumentException>(oldTableName != TableName,
                "The argument oldTableName [{0}] cannot be same as current table name {1}.", oldTableName, TableName);

            Id = tableId;
            HistoryTableNames.Add(version, oldTableName);
            IsTableRenamed = true;

			return this;
		}

        /// <summary>
        /// Indicates a table has been renamed.
        /// </summary>
        public bool IsTableRenamed { get; private set; }

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


		public string TableName
		{
            get; private set;
		}

        public string TableSpace { get; private set; }

        public string IndexTableSpace { get; private set; }


        public SqlDDLColumn PkColumn
		{
            get; private set;
		}

		public IList<SqlDDLColumn> Columns
		{
            get; private set;
		}

		public Dictionary<string, SqlDDLIndex> Indexes
		{
            get; private set;
		}

		public int Version
		{
            get; private set;
		}

		public string SchemaName
		{
            get;private set;
		}

		public string Comments
		{
            get; private set;
		}

		public string Category
		{
            get; private set;
		}


		private static string GetName(string suffix, string tableName, int sequence)
		{
			var name = string.Format("{0}{1}{2}", suffix,sequence, tableName);
			if (name.Length >= MaxNameLength)
			{
				name = name.Substring(0, MaxNameLength);
			}
			return name;
		}
    }
}
