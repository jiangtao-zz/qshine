using qshine.Utility;
using System;
using System.Collections.Generic;
using System.Data;

namespace qshine.database
{
    /// <summary>
    /// Table column structure.
    /// </summary>
    /// <remarks>
    /// A column unique internal id will be created to identify each column in the table.
    /// The column internal id is the column sequence number start from 1.
    /// To keep Table structure consistency each column order should not be changed after it created if the table column use auto internal id.
    /// Otherwise, explictly define an internal id for each column.
    /// 
    /// </remarks>
	public class SqlDDLColumn
    {
        /// <summary>
        /// Table instance
        /// </summary>
        public SqlDDLTable Table {get;set; }

        /// <summary>
        /// Ctro.
        /// </summary>
        /// <param name="tableName"></param>
		public SqlDDLColumn(string tableName)
		{
            TableName = tableName;
            //set default values
            Size = 0;//char size or number precision
            Scale = 0; //number scale
        }
        /// <summary>
        /// Table name
        /// </summary>
        public string TableName { get; private set; }
        /// <summary>
        /// Column name
        /// </summary>
		public string Name { get; set; }
        /// <summary>
        /// column data type
        /// </summary>
		public DbType DbType { get; set; }
        /// <summary>
        /// Number precision or characters size
        /// </summary>
		public int Size { get; set; }
        /// <summary>
        /// Number scale
        /// </summary>
        public int Scale { get; set; }
        /// <summary>
        /// Defines column default value
        /// </summary>
        public object DefaultValue { get; set; }
        /// <summary>
        /// Indicates a null or not null constraint
        /// </summary>
		public bool AllowNull { get; set; }
        /// <summary>
        /// Column comments
        /// </summary>
		public string Comments { get; set; }
        /// <summary>
        /// Indicates a column foreign key reference constraint 
        /// </summary>
		public SqlDDLColumn Reference { get; set; }

        /// <summary>
        /// Get FK sql clause
        /// </summary>
        /// <returns></returns>
        public string ToReferenceClause()
        {
            if (Reference == null) return "";
            return string.Format("{0}:{1}", Reference.TableName, Reference.Name);
        }

        /// <summary>
        /// INdicates a unique constraint column
        /// </summary>
        public bool IsUnique { get; set; }
        /// <summary>
        /// Indicates a PK column
        /// </summary>
		public bool IsPK { get; set; }
        /// <summary>
        /// Column check constraint attribute
        /// </summary>
		public string CheckConstraint { get; set; }
        /// <summary>
        /// Indicates a auto increment id. It is usually for PK column.
        /// </summary>
		public bool AutoIncrease { get; set; }
        /// <summary>
        /// Indicates an auto index column
        /// </summary>
		public bool IsIndex { get; set; }
        /// <summary>
        /// Increase version number if any column attribute changed
        /// </summary>
		public int Version { get; set; }
        /// <summary>
        /// Internal column id to tracking column uniqueness.
        /// It can be generated automatically if no column removed and new column added sequentially.
        /// Assign a unique unmber explictly if you need remove column or re-order the column in table structure.
        /// </summary>
        public int InternalId { get; set; }
        /// <summary>
        /// A hash code to identify column data
        /// </summary>
        public long HashCode {
            get
            {
                string defaultValue = DefaultValue == null ? "" : DefaultValue.ToString();
                string reference = Reference == null ? "" : ToReferenceClause();

                return FastHash.GetHashCode(
                    Name, DbType, Size, Scale, defaultValue, AllowNull, reference, IsUnique, IsPK, CheckConstraint, AutoIncrease, IsIndex, Version
                    , InternalId
                    );
            }
        }

        List<string> _columnNameHistory;
        /// <summary>
        /// Get column name change history
        /// </summary>
		public List<string> ColumnNameHistory
		{
			get
			{
				if (_columnNameHistory == null)
				{
					_columnNameHistory = new List<string>();
				}
				return _columnNameHistory;
			}
		}

        /// <summary>
        /// Previous column information from tracking column table.
        /// </summary>
        public TrackingColumn PreviousColumn { get; set; }

        /// <summary>
        /// The Column attribute changed.
        /// </summary>
        public bool IsDirty { get; set; }
        
        //public bool IsNullChanged { get; set; }
        //public bool IsConstraintChanged { get; set; }

        /// <summary>
        /// Indicates FK changed (added or removed)
        /// </summary>
        public bool IsReferenceChanged { get; set; }

        /// <summary>
        /// Indicates a new column
        /// </summary>
        public bool IsNew { get; set; }

        #region Column Type
        /// <summary>
        /// Need issue a column type change sql
        /// </summary>
        public bool NeedModifyType { get; set; }
        #endregion

        #region Column Rename
        /// <summary>
        /// Need issue a sql to rename the column
        /// </summary>
        public bool NeedRename { get; set; }
        #endregion

        #region Column auto-increasement
        /// <summary>
        /// Need issue a sql to add column auto-increasement
        /// </summary>
        public bool NeedAddAutoIncrease { get; set; }
        /// <summary>
        /// Need issue a sql to remove column auto-increasement
        /// </summary>
        public bool NeedRemoveAutoIncrease { get; set; }
        #endregion

        #region Column Index
        /// <summary>
        /// Need issue a sql to add an index
        /// </summary>
        public bool NeedAddIndex { get; set; }
        /// <summary>
        /// Need issue a sql to remove an index
        /// </summary>
        public bool NeedRemoveIndex { get; set; }
        #endregion

        #region Column Unique Key
        /// <summary>
        /// Need issue a sql to add unique key
        /// </summary>
        public bool NeedAddUnique { get; set; }
        /// <summary>
        /// Need issue a sql to remove unique key
        /// </summary>
        public bool NeedRemoveUnique { get; set; }
        #endregion

        #region Column PK
        /// <summary>
        /// Need issue a sql to add PK.
        /// </summary>
        public bool NeedAddPK { get; set; }
        /// <summary>
        /// Need issue a sql to remove a PK.
        /// </summary>
        public bool NeedRemovePK { get; set; }
        #endregion

        #region Column Default value
        /// <summary>
        /// Need issue a sql to remove default value
        /// </summary>
        public bool NeedRemoveDefault { get; set; }
        /// <summary>
        /// Need issue a sql to add default value.
        /// </summary>
        public bool NeedAddDefault { get; set; }
        /// <summary>
        /// Need issue a sql to modify default value.
        /// </summary>
        public bool NeedModifyDefault { get; set; }
        #endregion

        #region Column Null/Not NULL
        /// <summary>
        /// Need issue a nullable column sql
        /// </summary>
        public bool NeedNull { get; internal set; }
        /// <summary>
        /// Need issue a sql to set column not null constraint.
        /// </summary>
        public bool NeedNotNull { get; internal set; }
        #endregion

        #region Column Constraint
        /// <summary>
        /// Need issue a sql to remove the column constraint.
        /// </summary>
        public bool NeedRemoveConstraint { get; internal set; }
        /// <summary>
        /// Need issue a sql to add column constraint.
        /// </summary>
        public bool NeedAddConstraint { get; internal set; }
        /// <summary>
        /// Need issue a sql to modify constraint.
        /// </summary>
        public bool NeedModifyConstraint { get; internal set; }
        #endregion

        #region Column FK clause
        /// <summary>
        /// Need issue a sql to remove Fk.
        /// </summary>
        public bool NeedRemoveReference { get; internal set; }
        /// <summary>
        /// Need issue a sql to add FK.
        /// </summary>
        public bool NeedAddReference { get; internal set; }
        /// <summary>
        /// Need issue a sql to modify FK.
        /// </summary>
        public bool NeedModifyReference { get; internal set; }
        #endregion

        /// <summary>
        /// Store a temp name
        /// </summary>
        internal string TempColumnName { get; set; }
    }
}
