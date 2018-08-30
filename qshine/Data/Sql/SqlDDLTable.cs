﻿using System;
using System.Collections.Generic;
using System.Data;
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
		Dictionary<string, string> _indexes;
		int _indexSequence = 1;
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
			_indexes = new Dictionary<string, string>();
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
		public SqlDDLTable AddPKColumn(string columnName, DbType dbType, int size=1, bool allowNull = false, string defaultValue="", bool autoIncrease=true, int version=1, 
		                               params string[] oldColumnNames)
		{
			_pkColumn = new SqlDDLColumn
			{
				Name = columnName,
				DbType = dbType,
				Size = size,
				DefaultValue = defaultValue,
				IsPK = true,
				AllowNull = allowNull,
				AutoIncrease = autoIncrease,
				Comments = "PK",
				Version = version
			};
			foreach (var oldColumn in oldColumnNames)
			{
				_pkColumn.ColumnNameHistory.Add(oldColumn);
			}
			_columns.Add(_pkColumn);
			return this;
		}

		public SqlDDLTable AddColumn(string columnName, DbType dbType, int size, bool allowNull = true, object defaultValue = null, string comments = "",
		                             string checkConstraint = "", bool isUnique = false, string reference = "", bool isIndex = false, int version=1,
		                             params string[] oldColumnNames)
		{
			var column = new SqlDDLColumn
			{
				Name = columnName,
				DbType = dbType,
				Size = size,
				DefaultValue = defaultValue,
				IsUnique = isUnique,
				AllowNull = allowNull,
				AutoIncrease = false,
				Comments = comments,
				CheckConstraint = checkConstraint,
				Reference = reference,
				IsIndex = isIndex,
				Version = version
			};
			foreach (var oldColumn in oldColumnNames)
			{
				_pkColumn.ColumnNameHistory.Add(oldColumn);
			}

			_columns.Add(column);
			if (column.IsIndex)
			{
				AddIndex(columnName);
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
			AddColumn("created_on", DbType.DateTime, 0,false, SqlReservedWord.SysDate);
            AddColumn("updated_by", DbType.String, 100);
			AddColumn("updated_on", DbType.DateTime, 0, false, SqlReservedWord.SysDate);
			AddIndex("created_by", GetName("ixa", 1));
			AddIndex("updated_by", GetName("ixa", 2));
            AddIndex("created_on", GetName("ixa", 3));
			AddIndex("updated_on", GetName("ixa", 4));

			return this;
		}

		public SqlDDLTable AddIndex(string columnNames, string indexName = "")
		{
			string name = indexName;
			if (string.IsNullOrEmpty(name))
			{
				name = AutoIndex("inx");
			}
			if (!_indexes.ContainsKey(name))
			{
				_indexes.Add(name, columnNames);
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

		public Dictionary<string, string> Indexes
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

		private string AutoIndex(string suffix)
		{
			return GetName(suffix, _indexSequence++);
		}


		private string GetName(string suffix, int sequence)
		{
			var name = string.Format("{0}_{1}{2}",_tableName,suffix,sequence);
			if (name.Length > 20)
			{
				var end = string.Format("_{0}{1}", suffix,sequence);
				int n = 20 - end.Length;
				name = name.Substring(0, n);
			}
			return name;
		}

	}
}