using System;
using System.Linq;
using System.Collections.Generic;

namespace qshine.database
{
	public class TrackingTable
	{
        public TrackingTable() { }
        public TrackingTable(SqlDDLTable table) {
            SchemaName = table.SchemaName;
            ObjectType =  TrackingObjectType.Table;
            ObjectName = table.TableName;
            HashCode = table.GetHashCode();
            Version = table.Version;
            Comments = table.Comments;
            Category = table.Category;
            Columns = table.Columns.Select(x =>
                new TrackingColumn
                {
                    TableName = table.TableName,
                    InternalId = x.InternalId,
                    Comments = x.Comments,
                    ColumnName = x.Name,
                    ColumnType = x.DbType.ToString(),
                    Size = x.Size,
                    Scale = x.Scale,
                    DefaultValue = Convert.ToString(x.DefaultValue),
                    AllowNull = x.AllowNull,
                    Reference = x.Reference,
                    IsUnique = x.IsUnique,
                    IsPK = x.IsPK,
                    CheckConstraint = x.CheckConstraint,
                    AutoIncrease = x.AutoIncrease,
                    IsIndex = x.IsIndex,
                    Version = x.Version,
                }).ToList();

        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public long Id { get; set; }
		/// <summary>
		/// Gets or sets the name of the schema.
		/// </summary>
		/// <value>The name of the schema.</value>
		public string SchemaName { get; set; }
		/// <summary>
		/// Gets or sets the type of database object.
		/// </summary>
		/// <value>The type of the object.</value>
		public TrackingObjectType ObjectType { get; set; }
		/// <summary>
		/// Gets or sets the name of the object.
		/// </summary>
		/// <value>The name of the object.</value>
		public string ObjectName { get; set; }
		/// <summary>
		/// Gets or sets the hash code.
		/// </summary>
		/// <value>The hash code.</value>
		public long HashCode { get; set; }
		/// <summary>
		/// Gets or sets the version.
		/// </summary>
		/// <value>The version.</value>
		public int Version { get; set; }
		/// <summary>
		/// Gets or sets the created on.
		/// </summary>
		/// <value>The created on.</value>
		public DateTime CreatedOn { get; set; }
		/// <summary>
		/// Gets or sets the updated on.
		/// </summary>
		/// <value>The updated on.</value>
		public DateTime UpdatedOn { get; set; }
		/// <summary>
		/// Gets or sets the comments.
		/// </summary>
		/// <value>The comments.</value>
		public string Comments { get; set; }
		/// <summary>
		/// Gets or sets the category.
		/// </summary>
		/// <value>The category.</value>
		public string Category { get; set; }


		public List<TrackingColumn> Columns { get; set; }
	}
}
