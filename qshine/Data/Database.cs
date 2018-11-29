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
        string _dataSource="";
		DbProviderFactory  _factory;

        #region Ctor::
        /// <summary>
        /// Initializes a new instance of the <see cref="T:qshine.Database"/> class by given provider and connection string.
        /// </summary>
        /// <param name="providerName">Database provider name.</param>
        /// <param name="connectionString">Database connection string.</param>
        public Database(string providerName, string connectionString)
		{
			ProviderName = providerName;
			ConnectionString = connectionString;

            //Ensure provider and connection string available
            Check.Assert<InvalidProviderException>(
                !string.IsNullOrEmpty(ProviderName) && !string.IsNullOrEmpty(ConnectionString),
                "The given database provider or connection string couldn't be empty.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:qshine.Database"/> class from default configure setting.
        /// </summary>
        public Database()
			: this("") {}

        /// <summary>
        /// Initializes a new instance of the <see cref="T:qshine.Database"/> class from configure setting.
        /// </summary>
        /// <param name="configedConnectionName">Configed connection name from environment config files.</param>
        /// <remarks>
        ///  Find a given named connection string setting from configure file. The search path is:
        ///     a. The named connection string.
        ///     b. The "default" name connection string if name is not present
        ///     c. The first connection string if name is not present
        ///     d. throw exception if no any connection string found.

        /// </remarks>
        public Database(string configedConnectionName)
		{
            var name = string.IsNullOrEmpty(configedConnectionName) ? "default" : configedConnectionName;
            //1. Find a given named connection string setting from configure
            //a. The name exactly match.
            //b. The "default" name if name not present
            for (int i = 0; i<ApplicationEnvironment.Configure.ConnectionStrings.Count;i++)
			{
				var connectionStringSetting = ApplicationEnvironment.Configure.ConnectionStrings[i];

				if (name.Equals(connectionStringSetting.Name, StringComparison.InvariantCultureIgnoreCase))
				{
					ProviderName = connectionStringSetting.ProviderName;
					ConnectionString = connectionStringSetting.ConnectionString;
					break;
				}
			}

            //Get first one if name is empty or null
			if (string.IsNullOrEmpty(configedConnectionName) 
                && ApplicationEnvironment.Configure.ConnectionStrings.Count>0)
			{
				ProviderName = ApplicationEnvironment.Configure.ConnectionStrings[0].ProviderName;
				ConnectionString = ApplicationEnvironment.Configure.ConnectionStrings[0].ConnectionString;
			}

            //Ensure provider and connection string available
            Check.Assert< InvalidProviderException >(
                !string.IsNullOrEmpty(ProviderName) && !string.IsNullOrEmpty(ConnectionString),
				"The named {0} database provider and connection string couldn't be found from configuration setting.",
                name);
		}

        #endregion

        #region public Properties
        /// <summary>
        /// Get database provider factory
        /// </summary>
        public DbProviderFactory DbProviderFactory
		{
			get {
                if (_factory == null)
                {
                    _factory = DbProviderFactories.GetFactory(ProviderName);

                    Check.Assert<InvalidProviderException>(_factory != null,
                        ".NET DbProviderFactory {0} load error.",
                        ProviderName
                        );
                }
                return _factory;

            }
		}

        /// <summary>
        /// Gets the name of database provider.
        /// </summary>
        /// <value>The name of the provider.</value>
        public string ProviderName { get; private set; }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Get data source name
        /// </summary>
        public string DataSource
        {
            get
            {
                if (string.IsNullOrEmpty(_dataSource) && IsValid)
                {
                    _dataSource = ParseDataSource();
                }
                return _dataSource;
            }
        }

        /// <summary>
        /// Check the database provider and connection string format correct
        /// </summary>
        public bool IsValid
        {
            get
            {
                try
                {
                    return ConnectionStringBuilder!=null;
                }
                catch(Exception ex)
                {
                    Log.DevDebug("Database.IsValid exception: {0}", ex.Message);
                    return false;
                }
            }
        }

        DbConnectionStringBuilder _builder;
        /// <summary>
        /// Get given database connection string builder.
        /// </summary>
        public DbConnectionStringBuilder ConnectionStringBuilder
        {
            get {
                if (_builder == null)
                {
                    _builder = DbProviderFactory.CreateConnectionStringBuilder();
                    _builder.ConnectionString = ConnectionString;
                }
                return _builder;
            }
        }

        /// <summary>
        /// Check the database instance alive
        /// </summary>
        public bool IsAlive
        {
            get
            {
                using (var connection = CreateConnection())
                {
                    try
                    {
                        connection.Open();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }


        #endregion

        /// <summary>
        /// Creates the connection.
        /// </summary>
        /// <returns>The connection.</returns>
        public DbConnection CreateConnection()
		{
			var connection = DbProviderFactory.CreateConnection();
			connection.ConnectionString = ConnectionString;
			return connection;
		}

        /// <summary>
        /// Create a data adapter
        /// </summary>
        /// <returns></returns>
        public DbDataAdapter CreateAdapter()
        {
            return DbProviderFactory.CreateDataAdapter();
        }

        private string ParseDataSource()
		{
            object dataSource;
            if (ConnectionStringBuilder.TryGetValue("Data Source", out dataSource))
            {
                return dataSource.ToString();
            }
            return "";
		}

	}

}
