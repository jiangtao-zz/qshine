using System;
using System.Collections.Generic;

namespace qshine.Configuration
{
    /// <summary>
    /// Plugable environment configure class
    /// </summary>
	public class EnvironmentConfigure
	{
		readonly IDictionary<string, PlugableComponent> _componentConfigure;
        readonly IDictionary<string, NamedTypeElement> _moduleConfigure;
        readonly ConnectionStrings _connectionStrings;

        /// <summary>
        /// Ctor
        /// </summary>
        public EnvironmentConfigure()
        {
            _componentConfigure = new Dictionary<string, PlugableComponent>();
            _moduleConfigure = new Dictionary<string, NamedTypeElement>();
            _connectionStrings = new ConnectionStrings();

            AppSettings = new Dictionary<string, string>();
            ConfigureFolders = new List<string>();
            AssemblyFolders = new List<StateObject<bool,string>>();

            Environments = new Dictionary<string, EnvironmentElement>();
        }

        /// <summary>
        /// Get/Set Application setting key/value pair
        /// </summary>
		public IDictionary<string, string> AppSettings
        {
            get;
            private set;
        }

        /// <summary>
        /// Get/Set configure folders.
        /// The configure folders contains all configure files and plugable components to be used by the application.
        /// </summary>
        public List<string> ConfigureFolders
		{
            get;
            private set;
		}

        /// <summary>
        /// Get/Set plugable components binary assembly folder.
        /// The folders contain all assemblies to be loaded by configure files.
        /// </summary>
        public List<StateObject<bool,string>> AssemblyFolders
		{
            get;
            private set;
		}

        /// <summary>
        /// Get/Set named environment configures.
        /// Each environment target to one specific application setting. Such as DEV, QA, UA
        /// </summary>
		public IDictionary<string, EnvironmentElement> Environments
		{
            get;
            private set;
		}

        /// <summary>
        /// Get/Set named components configure
        /// </summary>
		public IDictionary<string, PlugableComponent> Components
		{
			get
			{
				return _componentConfigure;
			}
		}

        /// <summary>
        /// Get/Set named modules configure
        /// </summary>
		public IDictionary<string, NamedTypeElement> Modules
		{
			get
			{
				return _moduleConfigure;
			}
		}

        /// <summary>
        /// Get/Set Connection strings
        /// </summary>
        public ConnectionStrings ConnectionStrings
        {
            get
            {
                return _connectionStrings;
            }
        }

        #region internal methods

        /// <summary>
        /// Adds named environment setting.
        /// </summary>
        /// <returns>The environment.</returns>
        /// <param name="environment">Environment element.</param>
        /// <param name="overLoad">allow overload setting if set it to <c>true</c>. Otherwise, keep previous named one</param>
        internal EnvironmentElement AddEnvironment(EnvironmentElement environment, bool overLoad = false)
		{
			if (environment == null) return null;

			if (!Environments.ContainsKey(environment.Name))
			{
                Environments.Add(environment.Name, environment);
			}
			else
			{
				if (overLoad)
				{
                    Environments[environment.Name] = environment;
				}
			}
			return Environments[environment.Name];
		}

        /// <summary>
        /// Adds the component.
        /// </summary>
        /// <returns>The component.</returns>
        /// <param name="component">Component.</param>
        /// <param name="overWrite">If set to <c>true</c> overwrite exists component.</param>
        internal PlugableComponent AddComponent(ComponentElement component, bool overWrite)
		{
			if (component == null) return null;
			var c = new PlugableComponent
			{
				Name = component.Name,
				InterfaceTypeName = component.InterfaceType,
				ClassTypeName = component.Type,
                Scope = component.Scope,
			};
            if(!string.IsNullOrEmpty(c.Scope) && String.Equals(c.Scope, "singleton", StringComparison.OrdinalIgnoreCase))
            {
                c.Scope = IocInstanceScope.Singleton.ToString();
            }
            else
            {
                c.Scope = IocInstanceScope.Transient.ToString();
            }

            foreach (var p in component.Parameters)
			{
				c.Parameters.Add(p.Name, p.Value);
			}
			//c.InterfaceType = Type.GetType(c.InterfaceTypeName);
			//c.ClassType = Type.GetType(c.ClassTypeName);


			if (!_componentConfigure.ContainsKey(component.Name))
			{
				_componentConfigure.Add(component.Name, c);
			}
			else
			{
				if (overWrite)
				{
					_componentConfigure[component.Name] = c;
				}
			}
			return _componentConfigure[component.Name];
		}

        /// <summary>
        /// Adds the module.
        /// </summary>
        /// <returns>The module.</returns>
        /// <param name="module">Module.</param>
        /// <param name="overWrite">If set to <c>true</c> overwrite existing module.</param>
        internal NamedTypeElement AddModule(NamedTypeElement module, bool overWrite)
		{
			if (module == null) return null;

			if (!_moduleConfigure.ContainsKey(module.Name))
			{
				_moduleConfigure.Add(module.Name, module);
			}
			else
			{
				if (overWrite)
				{
					_moduleConfigure[module.Name] = module;
				}
			}
			return _moduleConfigure[module.Name];
		}

        /// <summary>
        /// Add or replace connection string element
        /// </summary>
        /// <param name="c">connection string element</param>
        /// <param name="overWrite">overwrite existing connection string if set to <c>true</c></param>
		internal void AddConnectionString(ConnectionStringElement c, bool overWrite)
		{
			if (_connectionStrings[c.Name] == null || overWrite)
			{
				_connectionStrings.AddOrUpdate(c);
			}
		}

        #endregion
    }
}
