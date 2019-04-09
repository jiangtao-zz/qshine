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
using qshine.Configuration.ConfigurationStore;

#if NETCORE
using System.Runtime.Loader;
#endif

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
        bool _initialized = false;
        #region Ctor
        /// <summary>
        /// Ctor:: build default application environment
        /// </summary>
        public ApplicationEnvironment()
            : this("", new EnvironmentInitializationOption())
        {
        }

        /// <summary>
        /// Ctor:: build named application environment by given options.
        /// </summary>
        /// <param name="name">Name application environment</param>
        /// <param name="options">Options to initialize application environment.</param>
        public ApplicationEnvironment(string name, EnvironmentInitializationOption options)
        {
            //init fields
            _options = options ?? new EnvironmentInitializationOption();

            EnvironmentConfigure = new EnvironmentConfigure();
            Name = string.IsNullOrEmpty(name) ? string.Empty : name;

            //Set current Application Environment context
            var contextName = GetEnvironmentContextName(name);
            EnvironmentContext.SetData(contextName, this);

            ConfigurationStore = new XmlConfigurationStore();

            //Initialize application environment
            Init();
        }
        #endregion

        #region public properties

        /// <summary>
        /// Configuration Store
        /// </summary>
        public IConfigurationStore ConfigurationStore { get; set; }

        /// <summary>
        /// Name of the application environment
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Get Environment Configure setting
        /// </summary>
        public EnvironmentConfigure EnvironmentConfigure { get; private set; }

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
        /// The reason of application environment construction failure. 
        /// </summary>
        public string InvalidReason { get; set; }

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

        #endregion

        #region public Methods

        /// <summary>
        /// it will invoke all instances that implemented type T interface or base class.
        /// The type T constructor()
        /// </summary>
        /// <typeparam name="T">An interface or base class for start up class implementation.</typeparam>
        public ApplicationEnvironment StartUp<T>()
        {
            var types = PluggableAssembly.SafeGetInterfacedTypes(typeof(T));

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

        #region CreateProvider/CreateMappedProvider

        /// <summary>
        /// Create a specific interface type provider. The provider interface must inherit from IProvider
        /// If the provider name is specified it create instance from named provider.
        /// Get default or first interface provider if the name is not specified.
        /// </summary>
        /// <typeparam name="T">Specifies provider interface type or base type.</typeparam>
        /// <param name="name">Provider name. If name is not specified, it will create default or first provider instance.
        /// The default one can be configured in provider Maps section.</param>
        /// <returns></returns>
        public T CreateProvider<T>(string name = "")
            where T : IProvider
        {
            return (T)CreateProvider(typeof(T), name);
        }

        /// <summary>
        /// Create a specific interface type provider. The provider interface must inherit from IProvider.
        /// If the provider name is specified it create instance from named provider.
        /// Get default or first interface provider if the name is not specified.
        /// </summary>
        /// <param name="providerInterface">Specifies provider interface type or base type.</param>
        /// <param name="name">Provider name. If name is not specified, it will create default or first provider instance.
        /// The default one can be configured in provider Maps section.</param>
        /// <returns>Returns a given interface type instance.</returns>
        public IProvider CreateProvider(Type providerInterface, string name="")
        {
            var component = CreateComponent(providerInterface, name);

            return (component != null) ? component as IProvider : null;
        }

        /// <summary>
        /// Create a mapped specific interface type provider. The provider interface must inherit from IProvider.
        /// If the mapped name is specified it create instance from named provider.
        /// </summary>
        /// <typeparam name="T">The provider interface type.</typeparam>
        /// <param name="mapKey">The provider map name. 
        /// If the mapKey is not configured or the mapped provider is invalid, it will create the default typed provider instance instead.</param>
        /// <returns>Returns mapped provider instance</returns>
        public T CreateMappedProvider<T>(string mapKey)
            where T : IProvider
        {
            return (T)CreateMappedProvider(typeof(T), mapKey);
        }

        /// <summary>
        /// Create a mapped specific interface type provider. The provider interface must inherit from IProvider.
        /// If the mapped name is specified it create instance from named provider.
        /// </summary>
        /// <param name="providerInterface">The provider interface type.</param>
        /// <param name="mapKey">The provider map name. 
        /// If the mapKey is not configured or the mapped provider is invalid, it will create the default typed provider instance instead.</param>
        /// <returns>Returns mapped provider instance</returns>
        public IProvider CreateMappedProvider(Type providerInterface, string mapKey)
        {
            var component = CreatedMappedComponent(providerInterface, mapKey);

            return (component != null) ? component as IProvider : null;
        }

        /// <summary>
        /// Get all given type of providers from configured components
        /// </summary>
        /// <typeparam name="T">type of provider interface</typeparam>
        /// <returns></returns>
        public IList<T> GetProviders<T>()
            where T : IProvider
        {
            if (!CheckInitialized()) return null;

            return EnvironmentConfigure.Components.Where(
                x => x.Value.InterfaceType != null &&
                x.Value.InterfaceType == typeof(T) &&
                x.Value.ClassType != null)
                .Select(
                    y => (T)y.Value.CreateInstance()
                    )
                .ToList();
        }

        /// <summary>
        /// Create a specific interface type component instance.
        /// If the component name is specified it create instance from named component.
        /// Get default or first typed component if the name is not specified.
        /// </summary>
        /// <typeparam name="T">The interface type of the component.</typeparam>
        /// <param name="name">The component name defined in component setting configure. If name is blank, it will try to get default or first component listed in configure setting.</param>
        /// <returns>returns typed component instance</returns>
        public T CreateComponent<T>(string name = "")
            where T : class
        {
            var component = CreateComponent(typeof(T), name);

            return (component != null) ? component as T : null;
        }

        /// <summary>
        /// Create a specific interface type component instance.
        /// If the component name is specified it create instance from named component.
        /// Get default or first typed component if the name is not specified.
        /// </summary>
        /// <param name="interfaceType">The interface type of the component.</param>
        /// <param name="name">The component name defined in component setting configure. If name is blank, it will try to get default or first component listed in configure setting.</param>
        /// <returns>returns typed component instance object</returns>
        public object CreateComponent(Type interfaceType, string name = "")
        {
            if (!CheckInitialized()) return null;

            //provider always be a single instance
            PlugableComponent provider = GetPluggableComponent(interfaceType, name);

            object instance = null;

            if (provider != null)
            {
                instance = provider.CreateInstance();
            }

            return instance;
        }

        /// <summary>
        /// Get or create a mapped provider.
        /// A mapped provider is one of the provider built from configure file component maps setting.
        /// The component map setting is a list of map key and provider name pair. 
        /// Provider configure setting map sample:
        /// <![CDATA[
        ///     <maps name="providerTypeName" default="defaultProviderName">
        ///         <add key="mapKey" value="providerName" />
        ///     </maps>
        ///     <components>
        ///         <component name="providerName1" interface="Ixxx" type="xxx" />
        ///         <component name="providerName" interface="Ixxx" type="xxx2" />
        ///     </components>
        /// ]]>
        /// </summary>
        /// <typeparam name="T">Provider interface type</typeparam>
        /// <param name="mapKey">A map key associated to named provider. If the mapKey or associated provider name is blank, get the default provider</param>
        /// <returns>A provider instance mapped mapKey</returns>
        public T CreateMappedComponent<T>(string mapKey)
            where T : class
        {
            return (T)CreatedMappedComponent(typeof(T), mapKey);
        }

        /// <summary>
        /// See above CreatedMappedComponent() comments.
        /// </summary>
        /// <param name="componentInterfaceType"></param>
        /// <param name="mapKey"></param>
        /// <returns></returns>
        public object CreatedMappedComponent(Type componentInterfaceType, string mapKey)
        {
            var componentName = GetMappedComponentName(componentInterfaceType, mapKey);
            return CreateComponent(componentInterfaceType, componentName);
        }

        #endregion

        /// <summary>
        /// Get component interface mapped name.
        /// </summary>
        /// <param name="componentInterfaceType">Component interface type</param>
        /// <param name="mapKey">Component map key</param>
        /// <returns>Returns configured component map key.</returns>
        public string GetMappedComponentName(Type componentInterfaceType, string mapKey)
        {
            //Map name usually is the type name
            string componentMapName = Map.GetMapName(componentInterfaceType);

            var maps = EnvironmentConfigure.Maps;
            if (maps != null)
            {
                //if the component map is found
                if (maps.ContainsKey(componentMapName))
                {
                    //get particular provider map and find mapped provider name
                    var componentMaps = maps[componentMapName];
                    if (componentMaps != null && componentMaps.ContainsKey(mapKey))
                    {
                        return componentMaps[mapKey];
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Get specific interface plugin component by given name. if the name is not specified, it returns default or first typed component.
        /// </summary>
        /// <param name="interfaceType">Specifies component interface type</param>
        /// <param name="name">Specifies the component name. If teh name is blank, it will get default or first component.
        /// if plugin associated to a maps collection, the name parameter will be a map key instead a component name.
        /// The component name is a map key associated value.
        /// </param>
        /// <returns>pluggable component</returns>
        public PlugableComponent GetPluggableComponent(Type interfaceType, string name="")
        {
            bool defaultOrFirst = string.IsNullOrEmpty(name); //search for default or first

            //if name is blank, try to find default provider name from component map.
            //  <maps name="interafceType" default="defaultComponentname">
            //    <add key="mapKey" value="providerName" />
            //  </maps>
            //
            var map = GetComponentMap(interfaceType);
            if(map!=null)
            {
                //Get default mapped component if the name is not specified.
                if (defaultOrFirst && !string.IsNullOrEmpty(map.Default))
                {
                    name = map.Default;
                }
                if(!string.IsNullOrEmpty(name))
                {
                    //try to find first matched map key
                    var matchedMap = map.Items.Keys.FirstOrDefault(x => name.Match(x));
                    if (matchedMap != null)
                    {
                        //get mapped key
                        name = map.Items[matchedMap];
                    }
                }
            }

            PlugableComponent pluginComponent = null;
            foreach (var component in EnvironmentConfigure.Components)
            {
                if (component.Value.InterfaceType != null && component.Value.InterfaceType.Name == interfaceType.Name)
                {
                    if (component.Value.Name == name)
                    {
                        //found named component
                        pluginComponent = component.Value;
                        break;
                    }

                    if (defaultOrFirst)
                    {
                        //reserve first component and continue keep looking for named component. 
                        pluginComponent = component.Value;
                        defaultOrFirst = false;
                    }
                }
            }

            if (pluginComponent != null)
            {
                pluginComponent.Instantiate();
            }

            return pluginComponent;
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

        #endregion

        #region private

        const string environmentContextStoreName = "_$$envContext.current";

        EnvironmentInitializationOption _options;

        List<string> _externalDepsJsonPaths = new List<string>();

        /// <summary>
        /// Load assembly from plugin folder
        /// </summary>
        /// <param name="path">path of plugin component</param>
        /// <returns></returns>
        Assembly SafeLoadAssembly(string path)
        {
            try
            {
#if NETCORE
                var assembly = ApplicationAssemblyResolver.Resolve(path);
#else
            var assembly = Assembly.LoadFrom(path);
#endif
                return assembly;
            }
            catch (Exception ex)
            {
                AddInnerException("AE:: Failed to load assembly path {0}. ({1})", path, ex.Message);
                return null;
            }
        }

        void AddInnerException (string format, params object[] args)
        {
            if (_options.InnerException == null)
            {
                _options.InnerException = new ConfigurationException();
            }
            string error = string.Format(format, args);
            _options.InnerException.InnerErrorMessages.Add(error);

        }

        bool CheckInitialized()
        {
            if (!_initialized)
            {
                InvalidReason = "The application environment has not been initialized.";
            }
            return _initialized;
        }


        /// <summary>
        /// Initializes the <see cref="T:qshine.Configuration.ApplicationEnvironment"/>.
        /// </summary>
        void Init()
        {
            //Set application assembly resolver for find pluggable assembly
            SetAssemblyResolve();

            //Raise enter event
            var eventArg = new InterceptorEventArgs("Init", Name, _options);
            _interceptor.RaiseOnEnterEvent(this, eventArg);

            //Load configure setting
            if (ConfigurationStore != null)
            {
                EnvironmentConfigure = ConfigurationStore.LoadConfig(_options);
            }

            //Load binary setting
            LoadBinaryFiles();

            //Load components from binary assembly
            LoadComponents();

            //Load modules
            LoadModules();

            //Load intercept handlers after all plugin components loaded
            LoadInterceptHandlers();

            //Component loaded
            _initialized = true;

            //Raise completion event
            _interceptor.RaiseOnSuccessEvent(this, eventArg);
        }

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

        [SecuritySafeCritical]
        void SetAssemblyResolve()
        {
#if NETCORE
            AssemblyLoadContext.Default.Resolving += OnResolving;
#else
            AppDomain.CurrentDomain.AssemblyResolve += ApplicationAssemblyResolve;
#endif
        }

#if NETCORE
        private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
        {
            return ApplicationAssemblyResolve(context, new ResolveEventArgs(name.FullName));
        }

#endif

        /// <summary>
        /// Resolve assembly location when lookup type by a qualified type name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Assembly ApplicationAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Ignore missing resources
            if (args.Name.Contains(".resources")) return null;

            var assemblyParts = args.Name.Split(',');
            //Should never happen
            if (assemblyParts.Length == 0) return null;

            var assemblyName = assemblyParts[0];

            //1. ** If assembly alreay in mapping list, directly load the assembly**
            //We only allow single version assembly loaded from mapping list.
            //Load sequence is based on searching order starting from application configuration entrypoint.
            //To avoid version conflict, DO NOT add multiple versions in configuration binary folder.
            Assembly assembly = PluggableAssembly.GetAssembly(assemblyName);

            if (assembly != null) return assembly;

            ////2. ** try to load assembly from default load context ***
            ////The default load context runtime components should be loaded first before load other components to "load-from context".
            ////Due to "default load context" component cannot load dependency from other context. if dependency assembly also exists 
            ////in other configuration binary folder, try load dependency dll earlier before load plugable component. 
            //// check for assemblies already loaded by the current application domain. It is necessary. See. 
            //// https://docs.microsoft.com/en-us/dotnet/framework/deployment/best-practices-for-assembly-loading
            {
                var pluggableAssembly = PluggableAssembly.GetPluggableAssembly(assemblyName);

                //Try to load assembly from different application configuration folders
                if (pluggableAssembly==null)
                {
                    //Try to load configuration binary folders into mapping list, if not loaded yet
                    LoadBinaryFiles();
                }

                //3. ** Try get assembly from "load-from context" and put in mapping list
                pluggableAssembly = PluggableAssembly.GetPluggableAssembly(assemblyName);

                if (pluggableAssembly!=null)
                {
                    //Assembly already in "load-from context" 
                    if (pluggableAssembly.Assembly != null)
                    {
                        return pluggableAssembly.Assembly;
                    }
                    //Load assembly from configured path and add in "load-from context"
                    //This may throw exception. The exception will be captured when call assemby Load()
#if NETCORE
                    assembly = ApplicationAssemblyResolver.Resolve(pluggableAssembly.Path);
#else
                    assembly = Assembly.LoadFrom(pluggableAssembly.Path);
#endif
                    if (assembly != null)
                    {
                        pluggableAssembly.Assembly = assembly;
                    }
                }
                else
                {
                    //Couldn't find assembly
                    AddInnerException("AE.AssemblyResolve:: Couldn't find assembly {0}", args.Name);
                }
            }

            return assembly;
        }

        /// <summary>
        /// Load components from application configuration file
        /// </summary>
		void LoadComponents()
        {
            foreach (var c in EnvironmentConfigure.Components)
            {
                LoadComponent(c.Value);
            }
        }

        /// <summary>
        /// Load modules from application configuration file
        /// </summary>
		void LoadModules()
        {
            foreach (var c in EnvironmentConfigure.Modules)
            {
                LoadModule(c.Value);
            }
        }

        /// <summary>
        /// Try to load a module.
        /// </summary>
        /// <param name="module">Module is a plugin component that will be auto loaded through application eenvironment.
        /// The module initialization could be implemented in type constructor or type static constructor (If initialization call only once.).
        /// </param>
        /// <remarks>
        /// The module must have a public constructor without parameter. 
        /// </remarks>
        void LoadModule(PlugableComponent module)
        {
            if (module.Instantiate())
            {
                module.CreateInstance();
            }
            else
            {
                //do not raise exception.
                AddInnerException("AE.LoadModule:: {0}", module.FormatObjectValues());
            }
        }

        void LoadComponent(PlugableComponent component)
        {
            component.Instantiate();
            Logger.Info("AE.LoadComponent:: {0}", component.FormatObjectValues());
        }

        /// <summary>
        /// Add all assembly files from binary folders into application environment.
        /// The AssemblyResolveHandler try to resolve assembly from those files.
        /// </summary>
		bool LoadBinaryFiles()
        {
            bool hasNewBinaryFile = false;

            foreach (var binPath in EnvironmentConfigure.AssemblyFolders)
            {
                if (!binPath.State && Directory.Exists(binPath.ObjectData))
                {
#if NETCORE
                    foreach (var depFile in new DirectoryInfo(binPath.ObjectData).GetFiles("*.deps.json"))
                    {
                        if (!_externalDepsJsonPaths.Contains(depFile.FullName))
                        {
                            _externalDepsJsonPaths.Add(depFile.FullName);
                        }
                    }
#endif

                    //bool hasBinary = false;
                    foreach (var dll in new DirectoryInfo(binPath.ObjectData).GetFiles("*.dll"))
                    {
                        var assemblyName = Path.GetFileNameWithoutExtension(dll.FullName);
                        var pluggableAssembly = PluggableAssembly.GetPluggableAssembly(assemblyName);

                        if (pluggableAssembly==null)
                        {
                            PluggableAssembly.AddAssembly(assemblyName, new PluggableAssembly
                            {
                                Path = dll.FullName,
                                Assembly = SafeLoadAssembly(dll.FullName)

                            });
                            Logger.Info("AE.LoadBinary:: Add assembly {0}", dll.FullName);
                            //Found new binary file.
                            hasNewBinaryFile = true;
                        }
                        else if (pluggableAssembly.Path != dll.FullName)
                        {
                            Logger.Info("AE.LoadBinary:: Ignore assembly {0}", dll.FullName);
                        }
                        //hasBinary = true;
                    }
                    //Mark the flag that assembly folder dlls loaded in folder mapping list
                    binPath.State = true;
                }
            }
            return hasNewBinaryFile;
        }

        /// <summary>
        /// Get the component map
        /// </summary>
        /// <param name="componentType">type of component</param>
        /// <returns>type specific component map</returns>
        Map GetComponentMap(Type componentType)
        {
            return GetEnvironmentMap(componentType.FullName);
        }

        /// <summary>
        /// Get map by name
        /// </summary>
        /// <param name="mapName">map name</param>
        /// <returns>return a environment map</returns>
        Map GetEnvironmentMap(string mapName)
        {
            var maps = EnvironmentConfigure.Maps;
            if (maps != null && maps.ContainsKey(mapName))
            {
                //get particular provider map and find mapped provider name
                return maps[mapName];
            }
            return null;
        }


#endregion
    }
}