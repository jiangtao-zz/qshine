using System;
namespace qshine.database
{
	/// <summary>
	/// Tracking table column.
	/// </summary>
	public class TrackingColumn
	{
        /// <summary>
        /// Tracking table Id
        /// </summary>
		public long Id { get; set; }
        /// <summary>
        /// Internal column Id
        /// </summary>
        public long InternalId { get; set; }
        /// <summary>
        /// Table Id
        /// </summary>
        public long TableId { get; set; }
        /// <summary>
        /// Table name
        /// </summary>
		public string TableName { get; set; }
        /// <summary>
        /// Comments
        /// </summary>
		public string Comments { get; set; }
        /// <summary>
        /// Column name
        /// </summary>
		public string ColumnName { get; set; }
        /// <summary>
        /// Column type
        /// </summary>
		public string ColumnType { get; set; }
        /// <summary>
        /// Column size
        /// </summary>
		public int Size { get; set; }
        /// <summary>
        /// Column scale
        /// </summary>
        public int Scale { get; set; }
        /// <summary>
        /// Column default value
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// Indicates column nullable
        /// </summary>
		public bool AllowNull { get; set; }
        /// <summary>
        /// FK
        /// </summary>
		public string Reference { get; set; }
        /// <summary>
        /// Indicates unique key column
        /// </summary>
		public bool IsUnique { get; set; }
        /// <summary>
        /// Indicates PK column
        /// </summary>
		public bool IsPK { get; set; }
        /// <summary>
        /// Column Check constraint
        /// </summary>
		public string CheckConstraint { get; set; }
        /// <summary>
        /// Auto increasement
        /// </summary>
		public bool AutoIncrease { get; set; }
        /// <summary>
        /// Index column
        /// </summary>
		public bool IsIndex { get; set; }
        /// <summary>
        /// Hash code
        /// </summary>
		public long HashCode { get; set; }
        /// <summary>
        /// Column version
        /// </summary>
		public int Version { get; set; }
        /// <summary>
        /// Column created time
        /// </summary>
		public DateTime CreatedOn { get; set; }
        /// <summary>
        /// Column last update time 
        /// </summary>
		public DateTime UpdatedOn { get; set; }
	}
}
