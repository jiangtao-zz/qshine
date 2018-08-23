using System;
using System.Data.Common;
using System.Data.OleDb;
using qshine.Configuration;

namespace qshine
{
	/// <summary>
	/// Relational database provider.
	/// </summary>
	public class Database
	{
		string _providerName;
		string _connectionString;
		string _dataSource;
		DbProviderFactory  _factory;
		/// <summary>
		/// Initializes a new instance of the <see cref="T:qshine.DbProvider"/> class.
		/// </summary>
		/// <param name="providerName">Database provider name.</param>
		/// <param name="connectionString">Database connection string.</param>
		public Database(string providerName, string connectionString)
		{
			_providerName = providerName;
			_connectionString = connectionString;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:qshine.Database"/> class.
		/// </summary>
		public Database()
			: this("") {}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="T:qshine.DbProvider"/> class.
		/// </summary>
		/// <param name="configedConnectionName">Configed connection name from environment config files.</param>
		public Database(string configedConnectionName)
		{
			//Load default database from configure manager

			for (int i = 0; i<EnvironmentManager.Configure.ConnectionStrings.Count;i++)
			{
				var connectionStringSetting = EnvironmentManager.Configure.ConnectionStrings[i];
				if (connectionStringSetting.Name == configedConnectionName ||
				    (string.IsNullOrEmpty(configedConnectionName) && (string.IsNullOrEmpty(connectionStringSetting.Name) || "default".Equals(connectionStringSetting.Name, StringComparison.InvariantCultureIgnoreCase)))
				   )
				{
					_providerName = connectionStringSetting.ProviderName;
					_connectionString = connectionStringSetting.ConnectionString;
					break;
				}
			}
			if (string.IsNullOrEmpty(configedConnectionName) && EnvironmentManager.Configure.ConnectionStrings.Count>0 && string.IsNullOrEmpty(_providerName))
			{
				_providerName = EnvironmentManager.Configure.ConnectionStrings[0].ProviderName;
				_connectionString = EnvironmentManager.Configure.ConnectionStrings[0].ConnectionString;
			}
			if (string.IsNullOrEmpty(_providerName) || string.IsNullOrEmpty(_connectionString))
			{
				throw new InvalidProviderException(string.Format("The named {0} database provider and connection string couldn't be found in configuration setting.", configedConnectionName));
			}
		}

		/// <summary>
		/// Get database provider factory
		/// </summary>
		public DbProviderFactory DbProviderFactory
		{
			get { 
				return _factory ?? (_factory = DbProviderFactories.GetFactory(_providerName)); 
			}
		}

		/// <summary>
		/// Creates the connection.
		/// </summary>
		/// <returns>The connection.</returns>
		public DbConnection CreateConnection()
		{
			var connection = DbProviderFactory.CreateConnection();
			connection.ConnectionString = _connectionString;
			return connection;
		}

		/// <summary>
		/// Get data source name
		/// </summary>
		public string DataSource
		{
			get
			{
				if (string.IsNullOrEmpty(_dataSource))
				{
					_dataSource = ParseDataSource(_connectionString);
				}
				return _dataSource;
			}
		}

		/// <summary>
		/// Gets the name of database provider.
		/// </summary>
		/// <value>The name of the provider.</value>
		public string ProviderName
		{
			get
			{
				return _providerName;
			}
		}

		/// <summary>
		/// Gets the connection string.
		/// </summary>
		/// <value>The connection string.</value>
		public string ConnectionString
		{
			get
			{
				return _connectionString;
			}
		}

		private static string ParseDataSource(string connectionString)
		{
			var builder = new OleDbConnectionStringBuilder(connectionString);
			return builder.DataSource;
		}

	}
}
