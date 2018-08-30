using System;
using System.Collections.Generic;

namespace qshine.database
{
	/// <summary>
	/// Native Sql database interface
	/// </summary>
	public interface ISqlDialectProvider:IProvider
	{
		/// <summary>
		/// Gets the database SQL Dialect instance.
		/// </summary>
		/// <returns>The instance of SqlDialect.</returns>
		/// <param name="connectionString">Db connection string.</param>
		ISqlDialect GetSqlDialect(string connectionString);
	}
}
