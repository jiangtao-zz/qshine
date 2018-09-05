using System;
using System.Collections.Generic;
using System.Data;

namespace qshine.database
{
	public class SqlDDLColumn
	{
		public SqlDDLColumn()
		{
            //set default values
            Size = 0;//char size or number precision
            Scale = 0; //number scale
        }
		public string Name { get; set; }
		public DbType DbType { get; set; }
		public int Size { get; set; }
        public int Scale { get; set; }
        public object DefaultValue { get; set; }
		public bool AllowNull { get; set; }
		public string Comments { get; set; }
		public string Reference { get; set; }
		public bool IsUnique { get; set; }
		public bool IsPK { get; set; }
		public string CheckConstraint { get; set; }
		public bool AutoIncrease { get; set; }
		public bool IsIndex { get; set; }
		public int Version { get; set; }

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
        public bool IsDefaultChanged { get; set; }
        public bool IsTypeChanged { get; set; }
        public bool IsNullChanged { get; set; }
        public bool IsConstraintChanged { get; set; }
        public bool IsReferenceChanged { get; set; }
    }
}
