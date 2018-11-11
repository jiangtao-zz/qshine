using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.Linq;
using System.Configuration;
using qshine.Utility;
using System.Xml;
using System.Dynamic;
using System.Data;
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
            if (string.IsNullOrEmpty(name))
            {
                name = string.Empty;
            }
            Name = name;

            if (options == null)
            {
                options = new EnvironmentInitializationOption();
            }
            _options = options;

            EnvironmentConfigure = new EnvironmentConfigure();
            _configFiles = new SafeDictionary<string, string>();
            _modules = new SafeDictionary<string, IModuleLoader>();

            Init();
        }
        #endregion

        #region public properties
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

        #endregion

        #region public Methods

        /// <summary>
        /// Create a named provider by given type (or interface)
        /// </summary>
        /// <param name="name">provider name. If name is not provided, it will create first given type instance.</param>
        /// <param name="providerInterface">provider interface type or base type or given class type.</param>
        /// <returns>Returns a given type instance.</returns>
        public IProvider CreateProvider(string name, Type providerInterface)
        {
            PlugableComponent providerComponent = null;
            foreach (var component in EnvironmentConfigure.Components)
            {
                if (component.Value.InterfaceType != null && component.Value.InterfaceType.Name == providerInterface.Name &&
                    (component.Value.Name == name || string.IsNullOrEmpty(name)))
                {
                    providerComponent = component.Value;
                    break;
                }
            }

            if (providerComponent != null && providerComponent.ClassType != null)
            {
                return CreateInstance(providerComponent.ClassType, providerComponent.Parameters.Values.ToArray()) as IProvider;
            }
            return null;
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
            if (_configFiles.ContainsKey(fileName))
            {
                return _configFiles[fileName];
            }
            return null;
        }

        /// <summary>
        /// Load a qualified type without throw exception
        /// </summary>
        /// <returns>The type or null.</returns>
        /// <param name="typeValue">Type value.</param>
        public Type SafeLoadType(string typeValue)
        {
            Type type = null;
            try
            {
                type = Type.GetType(typeValue);

                if (type == null)
                {
                    //This should not happen. Just in case type cannot be resolved by previous assembly_resolver, need load type from assembly directly
                    var assemblyNameParts = typeValue.Split(',');
                    if (assemblyNameParts.Length > 1)
                    {
                        var assemblyName = assemblyNameParts[1].Trim();
                        var typeNameOnly = assemblyNameParts[0].Trim();
                        if (_assemblyMaps.ContainsKey(assemblyName))
                        {
                            var assembly = _assemblyMaps[assemblyName].Assembly;
                            if (assembly != null)
                            {
                                type = assembly.GetType(typeNameOnly, true);
                            }
                        }
                    }
                }
                if (type == null)
                {
                    Logger.Error("AE.SafeLoadType:: Invalid type [{0}].", typeValue);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AE.SafeLoadType:: {0} throw exception: {1}", typeValue, ex.Message);
            }
            return type;
        }

        #endregion

        #region private

        const string environmentContextStoreName = "_$$envContext.current";
        /// <summary>
        /// Contains all environment configure files 
        /// </summary>
        SafeDictionary<string, string> _configFiles;

        /// <summary>
        /// Contains all named modules from configure environment.
        /// A module is an auto loaded component which can perform self initialization. 
        /// </summary>
        SafeDictionary<string, IModuleLoader> _modules;

        EnvironmentInitializationOption _options;

        List<string> _externalDepsJsonPaths = new List<string>();

        /// <summary>
        /// Initializes the <see cref="T:qshine.Configuration.ApplicationEnvironment"/>.
        /// </summary>
        void Init()
        {
            Logger.Info("AE.Init {0} begin", Name);

            //Load all domain user dlls
            MapRuntimeComponents();

            //Load intercept handlers before load plugin components
            //LoadInterceptHandlers();

            //Set application assembly resolver
            SetAssemblyResolve();


            //Load configure setting
            LoadConfig(_options.RootConfiguration==null?_options.RootConfigFile:null);
            //Load binary setting
            LoadBinaryFiles();

            //Load type from binary assembly
            LoadComponents();
            //Load modules
            LoadModules();


            //Load intercept handlers after plugin components loaded
            LoadInterceptHandlers();
            Logger.Info("AE.Init end {0}", Name);
        }

        ILogger _logger;
        ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = _options.Logger;
                }
                if (_logger == null)
                {
                    _logger = Log.SysLogger;
                }
                return _logger;
            }
        }

        /// <summary>
        /// Load application environment configuration from a specific configure files.
        /// It is a recursive function which will load user application environment configur files 
        /// from top to down until all level configure information loaded into EnvironmentConfigure object.
        /// </summary>
        /// <param name="configFile">Config file. The default is application domain configuration file: app.config or web.config</param>
        /// <returns>Return EnvironmentConfigure object.</returns>
        EnvironmentConfigure LoadConfig(string configFile = null)
        {
            System.Configuration.Configuration config;
            try
            {
                if (string.IsNullOrEmpty(configFile))
                {
                    // Get the current configuration file.
                    if (_options.RootConfiguration == null)
                    {
                        Logger.Info("AE.LoadConfig:: Open default config file.");
                        config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    }
                    else
                    {
                        //using option to get application configuration in case system cannot figure out the location of 
                        //the entrypoint config file.
                        Logger.Info("AE.LoadConfig:: Set option.RootConfiguration.");
                        config = _options.RootConfiguration;
                    }

                    var fileName = config.FilePath.ToLower();
                    _configFiles.Add(fileName, configFile);
                }
                else
                {
                    var fileName = Path.GetFileName(configFile).ToLower();
                    if (_configFiles.ContainsKey(fileName))
                    {
                        //Do not load the file if it already loaded
                        return EnvironmentConfigure;
                    }
                    _configFiles.Add(fileName, configFile);

                    var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = configFile };
                    Logger.Info("AE.LoadConfig:: Open config {0}.", configFile);
                    config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AE.LoadConfig:: {0} throw an exception {1}", configFile ?? "default", ex.Message);
                return EnvironmentConfigure;
            }

            if (config != null)
            {
                //Load ConnectionString sections first
                foreach (ConfigurationSection section in config.Sections)
                {
                    if (section is ConnectionStringsSection)
                    {
                        //Load connection strings
                        LoadDbConnectionStrings(section as ConnectionStringsSection);
                    }
                    else
                    {
                        var sectionInformation = section.SectionInformation;
                        if (sectionInformation != null && sectionInformation.Name == "system.data")
                        {
                            var rawXml = sectionInformation.GetRawXml();
                            if (rawXml != null)
                            {
                                var xmlHelper = new XmlHelper(rawXml);
                                var factories = xmlHelper.XmlSection;
                                if(factories.Items.Count>0 && factories.Items[0].Name== "DbProviderFactories"
                                    && factories.Items[0].Items.Count>0)
                                {
                                    foreach(var item in factories.Items[0].Items)
                                    {
                                        if (item.Name == "remove")
                                        {
                                            UnRegisterDbProviderFactory(item["invariant"]);
                                        }
                                        else if(item.Name == "add")
                                        {
                                            RegisterDbProviderFactory(item["invariant"],item["type"]);
                                        }
                                    }
                                }

                            }
                        }
                    }
                                    }

                                    //Load application environment specific sections
                foreach (var section in config.Sections)
                {
                    if (section is EnvironmentSection)
                    {
                        //Load environment section
                        LoadEnvironmentSection(section as EnvironmentSection);
                    }
                }
            }

            return EnvironmentConfigure;
        }

        /// <summary>
        /// Register DbProviderFactories for .NET CORE.
        /// .NET CORE doesn't implement the factory register.
        /// We have to add piece of code here to make it compatiable with >NET framework
        /// </summary>
        void RegisterDbProviderFactory(string invariantName, string assemblyQualifiedName)
        {
#if NETCORE
            System.Data.Common.DbProviderFactories.RegisterFactory(invariantName,  assemblyQualifiedName);                  
#else
            //var table = System.Data.Common.DbProviderFactories.GetFactoryClasses();
            //if (table != null)
            //{
            //    foreach (DataRow row in table.Rows)
            //    {
            //        if (row[2].ToString() == invariantName)
            //        {
            //            string name = (string)row[0];
            //            string description = (string)row[1];
            //            string invariant = (string)row[2];
            //            string assemblyType = (string)row[3];
            //            if (assemblyType != assemblyQualifiedName)
            //            {
            //                table.Rows.Remove(row);
            //                table.Rows.Add(name, description, invariant, assemblyQualifiedName);
            //                break;
            //            }
            //        }
            //    }
            //}
#endif
        }

        void UnRegisterDbProviderFactory(string invariantName)
        {
#if NETCORE
             System.Data.Common.DbProviderFactories.UnregisterFactory(invariantName);
#endif
        }

        /// <summary>
        /// Load connection strings
        /// </summary>
        /// <param name="section">ConnectionStrings section</param>
		void LoadDbConnectionStrings(ConnectionStringsSection section)
        {
            if (section != null)
            {
                //If the section is a protected (encrypted), unprotect section data
                //https://docs.microsoft.com/en-us/dotnet/api/system.configuration.rsaprotectedconfigurationprovider?redirectedfrom=MSDN&view=netframework-4.7.2
                if (section.SectionInformation.IsProtected)
                {
                    section.SectionInformation.UnprotectSection();
                }

                foreach (ConnectionStringSettings c in section.ConnectionStrings)
                {
                    Logger.Info("AE.LoadDbConnectionStrings:: {0}, {1}. Overwrite={2}", c.Name, c.ConnectionString, _options.OverwriteConnectionString);

                    EnvironmentConfigure.AddConnectionString(new ConnectionStringElement(c.Name, c.ConnectionString, c.ProviderName),
                        _options.OverwriteConnectionString);
                }
            }
        }

        /// <summary>
        /// Load environment section
        /// </summary>
        /// <param name="section">Application Environment section</param>
		void LoadEnvironmentSection(EnvironmentSection section)
        {
            if (section != null)
            {
#region Load Components
                foreach (var component in section.Components)
                {
                    Logger.Info("AE.LoadEnvironmentSection:: Add Component {0}, Overwrite={1}", component.Name, _options.OverwriteComponent);
                    EnvironmentConfigure.AddComponent(component, _options.OverwriteComponent);
                }
#endregion

#region Load Modules
                foreach (var module in section.Modules)
                {
                    Logger.Info("AE.LoadEnvironmentSection:: Add Module {0}, Overwrite={1}", module.Name, _options.OverwriteModule);
                    EnvironmentConfigure.AddModule(module, _options.OverwriteModule);
                }
#endregion

#region Load Applistion Settings
                foreach (var setting in section.AppSettings)
                {
                    Logger.Info("AE.LoadEnvironmentSection:: App Settings {0} = {1}, Overwrite={2}", setting.Key, setting.Value, _options.OverwriteAppSetting);

                    //Add or Update
                    if (!EnvironmentConfigure.AppSettings.ContainsKey(setting.Key))
                    {
                        EnvironmentConfigure.AppSettings.Add(setting.Key, setting.Value);
                    }
                    if (_options.OverwriteAppSetting)
                    {
                        EnvironmentConfigure.AppSettings[setting.Key] = setting.Value;
                    }
                }
#endregion

#region Load other level configures
                foreach (var environment in section.Environments)
                {
                    Logger.Info("AE.LoadEnvironmentSection:: Add Environment {0}", environment.Name);
                    EnvironmentConfigure.AddEnvironment(environment);
                    //Only load environment related setting
                    if (string.IsNullOrEmpty(environment.Host) ||//match all
                        environment.Host == Environment.MachineName || //machine name match
                       environment.Host == EnvironmentEx.MachineIp) //Ip match
                    {

                        var folder = Path.GetDirectoryName(section.CurrentConfiguration.FilePath);
                        var path = UnifiedFullPath(folder, environment.Path);
                        if (!Directory.Exists(path))
                        {
                            Logger.Warn("AE.LoadEnvironmentSection:: Config path {0} does not exist.", path);
                        }
                        else if (!EnvironmentConfigure.ConfigureFolders.Contains(path))
                        {
                            EnvironmentConfigure.ConfigureFolders.Add(path);

                            string binfolders = "bin";
                            if (!string.IsNullOrEmpty(environment.Bin))
                            {
                                binfolders = environment.Bin;
                            }

                            var folders = binfolders.Split(',');
                            if (folders.Length > 0)
                            {
                                foreach (var binfolder in folders)
                                {
                                    if (!string.IsNullOrEmpty(binfolder))
                                    {
                                        var binFolder = Path.Combine(Path.GetFullPath(path), binfolder);
                                        SetBinaryFolders(binFolder);
                                    }
                                }
                            }

                            //Find all configure files and load all
                            var files = Directory.GetFiles(path, _options.ConfigureFilePattern);
                            if (files != null)
                            {
                                for (int i = 0; i < files.Length; i++)
                                {
                                    LoadConfig(files[i]);
                                }
                            }
                        }
                    }
                }
#endregion
            }
        }

        /// <summary>
        /// Add configured binary folders in binary components search path.
        /// The available binary folders could be:
        /// 1. [given binary folder] ex: bin. Usually, add common non-version specific component
        /// 2. [given binary folder]/[qshine version folder] ex: bin/2.1. Usually, add qshine extension components
        /// 3. [given binary folder]/[Microsoft .NET version folder] ex: bin/net461 or bin/netcoreapp2.1. Usually, add components built with specific .NET library
        /// 4. [given binary folder]/[cpu architecture folder] ex: bin/x86 or bin/x64. Usually, add 3rd-party plug-in components
        /// The x86 only components should be in x86 folder.
        /// The x64 only components should be in x64 folder.
        /// The Any CPU components should be in bin folder directly.
        /// </summary>
        /// <param name="binFolder"></param>
		void SetBinaryFolders(string binFolder)
        {
            if (Directory.Exists(binFolder) && !EnvironmentConfigure.AssemblyFolders.Any(x => x.ObjectData == binFolder))
            {
                Logger.Info("AE.SetBinaryFolders:: Add folder {0}", binFolder);

                //add specified binary folder
                EnvironmentConfigure.AssemblyFolders.Add(new StateObject<bool, string>(false, binFolder));

                //add nearest qshine framework version specific binary folder if exists.
                for (int i = GetFrameworkVersionPaths().Length - 1; i >= 0; i--)
                {
                    var versionPath = Path.Combine(binFolder, GetFrameworkVersionPaths()[i]);
                    if (Directory.Exists(versionPath) && !EnvironmentConfigure.AssemblyFolders.Any(x => x.ObjectData == versionPath))
                    {
                        EnvironmentConfigure.AssemblyFolders.Add(new StateObject<bool, string>(false, versionPath));

                        Logger.Info("AE.SetBinaryFolders:: Add version path {0}", versionPath);
                        break;
                    }
                }

                //add cpu architecture binary folder
                var cpuArchitecturePath = Path.Combine(binFolder, EnvironmentEx.CpuArchitecture);
                if (Directory.Exists(cpuArchitecturePath) && !EnvironmentConfigure.AssemblyFolders.Any(x => x.ObjectData == cpuArchitecturePath))
                {
                    EnvironmentConfigure.AssemblyFolders.Add(new StateObject<bool, string>(false, cpuArchitecturePath));

                    Logger.Info("AE.SetBinaryFolders:: Add CPU Architecture path {0}", cpuArchitecturePath);
                }

                //add qshine components built from specific .net library folder
                if (!string.IsNullOrEmpty(EnvironmentEx.TargetFramework))
                {
                    //add cpu architecture binary folder
                    var targetDotNetFrameworkPath = Path.Combine(binFolder, EnvironmentEx.TargetFramework);
                    if (Directory.Exists(targetDotNetFrameworkPath) && !EnvironmentConfigure.AssemblyFolders.Any(x => x.ObjectData == targetDotNetFrameworkPath))
                    {
                        EnvironmentConfigure.AssemblyFolders.Add(new StateObject<bool, string>(false, targetDotNetFrameworkPath));

                        Logger.Info("AE.SetBinaryFolders:: Add Net target framework path {0}", targetDotNetFrameworkPath);
                    }
                }
            }

        }

        /// <summary>
        /// Filter assemblies before add it into candidate assembly list
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        bool IsCandidateAssembly(Assembly assembly)
        {
            //Ignore dynamic assembly
            if (assembly.IsDynamic) return true;

            string location = assembly.Location;
            var fullName = assembly.FullName;
            bool isCandidateAssembly = String.IsNullOrEmpty(location) || //ignore byte array loaded assembly
                assembly.ManifestModule.Name == "<In Memory Module>" || //ignore in memory module
                location.IndexOf("App_Web", StringComparison.Ordinal) > -1 || //ignore web dynamic compile dlls
                location.IndexOf("App_global", StringComparison.Ordinal) > -1 || //ignore web app resource dlls
                location.IndexOf("Microsoft", StringComparison.OrdinalIgnoreCase) > -1 || //ignore microsoft resource dlls
                fullName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) || //ignore Microsoft dlls
                fullName.StartsWith("System.", StringComparison.OrdinalIgnoreCase) || //ignore Microsoft System dlls
                fullName.StartsWith("mscorlib,", StringComparison.Ordinal) || //ignore Run-time Core
                fullName.StartsWith("runtime.", StringComparison.OrdinalIgnoreCase) || //ignore Run-time Core
                fullName.IndexOf("CppCodeProvider", StringComparison.Ordinal) > -1 || //ignore code dom provider
                fullName.IndexOf("SMDiagnostics", StringComparison.Ordinal) > -1 || //WCF
                fullName.StartsWith("WebMatrix.", StringComparison.Ordinal) || // ignore MS web secuirty dll
                fullName.StartsWith("WindowsBase.", StringComparison.Ordinal) || //WPF
                fullName.StartsWith("NETStandard.Library", StringComparison.OrdinalIgnoreCase) || //ignore .NET Core
                fullName.StartsWith("WindowsAzure.Storage", StringComparison.OrdinalIgnoreCase) || //ignore Azure storage
                fullName.StartsWith("nunit,", StringComparison.Ordinal) ||
                fullName.StartsWith("Ninject,", StringComparison.Ordinal) ||
                fullName.StartsWith("Castle.", StringComparison.Ordinal) ||
                fullName.StartsWith("Typemock", StringComparison.OrdinalIgnoreCase) ||
                fullName.StartsWith("Telerik.", StringComparison.Ordinal);

            if (!isCandidateAssembly && _options.IsCandidateAssembly != null)
            {
                isCandidateAssembly = !_options.IsCandidateAssembly(assembly);
            }
            return isCandidateAssembly;
        }

        /// <summary>
        /// Map application components from runtime location to application environment.
        /// Those components types could be resolved directly from run-time.
        /// The mapped runtime application components will be part of accessable types for plugable application environment.
        /// It will not include most system or common share components.
        /// </summary>
        void MapRuntimeComponents()
        {
            Assembly[] runtimeAssemblies = _options.RuntimeComponents;

            if (runtimeAssemblies == null)
            {
                runtimeAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            }


            foreach (var a in runtimeAssemblies)
            {
                //Skip system assemblies
                if (IsCandidateAssembly(a))
                {
                    continue;
                }

                //Get assembly name without version and culture info.
                var assemblyNameObject = new AssemblyName(a.FullName);
                var assemblyName = assemblyNameObject.Name;

                if (!_assemblyMaps.ContainsKey(assemblyName))
                {
                    _assemblyMaps.Add(assemblyName, new PlugableAssembly
                    {
                        Path = a.FullName,
                        Assembly = a
                    });
                }
            }
        }

        [SecuritySafeCritical]
        void SetAssemblyResolve()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ApplicationAssemblyResolve;
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
            if (_assemblyMaps.ContainsKey(assemblyName) && _assemblyMaps[assemblyName].Assembly != null)
            {
                return _assemblyMaps[assemblyName].Assembly;
            }

            //2. ** try to load assembly from default load context ***
            //The default load context runtime components should be loaded first before load other components to "load-from context".
            //Due to "default load context" component cannot load dependency from other context. if dependency assembly also exists 
            //in other configuration binary folder, try load dependency dll earlier before load plugable component. 
            // check for assemblies already loaded by the current application domain. It is necessary. See. 
            // https://docs.microsoft.com/en-us/dotnet/framework/deployment/best-practices-for-assembly-loading
            if (_options.RuntimeComponents == null)
            {
                _options.RuntimeComponents = AppDomain.CurrentDomain.GetAssemblies();
            }
            Assembly[] assemblies = _options.RuntimeComponents;
            var assembly = assemblies.FirstOrDefault(a => a.GetName().FullName.Equals(args.Name) ||
                                                    a.GetName().Name.Equals(args.Name) ||
                                                    a.GetName().Name.Equals(assemblyName));
            //Add "default load context" dlls in the mapping list
            if (assembly != null)
            {
                if (!_assemblyMaps.ContainsKey(assemblyName))
                {
                    _assemblyMaps[assemblyName] = new PlugableAssembly
                    {
                        Path = assembly.CodeBase
                    };
                }
                _assemblyMaps[assemblyName].Assembly = assembly;

                Logger.Info("AE.AssemblyResolve:: Loaded from default load context:: Assembly {0},{1}", assembly.FullName, assembly.CodeBase);
            }
            else
            {
                //Try to load assembly from different application configuration folders
                if (!_assemblyMaps.ContainsKey(assemblyName))
                {
                    //Try to load configuration binary folders into mapping list
                    LoadBinaryFiles();
                }

                //3. ** Try get assembly from "load-from context" and put in mapping list
                if (_assemblyMaps.ContainsKey(assemblyName))
                {
                    //Assembly already in "load-from context" 
                    if (_assemblyMaps[assemblyName].Assembly != null)
                    {
                        return _assemblyMaps[assemblyName].Assembly;
                    }
                    //Load assembly from configured path and add in "load-from context"
                    //This may throw exception. The exception will be captured when call assemby Load()
#if NETCORE
                    var assemblyResolver = new ApplicationAssemblyResolver(_assemblyMaps[assemblyName].Path);
                    assembly = assemblyResolver.Assembly;
                    if(assembly==null){
                        assemblyResolver.Dispose();
                    }
                    //assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(_assemblyMaps[assemblyName].Path);
#else
                    assembly = Assembly.LoadFrom(_assemblyMaps[assemblyName].Path);
#endif

                    _assemblyMaps[assemblyName].Assembly = assembly;
                }
                else
                {
#if NETCORE
                    if (args.RequestingAssembly != null && _externalDepsJsonPaths.Count > 0)
                    {
                        assembly = ApplicationAssemblyResolver.Resolve(_externalDepsJsonPaths, args.RequestingAssembly, args.Name);
                    }
#endif
                    //Couldn't find assembly
                    //Log.SysLogger.Warn("AE.AssemblyResolve:: Couldn't find assembly {0}", args.Name);
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
                LoadModule(c);
            }
        }

        void LoadModule(KeyValuePair<string, NamedTypeElement> module)
        {
            var instance = CreateInstance(SafeLoadType(module.Value.Type)) as IModuleLoader;
            if (instance != null)
            {
                if (!_modules.ContainsKey(module.Key))
                {
                    Logger.Info("AE.LoadModule:: {0}", module.FormatObjectValues());
                    _modules.Add(module.Key, instance);
                    instance.Initialize();
                }
            }
        }

        void LoadComponent(PlugableComponent component)
        {
            Logger.Info("AE.LoadComponent:: {0}", component.FormatObjectValues());

            component.InterfaceType = SafeLoadType(component.InterfaceTypeName);
            if (component.InterfaceType != null)
            {
                component.ClassType = SafeLoadType(component.ClassTypeName);
            }

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
                    bool hasBinary = false;
                    foreach (var dll in new DirectoryInfo(binPath.ObjectData).GetFiles("*.dll"))
                    {
                        var assemblyName = Path.GetFileNameWithoutExtension(dll.FullName);
                        if (!_assemblyMaps.ContainsKey(assemblyName))
                        {
                            _assemblyMaps.Add(assemblyName, new PlugableAssembly
                            {
                                Path = dll.FullName
                            });
                            Logger.Info("AE.LoadBinary:: Add assembly {0}", dll.FullName);
                            //Found new binary file.
                            hasNewBinaryFile = true;
                        }
                        else if (_assemblyMaps[assemblyName].Path != dll.FullName)
                        {
                            Logger.Info("AE.LoadBinary:: Ignore assembly {0}", dll.FullName);
                        }
                        hasBinary = true;
                    }
                    //Mark the flag that assembly folder dlls loaded in folder mapping list
                    binPath.State = true;
                    if (hasBinary)
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
                    }
                }
            }
            return hasNewBinaryFile;
        }

#endregion

#region public static methods and properties
        /// <summary>
        /// Get/Set current environment context store.
        /// The default current environment context store is Static Context store. It allows single current application environment.
        /// You may want to have a different context store to have current application environment based on different context. 
        /// In this case you can implement a special CurrentEnvironmentContextStore.
        /// The CurrentEnvironmentContextStore need be set in application Startup before ApplicationEnvironment.Boot()
        /// </summary>
        public static IContextStore CurrentEnvironmentContextStore
        {
            get
            {
                if (_contextStore == null)
                {
                    lock (lockObject)
                    {
                        if (_contextStore == null)
                        {
                            _contextStore = ContextManager.StaticContext;
                        }
                    }
                }
                return _contextStore;
            }
            set
            {
                lock (lockObject)
                {
                    _contextStore = value;
                }
            }
        }

        /// <summary>
        /// Get current application environment. The current application environment is a global
        /// </summary>
        public static ApplicationEnvironment Current
        {
            get
            {
                var current = CurrentEnvironmentContextStore.GetData(environmentContextStoreName) as ApplicationEnvironment;
                if (current == null)
                {
                    //Create default environment manager
                    lock (lockObject)
                    {
                        if (current == null)
                        {
                            current = new ApplicationEnvironment();
                            CurrentEnvironmentContextStore.SetData(environmentContextStoreName, current);
                        }
                    }
                }
                return current;
            }
        }

        /// <summary>
        /// Build and Initialize current application environment from a specific configure file.
        /// Ignore if current application environment already built.
        /// </summary>
        /// <param name="rootConfigFile">Specifies a root configure file which contains application environment setting. 
        /// If the root config file is not set, a default application configure file such as app.config, web.config or others loaded by host .net application as config file.</param>
        /// <remarks>
        /// The ApplicationEnvironment.Build() need be put in begin of the applciation execution path.
        /// Otherwise, default ApplicationEnvironment instance will be used.
        /// </remarks>
        public static void Build(string rootConfigFile = "")
        {
            Build("",
                new EnvironmentInitializationOption() {
                    RootConfigFile = rootConfigFile
                }
                );
        }

        /// <summary>
        /// Build and Initialize a named application environment with different options.
        /// Ignore if current application environment already built.
        /// </summary>
        /// <param name="name">name of application environment</param>
        /// <param name="options">Specifies options to initialize application environment</param>
        public static void Build(string name, EnvironmentInitializationOption options)
        {
            var contextName = environmentContextStoreName;
            if (!string.IsNullOrEmpty(name))
            {
                contextName += "_" + name;
            }

            if (options == null)
            {
                options = new EnvironmentInitializationOption();
            }

            var appEnvironment = CurrentEnvironmentContextStore.GetData(contextName) as ApplicationEnvironment;
            if (appEnvironment == null)
            {
                lock (lockObject)
                {
                    appEnvironment = CurrentEnvironmentContextStore.GetData(contextName) as ApplicationEnvironment;
                    if (appEnvironment == null)
                    {
                        appEnvironment = new ApplicationEnvironment(name, options);
                        CurrentEnvironmentContextStore.SetData(contextName, appEnvironment);
                    }
                }
            }
        }

        /// <summary>
        /// Gets a given type provider instance from current application environment.
        /// The provider must implemented IProvider interface.
        /// </summary>
        /// <returns>The provider instance.</returns>
        /// <typeparam name="T">The type of the provider.</typeparam>
        /// <remarks>The provider is a plugable component configured in application environment.
        /// It will load first defined component if many components found.
        /// </remarks>
        public static T GetProvider<T>()
        where T : class, IProvider
        {
            return GetProvider(typeof(T)) as T;
        }

        /// <summary>
        /// Gets the provider component by name from current application environment.
        /// The provider must implemented IProvider interface.
        /// </summary>
        /// <returns>The provider instance.</returns>
        /// <param name="name">Name of the provider.</param>
        /// <typeparam name="T">The type of the provider.</typeparam>
        /// <remarks>The provider is a plugable component configured in application environment with a given name.
        /// </remarks>
        public static T GetProvider<T>(string name)
        where T : class, IProvider
        {
            return GetProvider(name, typeof(T)) as T;
        }

        /// <summary>
        /// Gets a given type provider instance from current application environment.
        /// The provider must implemented IProvider interface.
        /// </summary>
        /// <returns>The provider instance.</returns>
        /// <param name="providerInterface">type of provider.</param>
        public static IProvider GetProvider(Type providerInterface)
        {
            return GetProvider(string.Empty, providerInterface);
        }

        /// <summary>
        /// Gets a named given type provider instance from current application environment.
        /// The provider must implemented IProvider interface.
        /// </summary>
        /// <returns>The provider instance.</returns>
        /// <param name="providerInterface">type of provider.</param>
        ///<param name="name">provider name</param>
        public static IProvider GetProvider(string name, Type providerInterface)
        {
            return Current.CreateProvider(name, providerInterface);
        }

        /// <summary>
        /// Creates the instance from a type.
        /// </summary>
        /// <returns>The instance.</returns>
        /// <param name="type">Type.</param>
        /// <param name="parameters">Parameters.</param>
        public static object CreateInstance(Type type, params object[] parameters)
        {
            if (type == null) return null;
            return Activator.CreateInstance(type, parameters);
        }

        /// <summary>
        /// Gets the configure.
        /// </summary>
        /// <value>The configure.</value>
        public static EnvironmentConfigure Configure
        {
            get
            {
                return Current.EnvironmentConfigure;
            }
        }

        /// <summary>
        /// Gets the assembly maps.
        /// </summary>
        /// <value>The assembly maps.</value>
        public static SafeDictionary<string, PlugableAssembly> AssemblyMaps
        {
            get
            {
                return _assemblyMaps;
            }
        }

        /// <summary>
        /// Gets the type by type name. The type name could be a qualified type name accessible by the environment.
        /// The plugable assembly always be accessible.
        /// </summary>
        /// <returns>The named type.</returns>
        /// <param name="typeName">Type name.</param>
        public static Type GetNamedType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
            {
                if (_commonNamedType.ContainsKey(typeName))
                {
                    return _commonNamedType[typeName];
                }

                type = _assemblyMaps.Values.Where(x => x.Assembly != null)
                    .Select(a => a.Assembly.GetType(typeName))
                    .FirstOrDefault(t => t != null);
                if (type != null)
                {
                    //thread-safe
                    if (!_commonNamedType.ContainsKey(typeName))
                    {
                        _commonNamedType.Add(typeName, type);
                    }
                }
            }
            return type;
        }

        public static IList<Type> SafeGetInterfacedTypes(Type interfaceType)
        {
            var selectedTypes = new List<Type>();
            foreach (var a in _assemblyMaps.Values)
            {
                if (a.Assembly != null)
                {
                    var types = SafeGetInterfacedTypes(a.Assembly, interfaceType);
                    if (types != null && types.Count > 0)
                    {
                        selectedTypes.AddRange(types);
                    }
                }
            }
            return selectedTypes;

        }

#endregion

#region private static
        static SafeDictionary<string, PlugableAssembly> _assemblyMaps = new SafeDictionary<string, PlugableAssembly>();
        static SafeDictionary<Type, object> _interceptHandlers = new SafeDictionary<Type, object>();
        static IContextStore _contextStore;
        static readonly SafeDictionary<string, Type> _commonNamedType = new SafeDictionary<string, Type>();

        static readonly object lockObject = new object();
        static IList<Type> SafeGetInterfacedTypes(Assembly assembly, Type interfacedType)
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (FileNotFoundException)
            {
                types = new Type[] { };
            }
            catch (NotSupportedException)
            {
                types = new Type[] { };
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null).ToArray();
            }
            return types.Where(t => interfacedType.IsAssignableFrom(t) && t.IsClass).ToList();
        }

        static string[] _versionPaths;
		/// <summary>
		/// Get qshine framework version paths.
		/// bin/1
		/// bin/1.2
		/// bin/1.2.3
		/// </summary>
		/// <returns>The version paths.</returns>
		static string[] GetFrameworkVersionPaths()
		{
			if (_versionPaths == null)
			{
				var callingAssembly = typeof(ApplicationEnvironment).Assembly;
				var info = FileVersionInfo.GetVersionInfo(callingAssembly.Location);
				_versionPaths  = info.FileVersion.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);


				string binPath = "";
				string version = "";
                for (var index = 0; index<_versionPaths.Length; index++)
                {
					version += index == 0 ? _versionPaths[index] : "." + _versionPaths[index];
					_versionPaths[index] = Path.Combine(binPath,version);
                }
			}
			return _versionPaths;
		}

        /// <summary>
        /// Return absolute path for a specified path. 
        /// If the specified path is a relative path, the absolute path is the combination of specified folder and relative path.
        /// </summary>
        /// <param name="folder">Specifies a folder for full path</param>
        /// <param name="path">Full path or relative path</param>
        /// <returns></returns>
		static string UnifiedFullPath(string folder, string path)
		{
			if (Path.IsPathRooted(path)) return path;

			return Path.Combine(folder, path);
		}

        static void LoadInterceptHandlers()
        {
            var types = SafeGetInterfacedTypes(typeof(IInterceptorHandler));
            foreach (var type in types)
            {
                Interceptor.RegisterHandlerType(type);
            }
        }
#endregion

    }

}