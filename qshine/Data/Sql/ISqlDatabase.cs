using System.Collections.Generic;
namespace qshine.database
{
	public interface ISqlDatabase: System.IDisposable
    {
		/// <summary>
		/// Gets the name of the provider.
		/// </summary>
		/// <value>The name of the provider.</value>
		string ProviderName { get; }

		/// <summary>
		/// Gets or sets the connection string.
		/// </summary>
		/// <value>The connection string.</value>
		string ConnectionString { get; set; }

		/// <summary>
		/// Is the database exists.
		/// </summary>
		/// <returns><c>true</c>, if database exists was ised, <c>false</c> otherwise.</returns>
		bool IsDatabaseExists();

		/// <summary>
		/// Is the table exists.
		/// </summary>
		/// <returns><c>true</c>, if table exists, <c>false</c> otherwise.</returns>
		/// <param name="tableName">Table name.</param>
		bool IsTableExists(string tableName);
		/// <summary>
		/// Creates the database.
		/// </summary>
		/// <returns><c>true</c>, if database was created, <c>false</c> otherwise.</returns>
		bool CreateDatabase();

		/// <summary>
		/// Creates the table.
		/// </summary>
		/// <returns><c>true</c>, if table was created, <c>false</c> otherwise.</returns>
		/// <param name="table">Table.</param>
		bool CreateTable(SqlDDLTable table);

		/// <summary>
		/// Updates the table.
		/// </summary>
		/// <returns><c>true</c>, if table was updated, <c>false</c> otherwise.</returns>
		/// <param name="table">Table.</param>
		/// <param name="trackingTable">Tracking table.</param>
		bool UpdateTable(SqlDDLTable table, TrackingTable trackingTable);

		void RenameTableName(string oldTableName, string newTableName);

		/// <summary>
		/// Build this instance.
		/// </summary>
		/// <returns>return true if build is sucessful.</returns>
		//bool Build(IList<SqlDDLTable> tables);

		/// <summary>
		/// Gets the last error message.
		/// </summary>
		/// <value>The last error message.</value>
		string LastErrorMessage { get; }

		/// <summary>
		/// Gets the DBClient.
		/// </summary>
		/// <value>The DBClient.</value>
		DbClient DBClient { get; }

		/// <summary>
		/// Gets a value indicating whether this Database can create.
		/// </summary>
		/// <value><c>true</c> if can create; otherwise, <c>false</c>.</value>
		bool CanCreate { get; }

        /// <summary>
        /// Get named parameter prefix symbol.
        /// </summary>
        string ParameterPrefix { get; }
	}
}
