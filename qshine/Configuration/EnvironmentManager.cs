using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.Linq;
using qshine.Configuration;

namespace qshine.Configuration
{
	public class EnvironmentManager
	{
		public static T GetProvider<T>()
		where T:class,IProvider
		{
			return null;
		}

		public static IProvider GetProvider(Type providerInterface)
		{
/*			if (_availableProvider.ContainsKey(providerInterface))
			{
				return _availableProvider[providerInterface];
			}
*/			return null;
		}

/*				static void LoadProviders()
				{

					foreach (var component in _environmentConfigure.Components)
					{
						//var provider = component.Value. as IProvider;
						//if(
					}

				}
				static bool _hasInitialized;
				static void Initialize()
				{
					if (_hasInitialized) return;

					lock (_initLock)
					{
						if (_hasInitialized) return;
						//Load configure setting
						LoadConfig();
						//Load provider
						LoadProviders();
						_hasInitialized = true;
					}
				}
		*/

		static EnvironmentManager _environmentManager;
		static object _initLock = new object();

		/// <summary>
		/// Gets or sets the current Environment Manager.
		/// </summary>
		/// <value>The current.</value>
		public static EnvironmentManager Current
		{
			get
			{
				if (_environmentManager == null)
				{
					lock (_initLock)
					{
						if (_environmentManager == null)
						{
							_environmentManager = new EnvironmentManager();
						}
					}
				}
				return _environmentManager;
			}
			set
			{
				_environmentManager = value;
			}
		}

		[SecuritySafeCritical]
		static void Initialize_Application()
		{
			AppDomain.CurrentDomain.AssemblyResolve += LookupAssembly;
		}


		static object lockObject = new object();

		/// <summary>
		/// Resolve assembly location when lookup type by a qualified type name.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		static Assembly LookupAssembly(object sender, ResolveEventArgs args)
		{
			// Ignore missing resources
			if (args.Name.Contains(".resources")) return null;

			var assemblyParts = args.Name.Split(',');
			//Should never happen
			if (assemblyParts.Length == 0) return null;

			var assemblyName = assemblyParts[0];

			if (EnvironmentManager.Current.AssemblyMaps.ContainsKey(assemblyName) && EnvironmentManager.Current.AssemblyMaps[assemblyName].Assembly != null)
			{
				// check for assemblies already loaded by the framework
				return EnvironmentManager.Current.AssemblyMaps[assemblyName].Assembly;
			}


			// check for assemblies already loaded by the current application domain. It is necessary. See. 
			// https://docs.microsoft.com/en-us/dotnet/framework/deployment/best-practices-for-assembly-loading
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var assembly = assemblies.FirstOrDefault(assem => assem.GetName().FullName.Equals(args.Name) ||
													assem.GetName().Name.Equals(args.Name) ||
													assem.GetName().Name.Equals(assemblyName));
			//Add loaded assembly in the list
			if (assembly != null)
			{
				lock (lockObject)
				{
					EnvironmentManager.Current.AssemblyMaps[assemblyName] = new PlugableAssembly { Assembly = assembly };
					//Logger.SystemDebug(string.Format("Loaded from AppDomain assembly {0}", assemby.FullName));
				}
			}
			else
			{

				//Try to load assembly from different configured folders
				if (EnvironmentManager.Current.AssemblyMaps.ContainsKey(assemblyName))
				{
					//Assembly already loaded 
					if (EnvironmentManager.Current.AssemblyMaps[assemblyName].Assembly != null)
					{
						return EnvironmentManager.Current.AssemblyMaps[assemblyName].Assembly;
					}

					lock (lockObject)
					{

						assembly = Assembly.LoadFrom(EnvironmentManager.Current.AssemblyMaps[assemblyName].Path);

						EnvironmentManager.Current.AssemblyMaps[assemblyName].Assembly = assembly;
					}
				}
			}
			//			else {
			//Couldn't find assembly
			//Logger.SystemDebug(string.Format("==> no assembly found in list matching {0}", justAssemblyName));
			//			}

			return assembly;
		}



		EnvironmentConfigure _environmentConfigure = new EnvironmentConfigure();
		IDictionary<string, PlugableAssembly> _assemblyMaps = new Dictionary<string, PlugableAssembly>();

		ILogger _logger;

		public EnvironmentManager()
		{
			_logger = Logger.GetLogger<EnvironmentManager>();
			//Load configure setting
			LoadConfig();
			LoadBinary();
			LoadComponents();
		}

		public EnvironmentConfigure EnvironmentConfigure {
			get{
				return _environmentConfigure;
			}
		}

		public IDictionary<string, PlugableAssembly> AssemblyMaps
		{
			get
			{
				return _assemblyMaps;
			}
		}


		private void LoadComponents()
		{
			foreach (var c in _environmentConfigure.Components)
			{
				SafeLoadType(c.Value);
			}
		}

		private void SafeLoadType(PlugableComponent component)
		{
			string typeName=string.Empty;
			try
			{
				typeName = component.InterfaceTypeName;
				component.InterfaceType = Type.GetType(component.InterfaceTypeName);
				typeName = component.ClassTypeName;
				component.ClassType = Type.GetType(component.ClassTypeName);
			}
			catch (Exception ex)
			{
				_logger.Error("Load component type [{0}] error. {1}", typeName, ex.Message);
			}
		}

		private void LoadBinary()
		{
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
						}
					}
				}
			}
		}

		/// <summary>
		/// Load system configuration from configure files.
		/// 
		/// </summary>
		/// <param name="configFile">Config file. The default is application domain configuration file: app.config or web.config</param>
		private EnvironmentConfigure LoadConfig(string configFile = null)
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
					var fileMap = new System.Configuration.ExeConfigurationFileMap { ExeConfigFilename = configFile };
					config = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(fileMap, System.Configuration.ConfigurationUserLevel.None);
				}
			}
			catch (Exception ex)
			{
				_logger.Error("Load config {0} throw an exception {1}", configFile ?? "default", ex.Message);
				return _environmentConfigure;
			}

			if (config != null)
			{
				foreach (var section in config.Sections)
				{
					var mySection = section as EnvironmentSection;

					if (mySection != null)
					{
						#region Load Components
						foreach (var component in mySection.Components)
						{
							_environmentConfigure.AddComponent(component);
						}
						#endregion

						#region Load Modules
						foreach (var module in mySection.Modules)
						{
							_environmentConfigure.AddModule(module);
						}
						#endregion

						#region Load Applistion Settings
						foreach (var setting in mySection.AppSettings)
						{
							if (!_environmentConfigure.AppSettings.ContainsKey(setting.Key))
							{
								_environmentConfigure.AppSettings.Add(setting.Key, setting.Value);
							}
						}
						#endregion

						#region Load other level configures
						foreach (var environment in mySection.Environments)
						{
							_environmentConfigure.AddEnvironment(environment);
							//Only load environment related setting
							if (string.IsNullOrEmpty(environment.Host) ||
							    environment.Host == Environment.MachineName ||
							   environment.Host == EnvironmentEx.MachineIp)
							{
								var path = UnifiedPath(environment.Path);
								if (Directory.Exists(path) && !_environmentConfigure.ConfigureFolders.Contains(path))
								{
									_environmentConfigure.ConfigureFolders.Add(path);
									SetBinaryFolders(path);
									//Find all configure files and load all
									var files = Directory.GetFiles(path, "*.config");
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
			}
			return _environmentConfigure;
		}

		string[] _versionPaths;
		void SetBinaryFolders(string path)
		{
			var binFolder = Path.Combine(Path.GetFullPath(path),"bin");
			                             
			if (Directory.Exists(binFolder) && !_environmentConfigure.AssemblyFolders.Contains(binFolder))
			{
				_environmentConfigure.AssemblyFolders.Add(binFolder);
				for (int i = FrameworkVersionPaths().Length - 1; i >= 0;i--)
				{
					var versionPath = Path.Combine(binFolder, FrameworkVersionPaths()[i]);
					if (Directory.Exists(versionPath) && !_environmentConfigure.AssemblyFolders.Contains(versionPath))
					{
						_environmentConfigure.AssemblyFolders.Add(versionPath);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Get framework version paths.
		/// bin/4
		/// bin/45
		/// bin/471
		/// </summary>
		/// <returns>The version paths.</returns>
		public string[] FrameworkVersionPaths()
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

		string UnifiedPath(string path)
		{
			return Path.GetFullPath(path);
		}
	}

}