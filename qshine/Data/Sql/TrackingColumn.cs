using System;
namespace qshine.database
{
	/// <summary>
	/// Tracking table column.
	/// </summary>
	public class TrackingColumn
	{
		public long Id { get; set; }
		public string TableName { get; set; }
		public string Comments { get; set; }
		public string ColumnName { get; set; }
		public string ColumnType { get; set; }
		public int Size { get; set; }
        public int Scale { get; set; }
        public string DefaultValue { get; set; }
		public bool AllowNull { get; set; }
		public string Reference { get; set; }
		public bool IsUnique { get; set; }
		public bool IsPK { get; set; }
		public string CheckConstraint { get; set; }
		public bool AutoIncrease { get; set; }
		public bool IsIndex { get; set; }
		public long HashCode { get; set; }
		public int Version { get; set; }
		public DateTime CreatedOn { get; set; }
		public DateTime UpdatedOn { get; set; }
	}
}
