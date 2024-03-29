﻿using System;
using System.Collections.Generic;
using qshine.Configuration.Setting;

namespace qshine.Configuration
{
    /// <summary>
    /// Plugable environment configure class
    /// </summary>
	public class EnvironmentConfigure
	{
        /// <summary>
        /// Ctor
        /// </summary>
        public EnvironmentConfigure()
        {
            Components = new PluggableComponentCollection();
            Modules = new PluggableComponentCollection();
            ConnectionStrings = new ConnectionStrings();
            AppSettings = new Dictionary<string, string>();
            ConfigureFolders = new List<string>();
            AssemblyFolders = new List<StateObject<bool,string>>();
            Environments = new Dictionary<string, EnvironmentElement>();
            Maps = new Dictionary<string, Map>();
        }

        /// <summary>
        /// Clear all configure setting
        /// </summary>
        public void Clear()
        {
            Components.Clear();
            Modules.Clear();
            ConnectionStrings.Clear();
            AppSettings.Clear();
            ConfigureFolders.Clear();
            AssemblyFolders.Clear();
            Environments.Clear();
            Maps.Clear();
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

        List<string> _configFiles = new List<string>();
        /// <summary>
        /// named configuration files found in Configuration folders.
        /// </summary>
        public List<string> ConfigFiles {
            get { return _configFiles; }
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
		public PluggableComponentCollection Components
		{
            get;
            private set;
		}

        /// <summary>
        /// Get/Set named modules configure
        /// </summary>
		public PluggableComponentCollection Modules
		{
            get;
            private set;
		}

        /// <summary>
        /// Get/Set named maps configure
        /// </summary>
		public IDictionary<string, Map> Maps
        {
            get;
            private set;
        }

        /// <summary>
        /// Get/Set Connection strings
        /// </summary>
        public ConnectionStrings ConnectionStrings
        {
            get;
            private set;
        }

        /// <summary>
        /// Get/Set ApplicationEnvironment build error and warning handler.
        /// As default, all build error will be ignored and error will be logged through default Logger.
        /// 
        /// The Builder error and warning handler take two arguments:
        ///     arg1 - error or warning code
        ///     arg2 - error message.
        /// </summary>
        public Action<string, string> BuildErrorHandler { get; set; }

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
        internal PluggableComponent AddComponent(ComponentElement component, bool overWrite)
		{
			if (component == null) return null;
			var c = new PluggableComponent
			{
                ConfigureFilePath = component.CurrentConfiguration.FilePath,
                Name = component.Name,
				InterfaceTypeName = component.InterfaceType,
				ClassTypeName = component.Type,
                Scope = component.Scope,
                IsDefault =component.Default
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


			if (!Components.Contains(component.Name))
			{
                Components.Add(component.Name, c);
			}
			else
			{
				if (overWrite)
				{
                    Components[component.Name] = c;
				}
			}
			return Components[component.Name];
		}

        /// <summary>
        /// Adds the module.
        /// </summary>
        /// <returns>The module.</returns>
        /// <param name="module">Module.</param>
        /// <param name="overWrite">If set to <c>true</c> overwrite existing module.</param>
        internal PluggableComponent AddModule(PluggableComponent module, bool overWrite)
		{
			if (module == null) return null;

			if (!Modules.Contains(module.Name))
			{
                Modules.Add(module.Name, module);
			}
			else
			{
				if (overWrite)
				{
                    Modules[module.Name] = module;
				}
			}
			return Modules[module.Name];
		}

        /// <summary>
        /// Add or replace connection string element
        /// </summary>
        /// <param name="c">connection string element</param>
        /// <param name="overWrite">overwrite existing connection string if set to <c>true</c></param>
		internal void AddConnectionString(ConnectionStringElement c, bool overWrite)
		{
			if (ConnectionStrings[c.Name] == null || overWrite)
			{
                ConnectionStrings.AddOrUpdate(c);
			}
		}

        /// <summary>
        /// Adds the map.
        /// </summary>
        /// <returns>The map.</returns>
        /// <param name="name">map collection name</param>
        /// <param name="defaultKey">default map key within a map collection</param>
        /// <param name="map">key/value map.</param>
        /// <param name="overWrite">If set to <c>true</c> overwrite existing module.</param>
        public void AddMap(string name, string defaultKey, KeyValueElement map, bool overWrite)
        {
            if (map == null) return;

            if (string.IsNullOrEmpty(name)) name = "default";

            if (!Maps.ContainsKey(name))
            {
                Maps.Add(name, new Map
                {
                    Default = defaultKey,
                    Name = name,
                });
                Maps[name][map.Key] = map.Value;
            }
            else
            {
                if (Maps[name].ContainsKey(map.Key))
                {
                    if (overWrite)
                    {
                        Maps[name].Default = defaultKey;
                        Maps[name][map.Key]= map.Value;
                    }

                }else 
                {
                    Maps[name][map.Key]= map.Value;
                }
            }
        }

        #endregion
    }
}
