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
		public SqlDDLColumn()
		{
            //set default values
            Size = 0;//char size or number precision
            Scale = 0; //number scale
        }
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
		public string Reference { get; set; }
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

		List<string> _columnNameHistory;
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
        //public bool IsDefaultChanged { get; set; }
        public bool NeedModifyType { get; set; }
        //public bool IsNullChanged { get; set; }
        //public bool IsConstraintChanged { get; set; }
        public bool IsReferenceChanged { get; set; }
        public bool IsNew { get; set; }
        public bool NeedRename { get; set; }
        public bool NeedAddAutoIncrease { get; set; }
        public bool NeedRemoveAutoIncrease { get; set; }
        public bool NeedAddIndex { get; set; }
        public bool NeedRemoveIndex { get; set; }
        public bool NeedAddUnique { get; set; }
        public bool NeedRemoveUnique { get; set; }
        public bool NeedAddPK { get; set; }
        public bool NeedRemovePK { get; set; }
        public bool NeedRemoveDefault { get; set; }
        public bool NeedAddDefault { get; set; }
        public bool NeedModifyDefault { get; set; }
        public string TempColumnName { get; set; }
        public bool NeedNull { get; internal set; }
        public bool NeedNotNull { get; internal set; }
        public bool NeedRemoveConstraint { get; internal set; }
        public bool NeedAddConstraint { get; internal set; }
        public bool NeedModifyConstraint { get; internal set; }
        public bool NeedRemoveReference { get; internal set; }
        public bool NeedAddReference { get; internal set; }
        public bool NeedModifyReference { get; internal set; }
    }
}
