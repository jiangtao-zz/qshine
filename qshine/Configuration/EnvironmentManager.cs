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
		/// <summary>
		/// Gets the provider.
		/// </summary>
		/// <returns>The provider.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetProvider<T>()
		where T:class,IProvider
		{
			return GetProvider(typeof(T)) as T;
		}

		/// <summary>
		/// Gets the provider.
		/// </summary>
		/// <returns>The provider.</returns>
		/// <param name="providerInterface">Provider interface.</param>
		public static IProvider GetProvider(Type providerInterface)
		{
			PlugableComponent providerComponent=null;
			foreach (var component in _environmentConfigure.Components)
			{
				if (component.Value.InterfaceType.Name == providerInterface.Name)
				{
					providerComponent = component.Value;
					break;
				}
			}

			if(providerComponent!=null)
			{
				var type = providerComponent.ClassType;
				if (type == null) return null;

				var parameters = providerComponent.Parameters.Values.ToArray();
				var instance = Activator.CreateInstance(type,parameters);
				return instance as IProvider;
			}
			return null;
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
		public static IDictionary<string, PlugableAssembly> AssemblyMaps
		{
			get
			{
				return _assemblyMaps;
			}
		}


		#region privates methods/properties

		static ILogger _sysLogger;
		static EnvironmentConfigure _environmentConfigure;
		static IDictionary<string, PlugableAssembly> _assemblyMaps;
		static IDictionary<string, string> _configFiles;

		/// <summary>
		/// Initializes the <see cref="T:qshine.Configuration.EnvironmentManager"/> class when access any EnvironmentManager proeprty or method.
		/// </summary>
		static EnvironmentManager()
		{
			Log.DevDebug("EnvironmentManager.ctor begin");

			//Initial variables
			_environmentConfigure = new EnvironmentConfigure();
			_assemblyMaps = new Dictionary<string, PlugableAssembly>();
			_configFiles = new Dictionary<string, string>();
			_sysLogger = Log.SysLogger;


			//Load configure setting
			LoadConfig();
			//Load binary setting
			LoadBinary();
			//Set application assembly resolver
			SetAssemblyResolve();
			//Load type from binary assembly
			LoadComponents();

			Log.DevDebug("EnvironmentManager.ctor end");
		}

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
			var assembly = assemblies.FirstOrDefault(assem => assem.GetName().FullName.Equals(args.Name) ||
													assem.GetName().Name.Equals(args.Name) ||
													assem.GetName().Name.Equals(assemblyName));
			//Add loaded assembly in the list
			if (assembly != null)
			{
				_assemblyMaps[assemblyName] = new PlugableAssembly { Assembly = assembly };
				Log.SysLogger.Debug("Loaded from AppDomain assembly {0}", assembly.FullName);
			}
			else
			{

				//Try to load assembly from different configured folders
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
				SafeLoadType(c.Value);
			}

			Log.DevDebug("EnvironmentManager.LoadComponents end");

		}

		static void SafeLoadType(PlugableComponent component)
		{
			Log.DevDebug("EnvironmentManager.SafeLoadType begin {0}",component.FormatObjectValues());

			string typeName=string.Empty;
			try
			{
				typeName = component.InterfaceTypeName;
				component.InterfaceType = Type.GetType(component.InterfaceTypeName);
				typeName = component.ClassTypeName;
				component.ClassType = Type.GetType(component.ClassTypeName);

				if (component.ClassType == null)
				{
					//This should not happen. Just in case type cannot be resolved by previous assembly_resolver, need load type from assembly directly
					var assemblyNameParts = component.ClassTypeName.Split(',');
					if (assemblyNameParts.Length > 1)
					{
						var assemblyName = assemblyNameParts[1].Trim();
						var typeNameOnly = assemblyNameParts[0].Trim();
						if (_assemblyMaps.ContainsKey(assemblyName))
						{
							var assembly = _assemblyMaps[assemblyName].Assembly;
							if (assembly != null)
							{
								component.ClassType = assembly.GetType(typeNameOnly, true);
							}
						}
					}
				}
				if (component.InterfaceType == null)
				{
					_sysLogger.Error("Invalid interface type [{0}].", component.InterfaceTypeName);
				}
				if (component.ClassType == null)
				{
					_sysLogger.Error("Invalid class type [{0}].", component.ClassTypeName);
				}
			}
			catch (Exception ex)
			{
				_sysLogger.Error("Load component type [{0}] error. {1}", typeName, ex.Message);
			}

			Log.DevDebug("EnvironmentManager.SafeLoadType end");
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
			Log.DevDebug("EnvironmentManager.LoadConfig begin");
			
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

			Log.DevDebug("EnvironmentManager.LoadConfig end");

			return _environmentConfigure;
		}

		static void SetBinaryFolders(string path)
		{
			Log.DevDebug("EnvironmentManager.SetBinaryFolders begin {0}", path);


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

						Log.DevDebug("EnvironmentManager.SetBinaryFolders version path {0} found",versionPath);
						break;
					}
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

		static string UnifiedPath(string path)
		{
			return Path.GetFullPath(path);
		}

		#endregion
	
	}

}