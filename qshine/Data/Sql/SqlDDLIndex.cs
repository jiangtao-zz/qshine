namespace qshine.database
{
    /// <summary>
    /// Index information
    /// </summary>
    public class SqlDDLIndex
    {
        /// <summary>
        /// Table Name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Index Name
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// Comma separated column names
        /// </summary>
        public string IndexColumns { get; set; }

        /// <summary>
        /// Indicates a unique index
        /// </summary>
        public bool IsUnique { get; set; }
    }
}
