using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.Linq;
using qshine.Configuration;
using System.Configuration;
using qshine.Utility;

namespace qshine.Configuration
{
	public class EnvironmentManager
	{
		static Interceptor _configureSectionInterceptor;

        static string _defaultConfigureFile;
		/// <summary>
		/// Gets or sets the global interceptor.
		/// </summary>
		/// <value>The global interceptor.</value>
		public static Interceptor ConfigureSectionInterceptor
		{
			get { return _configureSectionInterceptor; }
		}

		public static void Boot(string rootConfigFile="")
		{
            _defaultConfigureFile = rootConfigFile;
            //Auto load configure when call the static method
            Init();

		}

		/// <summary>
		/// Gets the provider.
		/// </summary>
		/// <returns>The provider.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetProvider<T>()
		where T : class, IProvider
		{
			return GetProvider(typeof(T)) as T;
		}

		/// <summary>
		/// Gets the provider by name.
		/// </summary>
		/// <returns>The provider.</returns>
		/// <param name="name">Name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetProvider<T>(string name)
		where T : class, IProvider
		{
			return GetProvider(name, typeof(T)) as T;
		}


		/// <summary>
		/// Gets the provider.
		/// </summary>
		/// <returns>The provider.</returns>
		/// <param name="providerInterface">Provider interface.</param>
		public static IProvider GetProvider(Type providerInterface)
		{
			return GetProvider(string.Empty, providerInterface);
		}

		/// <summary>
		/// Gets the provider by name.
		/// </summary>
		/// <returns>The provider.</returns>
		/// <param name="providerInterface">Provider interface.</param>
		public static IProvider GetProvider(string name, Type providerInterface)
		{
            Init();
			PlugableComponent providerComponent = null;
			foreach (var component in _environmentConfigure.Components)
			{
				if (component.Value.InterfaceType!=null &&  component.Value.InterfaceType.Name == providerInterface.Name &&
				    (component.Value.Name==name ||string.IsNullOrEmpty(name)))
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
		/// Gets the first config file path searching through all config folders.
		/// </summary>
		/// <returns>The config file path if any or null.</returns>
		/// <param name="configFileName">Config file name.</param>
		public static string GetConfigFilePathIfAny(string configFileName)
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
		/// Gets the configure.
		/// </summary>
		/// <value>The configure.</value>
		public static EnvironmentConfigure Configure
		{
			get
			{
				return _environmentConfigure;
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

		static readonly SafeDictionary<string, Type> _commonNamedType = new SafeDictionary<string, Type>();
		static readonly object lockObject = new object();
		/// <summary>
		/// Gets the type by type name. The type name could be a qualified type name accessible by the environment.
		/// The plugable assembly always be accessible.
		/// </summary>
		/// <returns>The named type.</returns>
		/// <param name="typeName">Type name.</param>
		public static Type GetNamedType (string typeName)
		{
			var type = Type.GetType(typeName);
            if (type == null)
            {
                if (_commonNamedType.ContainsKey(typeName))
                {
                    return _commonNamedType[typeName];
                }

				type = AssemblyMaps.Values.Where(x=>x.Assembly!=null)
                    .Select(a => a.Assembly.GetType(typeName))
                    .FirstOrDefault(t => t != null);
                if (type != null)
                {
                    lock (lockObject)
                    {
                        if (!_commonNamedType.ContainsKey(typeName))
                        {
                            _commonNamedType.Add(typeName, type);
                        }
                    }
                }
            }
            return type;			
		}


		#region privates methods/properties

		static ILogger _sysLogger;
		static EnvironmentConfigure _environmentConfigure;
		static SafeDictionary<string, PlugableAssembly> _assemblyMaps;
		static SafeDictionary<string, string> _configFiles;
		static SafeDictionary<string, IModuleLoader> _modules;

		static bool isInit;
		static object lockEnvironment = new object();
		/// <summary>
		/// Initializes the <see cref="T:qshine.Configuration.EnvironmentManager"/> class when access any EnvironmentManager proeprty or method.
		/// </summary>
		static void Init()
		{
			if (!isInit)
			{
				lock (lockEnvironment)
				{
					if (!isInit)
					{
						Log.DevDebug("EnvironmentManager.Init begin");

						//Initial variables
						_environmentConfigure = new EnvironmentConfigure();
						_assemblyMaps = new SafeDictionary<string, PlugableAssembly>();
						_configFiles = new SafeDictionary<string, string>();
						_modules = new SafeDictionary<string, IModuleLoader>();
						_sysLogger = Log.SysLogger;

						//Load all domain user dlls
						MapDomainUserAssemblies();
						//Load intercept handlers before load plugin components
						LoadInterceptHandlers();

						_configureSectionInterceptor = new Interceptor(typeof(EnvironmentManager));

                        //Set application assembly resolver
                        SetAssemblyResolve();


                        //Load configure setting
                        LoadConfig(_defaultConfigureFile);
						//Load binary setting
						LoadBinary();

						//Load type from binary assembly
						LoadComponents();
						//Load modules
						LoadModules();

						//Load intercept handlers after plugin components loaded
						LoadInterceptHandlers();

						isInit = true;
						Log.DevDebug("EnvironmentManager.Init end");
					}
				}
			}
		}

		static SafeDictionary<Type, object> _interceptHandlers = new SafeDictionary<Type, object>();
		static void LoadInterceptHandlers()
		{
			var types = SafeGetInterfacedTypes(typeof(IInterceptorHandler));
			foreach (var type in types)
			{
				Interceptor.RegisterHandlerType(type);
			}
		}


		static void MapDomainUserAssemblies()
		{
			foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (IsSystemAssembly.Any(x=>x(a))){
					continue;
				}
				var assemblyParts = a.FullName.Split(',');
				//Should never happen
				if (assemblyParts.Length == 0) continue;
				var assemblyName = assemblyParts[0];
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

		private static IList<Type> SafeGetInterfacedTypes(Assembly assembly, Type interfacedType)
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

		public static readonly List<Func<Assembly, bool>> IsSystemAssembly = new List<Func<Assembly, bool>>
		{
			a => (
				String.IsNullOrEmpty(a.Location) || //ignore byte array loaded assembly
				a.ManifestModule.Name == "<In Memory Module>" || //ignore in memory module
				a.Location.IndexOf("App_Web", StringComparison.Ordinal) > -1 || //ignore web dynamic compile dlls
				a.Location.IndexOf("App_global", StringComparison.Ordinal) > -1 || //ignore web app resource dlls
				a.FullName.IndexOf("CppCodeProvider", StringComparison.Ordinal) > -1 || //ignore code dom provider
				a.FullName.IndexOf("SMDiagnostics", StringComparison.Ordinal) > -1 || //WCF
				a.FullName.StartsWith("WebMatrix.", StringComparison.Ordinal) || // ignore MS web secuirty dll
				a.FullName.StartsWith("Microsoft.", StringComparison.Ordinal) || //Microsoft dlls
				a.FullName.StartsWith("WindowsBase.", StringComparison.Ordinal) || //WPF
				a.FullName.StartsWith("System.", StringComparison.Ordinal) || //Microsoft System dlls
				a.FullName.StartsWith("System,", StringComparison.Ordinal) || //Microsoft System dll
				a.FullName.StartsWith("mscorlib,", StringComparison.Ordinal) || //Core
				a.FullName.StartsWith("Oracle.", StringComparison.Ordinal)), //Oracle
			a => a.FullName.StartsWith("nunit,", StringComparison.Ordinal),
			a => a.FullName.StartsWith("nunit.", StringComparison.Ordinal),
			a => a.FullName.StartsWith("Ninject,", StringComparison.Ordinal),
			a => a.FullName.StartsWith("Ninject.", StringComparison.Ordinal),
			a => a.FullName.StartsWith("Castle.", StringComparison.Ordinal),
			a => a.FullName.StartsWith("TypeMock", StringComparison.Ordinal),
			a => a.FullName.StartsWith("Typemock", StringComparison.Ordinal),
			a => a.FullName.StartsWith("Telerik.", StringComparison.Ordinal),
			a => a.FullName.StartsWith("Oracle.", StringComparison.Ordinal)
		};


		[SecuritySafeCritical]
		static void SetAssemblyResolve()
		{
			AppDomain.CurrentDomain.AssemblyResolve += ApplicationAssemblyResolve;
		}

		/// <summary>
		/// Resolve assembly location when lookup type by a qualified type name.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		static Assembly ApplicationAssemblyResolve(object sender, ResolveEventArgs args)
		{
			// Ignore missing resources
			if (args.Name.Contains(".resources")) return null;

			var assemblyParts = args.Name.Split(',');
			//Should never happen
			if (assemblyParts.Length == 0) return null;

			var assemblyName = assemblyParts[0];

			if (_assemblyMaps.ContainsKey(assemblyName) && _assemblyMaps[assemblyName].Assembly != null)
			{
				// check for assemblies already loaded by the framework
				return _assemblyMaps[assemblyName].Assembly;
			}


			// check for assemblies already loaded by the current application domain. It is necessary. See. 
			// https://docs.microsoft.com/en-us/dotnet/framework/deployment/best-practices-for-assembly-loading
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var assembly = assemblies.FirstOrDefault(a => a.GetName().FullName.Equals(args.Name) ||
													a.GetName().Name.Equals(args.Name) ||
													a.GetName().Name.Equals(assemblyName));
			//Add loaded assembly in the list
			if (assembly != null)
			{
				_assemblyMaps[assemblyName] = new PlugableAssembly { Assembly = assembly };
				Log.SysLogger.Debug("Loaded from AppDomain assembly {0}", assembly.FullName);
			}
			else
			{

				//Try to load assembly from different configured folders
				if (!_assemblyMaps.ContainsKey(assemblyName))
                {
                    LoadBinary();
                }
                if (_assemblyMaps.ContainsKey(assemblyName))
                {
                    //Assembly already loaded 
                    if (_assemblyMaps[assemblyName].Assembly != null)
					{
						return _assemblyMaps[assemblyName].Assembly;
					}

					assembly = Assembly.LoadFrom(_assemblyMaps[assemblyName].Path);
					_assemblyMaps[assemblyName].Assembly = assembly;
				}
				else {
                    //Couldn't find assembly
                    _sysLogger.Warn("Couldn't find assembly {0}", assemblyName);
				}
			}

			return assembly;
		}


		static void LoadComponents()
		{
			Log.DevDebug("EnvironmentManager.LoadComponents begin");

			foreach (var c in _environmentConfigure.Components)
			{
				LoadComponent(c.Value);
			}

			Log.DevDebug("EnvironmentManager.LoadComponents end");

		}

		static void LoadModules()
		{
			Log.DevDebug("EnvironmentManager.LoadModules begin");

			foreach (var c in _environmentConfigure.Modules)
			{
				LoadModule(c);
			}

			Log.DevDebug("EnvironmentManager.LoadModules end");
		}

		static void LoadModule(KeyValuePair<string,NamedTypeElement> module)
		{
			var instance = CreateInstance(SafeLoadType(module.Value.Type)) as IModuleLoader;
			if (instance != null)
			{
				if (!_modules.ContainsKey(module.Key))
				{
					_modules.Add(module.Key, instance);
					instance.Initialize();
				}
			}
		}

		/// <summary>
		/// Load a qualified type without throw exception
		/// </summary>
		/// <returns>The type or null.</returns>
		/// <param name="typeValue">Type value.</param>
		public static Type SafeLoadType(string typeValue)
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
					_sysLogger.Error("Invalid type [{0}].", typeValue);
				}
			}
			catch (Exception ex)
			{
				_sysLogger.Error("SafeLoadType {0} failed: {1}", typeValue, ex.Message);
			}
			return type;
		}

		static void LoadComponent(PlugableComponent component)
		{
			Log.DevDebug("EnvironmentManager.LoadComponent begin {0}",component.FormatObjectValues());

			component.InterfaceType = SafeLoadType(component.InterfaceTypeName);
			component.ClassType = SafeLoadType(component.ClassTypeName);

			Log.DevDebug("EnvironmentManager.LoadComponent end");
		}

		static void LoadBinary()
		{
			Log.DevDebug("EnvironmentManager.LoadBinary begin");

			foreach (var binPath in _environmentConfigure.AssemblyFolders)
			{
				if (Directory.Exists(binPath))
				{
					foreach (var dll in new DirectoryInfo(binPath).GetFiles("*.dll"))
					{
						var assemblyName = Path.GetFileNameWithoutExtension(dll.FullName);
						if (!_assemblyMaps.ContainsKey(assemblyName))
						{
							_assemblyMaps.Add(assemblyName, new PlugableAssembly
							{
								Path = dll.FullName
							});
							Log.DevDebug("EnvironmentManager.LoadBinary {0}", dll.FullName);
						}
					}
				}
			}

			Log.DevDebug("EnvironmentManager.LoadBinary end");

		}

		/// <summary>
		/// Load system configuration from configure files.
		/// 
		/// </summary>
		/// <param name="configFile">Config file. The default is application domain configuration file: app.config or web.config</param>
		static EnvironmentConfigure LoadConfig(string configFile = null)
		{
			return ConfigureSectionInterceptor.JoinPoint(() =>
			{

				System.Configuration.Configuration config;
				try
				{
					if (string.IsNullOrEmpty(configFile))
					{
						// Get the current configuration file.
						config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
					}
					else
					{
						var fileName = Path.GetFileName(configFile).ToLower();
						if (_configFiles.ContainsKey(fileName))
						{
							//Do not load the file if it already loaded
							return _environmentConfigure;
						}
						_configFiles.Add(fileName, configFile);

						var fileMap = new System.Configuration.ExeConfigurationFileMap { ExeConfigFilename = configFile };
						config = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(fileMap, System.Configuration.ConfigurationUserLevel.None);
					}
				}
				catch (Exception ex)
				{
					Log.SysLogger.Error("Load config {0} throw an exception {1}", configFile ?? "default", ex.Message);
					return _environmentConfigure;
				}

				if (config != null)
				{
					foreach (var section in config.Sections)
					{
						//intercept section element
						ConfigureSectionInterceptor.ForEach(section);

						LoadEnvironmentSection(section as EnvironmentSection);
						LoadDbConnectionStrings(section as ConnectionStringsSection);
					}
				}

				return _environmentConfigure;
			}, "LoadConfig", configFile);
		}

		static void LoadDbConnectionStrings(ConnectionStringsSection section)
		{
			if (section != null)
			{
				if (section.SectionInformation.IsProtected)
				{
					section.SectionInformation.UnprotectSection();
				}
				foreach (ConnectionStringSettings c in section.ConnectionStrings)
				{
					_environmentConfigure.AddConnectionString(c);
				}
			}
		}

		static void LoadEnvironmentSection(EnvironmentSection section)
		{
			if (section != null)
			{
				#region Load Components
				foreach (var component in section.Components)
				{
					_environmentConfigure.AddComponent(component);
				}
				#endregion

				#region Load Modules
				foreach (var module in section.Modules)
				{
					_environmentConfigure.AddModule(module);
				}
				#endregion

				#region Load Applistion Settings
				foreach (var setting in section.AppSettings)
				{
					if (!_environmentConfigure.AppSettings.ContainsKey(setting.Key))
					{
						_environmentConfigure.AppSettings.Add(setting.Key, setting.Value);
					}
				}
				#endregion

				#region Load other level configures
				foreach (var environment in section.Environments)
				{
					_environmentConfigure.AddEnvironment(environment);
					//Only load environment related setting
					if (string.IsNullOrEmpty(environment.Host) ||
						environment.Host == Environment.MachineName ||
					   environment.Host == EnvironmentEx.MachineIp)
					{

						var folder = Path.GetDirectoryName(section.CurrentConfiguration.FilePath);
						var path = UnifiedPath(folder,environment.Path);
						if (Directory.Exists(path) && !_environmentConfigure.ConfigureFolders.Contains(path))
						{
							_environmentConfigure.ConfigureFolders.Add(path);

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
							var files = Directory.GetFiles(path, "*.config");
							if (files != null)
							{
								for (int i = 0; i<files.Length; i++)
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

		static void SetBinaryFolders(string binFolder)
		{
			Log.DevDebug("EnvironmentManager.SetBinaryFolders begin {0}", binFolder);


			if (Directory.Exists(binFolder) && !_environmentConfigure.AssemblyFolders.Contains(binFolder))
			{
				_environmentConfigure.AssemblyFolders.Add(binFolder);
				for (int i = FrameworkVersionPaths().Length - 1; i >= 0;i--)
				{
					var versionPath = Path.Combine(binFolder, FrameworkVersionPaths()[i]);
					if (Directory.Exists(versionPath) && !_environmentConfigure.AssemblyFolders.Contains(versionPath))
					{
						_environmentConfigure.AssemblyFolders.Add(versionPath);

						Log.DevDebug("EnvironmentManager.SetBinaryFolders version path {0} found",versionPath);
						break;
					}
				}
				var cpuArchitecturePath = Path.Combine(binFolder, EnvironmentEx.CpuArchitecture);
				if (Directory.Exists(cpuArchitecturePath) && !_environmentConfigure.AssemblyFolders.Contains(cpuArchitecturePath))
				{
					_environmentConfigure.AssemblyFolders.Add(cpuArchitecturePath);

					Log.DevDebug("EnvironmentManager.SetBinaryFolders CPU Architecture path {0} found",cpuArchitecturePath);
				}
			}

			Log.DevDebug("EnvironmentManager.SetBinaryFolders end");

		}

		static string[] _versionPaths;
		/// <summary>
		/// Get framework version paths.
		/// bin/4
		/// bin/45
		/// bin/471
		/// </summary>
		/// <returns>The version paths.</returns>
		static string[] FrameworkVersionPaths()
		{
			if (_versionPaths == null)
			{
				var callingAssembly = typeof(EnvironmentManager).Assembly;
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


		static string UnifiedPath(string folder, string path)
		{
			if (Path.IsPathRooted(path)) return path;

			return Path.Combine(folder, path);
		}

		#endregion
	
	}

}