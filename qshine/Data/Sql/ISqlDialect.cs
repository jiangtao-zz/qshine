using System.Collections.Generic;
namespace qshine.database
{
	public interface ISqlDialect
    {
        /// <summary>
        /// Check database instance exists.
        /// </summary>
        /// <returns><c>true</c>, if database exists, <c>false</c> otherwise.</returns>
        bool DatabaseExists();

		/// <summary>
		/// Creates a database based on given connection string.
		/// </summary>
		/// <returns><c>true</c>, if database was created, <c>false</c> otherwise.</returns>
		bool CreateDatabase();

        /// <summary>
        /// Get a SQL statement to check table exists.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <returns></returns>
        string TableExistSql(string tableName);

        /// <summary>
        /// Get a SQL statement to rename a table 
        /// </summary>
        /// <param name="oldTableName">table name to be changed</param>
        /// <param name="newTableName">new table name</param>
        /// <returns>return rename table statement ex:"rename table [oldtable] to [newtable]"</returns>
        string TableRenameClause(string oldTableName, string newTableName);

        /// <summary>
        /// Get Sql statements to create a new table.
        /// It is a collection of sql commands
        /// </summary>
        /// <returns>Get table creation statement</returns>
        /// <param name="table">Table.</param>
        /// <example>
        /// CREATE TABLE {
        ///     C1 VARCHAR2(20) NOT NULL UNIQUE PRIMARY KEY,
        ///     C2 NUMBER
        /// };
        /// ...
        /// </example>
        List<string> TableCreateSqls(SqlDDLTable table);

        /// <summary>
        /// Analyse the table structure and get table and column modified information.
        /// The TableUpdateSql is based on analysis result.
        /// </summary>
        /// <returns>true, if the table structure changed</returns>
        /// <param name="table">Table structure.</param>
        /// <param name="trackingTable">Tracking table information.</param>
        /// <remarks>This method will update the SqlDDLTable "table" status.</remarks>
        bool AnalyseTableChange(SqlDDLTable table, TrackingTable trackingTable);

        /// <summary>
        /// Get Sql statements for table structure update.
        /// Sql statement is based on table change analysis result. 
        /// Call AnalyseTableChange() before perform TableUpdateSql.
        /// </summary>
        /// <returns>table update statement</returns>
        /// <param name="table">Table.</param>
        List<string> TableUpdateSqls(SqlDDLTable table);

		/// <summary>
		/// Gets a value indicating whether a database can be created.
        /// Some database only can be created by DBA.
		/// </summary>
		/// <value><c>true</c> if can create; otherwise, <c>false</c>.</value>
		bool CanCreate { get; }

        /// <summary>
        /// Get named parameter prefix symbol. 
        /// Standard SQL use "@" as parameter prefix in SQL statement. Ex: select * from t1 where id=@p1.
        /// </summary>
        string ParameterPrefix { get; }

        /// <summary>
        /// Indicates an unique index created automatically if the column has unique constraint
        /// </summary>
        bool AutoUniqueIndex { get; }

        /// <summary>
        /// Indicates using outline constraints instead of inline column constraint 
        /// </summary>
        bool EnableOutlineCheckConstraint { get; }

        /// <summary>
        /// Transfer C# DbType string to native database column type name.
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="size">size of character or number precision (total number of digits)</param>
        /// <param name="scale">number scale (digits to the right of the decimal point)</param>
        /// <returns></returns>
        string ToNativeDBType(string dbType, int size, int scale);

        /// <summary>
        /// Get Sql condition 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        string ToSqlCondition(string columnName, string op, object value);

        /// <summary>
        /// Gets the last error message.
        /// </summary>
        /// <value>The last error message.</value>
        string LastErrorMessage { get; }
    }
}
