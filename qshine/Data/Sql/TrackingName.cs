using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.database
{
    /// <summary>
    /// Tracking table name change class.
    /// </summary>
    public class TrackingName
    {
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
        /// Gets or sets the id of the object.
        /// </summary>
        /// <value>The Id of the object.</value>
        public long ObjectId { get; set; }
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
    }
}
