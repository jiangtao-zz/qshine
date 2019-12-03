using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.Linq;
using System.Configuration;
using qshine.Utility;
using qshine.Logger;
using qshine.Configuration.Setting;
using qshine.Globalization;
using qshine.Configuration.ConfigurationStore;

namespace qshine.Configuration
{
    /// <summary>
    /// Service class to manage application environment configuration to support plugable components.
    /// Each application usually has a single ApplicationEnvironment instance to manage different set of configuration files for different running environments.
    /// Only current run-time environment appreciated plugable components and environment settings will be loaded into application as default behaviour. 
    /// The behaviour could be changed by application environment manager  .
    /// 
    /// </summary>
	public partial class ApplicationEnvironment
    {
        #region Static shortcut
        static ApplicationEnvironment _defaultApplicationEnvironment;
        /// <summary>
        /// Get default application environment
        /// </summary>
        public static ApplicationEnvironment Default
        {
            get
            {
                if(_defaultApplicationEnvironment == null)
                    _defaultApplicationEnvironment  = new ApplicationEnvironment(ApplicationEnvironmentContext.GetContext(""));

                //Check.HaveValue(_defaultApplicationEnvironment, "ApplicationEnvironment.Default");

                return _defaultApplicationEnvironment;
            }
        }

        /// <summary>
        /// Build default environment
        /// </summary>
        /// <param name="configFile">configuration file.</param>
        /// <param name="options">configuration options.</param>
        /// <returns></returns>
        public static ApplicationEnvironment Build(string configFile=null, EnvironmentInitializationOption options=null)
        {

            if (options == null)
            {
                options = new EnvironmentInitializationOption
                {
                    OverwriteConnectionString = true
                };
            }

            var builder = new ApplicationEnvironmentBuilder();

            var env = builder.Configure(
                (appContext, config) =>
                    {
                        config.LoadConfigFile(configFile, options);
                    }
                    )
                    .Build();

            return env;
        }

        #endregion

        ApplicationEnvironmentContext _context;
        #region Ctor

        /// <summary>
        /// Ctor:: build application environment by enironment context.
        /// </summary>
        /// <param name="context">Application environment context</param>
        internal ApplicationEnvironment(ApplicationEnvironmentContext context)
        {
            _context = context;

            //Set default application environment
            if (_defaultApplicationEnvironment == null)
            {
                //set first application environment
                _defaultApplicationEnvironment = this;
            }
            else
            {
                if (!string.IsNullOrEmpty(_defaultApplicationEnvironment.Name) && string.IsNullOrEmpty(Name))
                {
                    //The default one is not a named app environment
                    _defaultApplicationEnvironment = this;
                }
            }
        }
        #endregion

        #region public properties

        /// <summary>
        /// Name of the application environment
        /// </summary>
        public string Name
        {
            get
            {
                return _context != null ? _context.Name:"";
            }
        }

        /// <summary>
        /// Get Environment Configure setting
        /// </summary>
        public EnvironmentConfigure EnvironmentConfigure { get { return _context.EnvironmentConfigure; } }

        /// <summary>
        /// Gets configure setting value by a given key
        /// </summary>
        /// <param name="key">specifies an application environment setting key</param>
        public string this[string key]
        {
            get
            {
                return EnvironmentConfigure.AppSettings.ContainsKey(key) ? EnvironmentConfigure.AppSettings[key] : string.Empty;
            }
        }

        /// <summary>
        /// Get connection strings
        /// </summary>
        public ConnectionStrings ConnectionStrings
        {
            get
            {
                return EnvironmentConfigure.ConnectionStrings;
            }
        }

        /// <summary>
        /// Get environment pluggable assembly collection.
        /// </summary>
        public PluggableAssemblyCollection PlugableAssemblies
        {
            get
            {
                return _context.PlugableAssemblies;
            }
        }

        MappedComponents _services;

        /// <summary>
        /// Get pluggable component services
        /// </summary>
        public MappedComponents Services
        {
            get
            {
                if (_services == null)
                {
                    _services = new MappedComponents(_context.EnvironmentConfigure);
                }
                return _services;
            }
        }

        #endregion

        #region public Methods

        /// <summary>
        /// It will invoke all instances that implemented type T interface or base class.
        /// The type T constructor()
        /// </summary>
        /// <typeparam name="T">An interface or base class for start up class implementation.</typeparam>
        public ApplicationEnvironment Startup<T>()
        {
            var types = PlugableAssemblies.SafeGetInterfacedTypes(typeof(T));

            foreach (var type in types)
            {
                //Try to create startup instance using current ApplicationEnvironment instance as parameter
                var instance = type.TryCreateInstance(this);

                if (instance == null)
                {
                    //If parameter is not type of ApplicationEnvironment. use default constructor. 
                    type.TryCreateInstance();
                }
            }
            return this;
        }

        /// <summary>
        /// Gets the first config file path searching through all config folders.
        /// </summary>
        /// <returns>The config file path if any or null.</returns>
        /// <param name="configFileName">Config file name.</param>
        public string GetConfigFilePathIfAny(string configFileName)
        {
            if (string.IsNullOrWhiteSpace(configFileName))
            {
                return null;
            }
            var fileName = configFileName.ToLower();
            if (EnvironmentConfigure.ConfigFiles.Any(x => x.EndsWith(fileName)))
            {
                return EnvironmentConfigure.ConfigFiles.SingleOrDefault(x => x.EndsWith(fileName));
            }
            return null;
        }

        /// <summary>
        /// Add a pluggable component into application environment
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public ApplicationEnvironment PlugComponent(PluggableComponent component)
        {
            if (!EnvironmentConfigure.Components.Any(x => x.Name == component.Name))
            {
                EnvironmentConfigure.Components.Add(component.Name, component);
                if (component.Instantiate(_context))
                {
                    Logger.Info("PlugComponent:: {0}"._G(component.FormatObjectValues()));
                }
                else
                {
                    throw new InvalidProviderException("PlugComponent:: {0}"._G(component.FormatObjectValues()));
                }
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="IT"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="scope"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ApplicationEnvironment PlugComponent<IT, T>(string name, IocInstanceScope scope = IocInstanceScope.Transient, IDictionary<string, string> args = null)
        {
            return PlugComponent(typeof(IT), typeof(T), name, scope, args);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="classType"></param>
        /// <param name="name"></param>
        /// <param name="scope"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ApplicationEnvironment PlugComponent(Type interfaceType, Type classType, string name, IocInstanceScope scope = IocInstanceScope.Transient, IDictionary<string, string> args=null)
        {
            var component = new PluggableComponent
            {
                ConfigureFilePath = ".",
                Name = name,
                InterfaceType = interfaceType,
                InterfaceTypeName = interfaceType.FullName,
                ClassType = classType,
                ClassTypeName = classType.FullName,
                Scope = scope.ToString(),
                IsDefault = false
            };

            if (args != null)
            {
                foreach(var key in args.Keys)
                {
                    component.Parameters.Add(key, args[key]);
                }
            }
            return PlugComponent(component); ;
        }

        #endregion

        #region private

        /// <summary>
        /// Logger
        /// </summary>
        ILogger _logger;
        ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = Log.SysLogger;
                }
                return _logger;
            }
            set
            {
                _logger = value;
            }
        }

        #endregion
    }
}