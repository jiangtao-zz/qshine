using System.Collections.Generic;
namespace qshine.database
{
	public interface ISqlDialect
    {
		/// <summary>
		/// Gets .NET ADO provider name.
		/// </summary>
		/// <value>The name of the provider.</value>
		string ProviderName { get; }

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
        string TableRenameSql(string oldTableName, string newTableName);

        /// <summary>
        /// Get Sql statements to create a new table.
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
        string TableCreateSql(SqlDDLTable table);

        /// <summary>
        /// Get Sql statements to update table structure.
        /// </summary>
        /// <returns>table update statement</returns>
        /// <param name="table">Table.</param>
        string TableUpdateSql(SqlDDLTable table);

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
        /// Transfer C# DbType string to native database column type name.
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        string ToNativeDBType(string dbType, int size);

        /// <summary>
        /// Gets the last error message.
        /// </summary>
        /// <value>The last error message.</value>
        string LastErrorMessage { get; }
    }
}
