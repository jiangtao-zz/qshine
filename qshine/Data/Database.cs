using System;
using System.Collections.Generic;
using System.Data.Common;
using qshine.Configuration;
using qshine.Utility;
using qshine.Logger;
using System.Linq;
using qshine.Globalization;

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
                "The given database provider or connection string couldn't be empty."._G());
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
            for (int i = 0; i<ApplicationEnvironment.Default.EnvironmentConfigure.ConnectionStrings.Count;i++)
			{
				var connectionStringSetting = ApplicationEnvironment.Default.EnvironmentConfigure.ConnectionStrings[i];

				if (name.Equals(connectionStringSetting.Name, StringComparison.InvariantCultureIgnoreCase))
				{
					ProviderName = connectionStringSetting.ProviderName;
					ConnectionString = connectionStringSetting.ConnectionString;
					break;
				}
			}

            //Get first one if name is empty or null
			if (string.IsNullOrEmpty(configedConnectionName) 
                && ApplicationEnvironment.Default.EnvironmentConfigure.ConnectionStrings.Count>0)
			{
				ProviderName = ApplicationEnvironment.Default.EnvironmentConfigure.ConnectionStrings[0].ProviderName;
				ConnectionString = ApplicationEnvironment.Default.EnvironmentConfigure.ConnectionStrings[0].ConnectionString;
			}

            //Ensure provider and connection string available
            Check.Assert< InvalidProviderException >(
                !string.IsNullOrEmpty(ProviderName) && !string.IsNullOrEmpty(ConnectionString),
				"The named {0} database provider and connection string couldn't be found from configuration setting."._G(name));
		}

        #endregion

        /// <summary>
        /// Override instance hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return FastHash.GetHashCode(ProviderName, ConnectionString);
        }


        #region public Properties
        static Dictionary<string, string> _parameterPrefixLookup = new Dictionary<string, string>();

        void SetParameterPrefix(DbProviderFactory factory)
        {
            CommandBuilder = factory.CreateCommandBuilder();
            CommandBuilder.TryCallNonPublic(out string placeholder, null, "GetParameterPlaceholder", 0);
            if (!string.IsNullOrEmpty(placeholder))
            {
                _parameterPrefixLookup.Add(ProviderName, placeholder.Substring(0,1));
            }
        }

        /// <summary>
        /// Get database parameterPrefix
        /// </summary>
        public string ParameterPrefix
        {
            get
            {
                return _parameterPrefixLookup[ProviderName];
            }
        }

        /// <summary>
        /// The command builder will be available after DbProviderFactory set;
        /// </summary>
        public DbCommandBuilder CommandBuilder
        {
            get; private set;
        }

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
                        ".NET DbProviderFactory {0} load error."._G(ProviderName));

                    if (!_parameterPrefixLookup.ContainsKey(ProviderName))
                    {
                        SetParameterPrefix(_factory);
                    }
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

        List<IDbTypeMapper> _dbTypeMappers;
        /// <summary>
        /// Map database dbtype
        /// </summary>
        public List<IDbTypeMapper> DbTypeMappers
        {
            get
            {
                if (_dbTypeMappers == null)
                {
                    _dbTypeMappers = new List<IDbTypeMapper>();
                    foreach (var mapperProviderName in _globalDbTypeMappers.Keys.Where(x=> IsMatchProvider(x)))
                    {
                        _dbTypeMappers.AddRange(_globalDbTypeMappers[mapperProviderName]);
                    }
                }
                return _dbTypeMappers;
            }
        }

        /// <summary>
        /// Check the tags match to database provider
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public bool IsMatchProvider(string tags)
        {
            if (tags == "*") return true;

            var provider = ProviderName;

            var tagList = tags.Split(',');
            foreach (var tag in tagList)
            {
                if (provider.IndexOf(tag, StringComparison.InvariantCultureIgnoreCase) >= 0) return true;
            }
            return false;
        }

        private string ParseDataSource()
		{
            if (ConnectionStringBuilder.TryGetValue("Data Source", out object dataSource))
            {
                return dataSource.ToString();
            }
            return "";
		}

        static SafeDictionary<string, List<IDbTypeMapper>> _globalDbTypeMappers = new SafeDictionary<string, List<IDbTypeMapper>>();

        /// <summary>
        /// Register a database DBType mapper.
        /// </summary>
        /// <param name="mapper"></param>
        static public void RegisterDbTypeMapper(IDbTypeMapper mapper)
        {
            var providerName = mapper.SupportedProviderNames;
            if (!_globalDbTypeMappers.ContainsKey(providerName))
            {
                _globalDbTypeMappers.Add(providerName, new List<IDbTypeMapper>() { mapper });
            }
            else
            {
                if(!_globalDbTypeMappers[providerName].Any(x=>x.GetType()==mapper.GetType()))
                {
                    _globalDbTypeMappers[providerName].Add(mapper);
                }
            }
        }

	}
}
