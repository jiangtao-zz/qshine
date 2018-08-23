using System;
using System.Collections.Generic;
using System.Data;

namespace qshine.database
{
	public class SqlDDLColumn
	{
		public SqlDDLColumn()
		{
		}
		public string Name { get; set; }
		public DbType DbType { get; set; }
		public int Size { get; set; }
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
	}
}
