using System.Collections.Generic;
namespace qshine.database
{
	public interface ISqlDDLSyntax
    {
		/// <summary>
		/// Gets the name of the provider.
		/// </summary>
		/// <value>The name of the provider.</value>
		string ProviderName { get; }

        Database Database { get; }

        /// <summary>
        /// Is the database exists.
        /// </summary>
        /// <returns><c>true</c>, if database exists was ised, <c>false</c> otherwise.</returns>
        bool IsDatabaseExists();

		/// <summary>
		/// Creates the database.
		/// </summary>
		/// <returns><c>true</c>, if database was created, <c>false</c> otherwise.</returns>
		bool CreateDatabase();

        /// <summary>
        /// Get table name statement.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        string GetTableNameStatement(string tableName);

        /// <summary>
        /// Get Rename table statement
        /// </summary>
        /// <param name="oldTableName"></param>
        /// <param name="newTableName"></param>
        /// <returns>return rename table statement ex:"rename table [oldtable] to [newtable]"</returns>
        string GetRenameTableStatement(string oldTableName, string newTableName);

        /// <summary>
        /// Get table creation statement.
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
        string GetCreateTableStatement(SqlDDLTable table);

        /// <summary>
        /// Get table update statement.
        /// </summary>
        /// <returns>table update statement</returns>
        /// <param name="table">Table.</param>
        string GetUpdateTableStatement(SqlDDLTable table);

        /// <summary>
        /// Gets the last error message.
        /// </summary>
        /// <value>The last error message.</value>
        string LastErrorMessage { get; }

		/// <summary>
		/// Gets a value indicating whether this Database can create.
		/// </summary>
		/// <value><c>true</c> if can create; otherwise, <c>false</c>.</value>
		bool CanCreate { get; }

        /// <summary>
        /// Get named parameter prefix symbol.
        /// </summary>
        string ParameterPrefix { get; }

        /// <summary>
        /// Transfer C# DbType string to native database column type name.
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        string ToNativeDBType(string dbType, int size);
    }
}
