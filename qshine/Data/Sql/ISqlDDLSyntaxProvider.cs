using System;
using System.Collections.Generic;

namespace qshine.database
{
	/// <summary>
	/// Native Sql database interface
	/// </summary>
	public interface ISqlDDLSyntaxProvider:IProvider
	{
		/// <summary>
		/// Gets the database instance.
		/// </summary>
		/// <returns>The instance.</returns>
		/// <param name="dbConnectionString">Db connection string.</param>
		ISqlDDLSyntax GetInstance(string dbConnectionString);
	}
}
