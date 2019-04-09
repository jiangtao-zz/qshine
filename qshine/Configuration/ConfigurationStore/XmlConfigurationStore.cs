using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace qshine.Configuration.ConfigurationStore
{
    /// <summary>
    /// Implement .NET framework formatted application environment configure files 
    /// </summary>
    public class XmlConfigurationStore : IConfigurationStore
    {
        EnvironmentConfigure _environmentConfigure = new EnvironmentConfigure();
        bool _isRootConfig = true;
        EnvironmentInitializationOption _options;

        /// <summary>
        /// Load specific config file with options
        /// </summary>
        /// <param name="option">EnvironmentInitializationOption object to indicate how to load the the config files.</param>
        /// <returns></returns>
        public EnvironmentConfigure LoadConfig(EnvironmentInitializationOption option)
        {
            _options = option;
            return LoadConfig(_options.RootConfigFile);
        }

        /// <summary>
        /// Load specific config file. If the config file is not specified, use default config file
        /// </summary>
        /// <param name="configFile">configure file path.</param>
        /// <returns>Environment configure instance</returns>
        EnvironmentConfigure LoadConfig(string configFile)
        {
            System.Configuration.Configuration config;
            try
            {
                if (string.IsNullOrEmpty(configFile))
                {
                    config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                    if (config != null)
                    {
                        var filePath = config.FilePath.ToLower();
                        _environmentConfigure.ConfigFiles.Add(filePath);
                    }

                }
                else
                {
                    var fileName = configFile.ToLower();
                    if (_environmentConfigure.ConfigFiles.Contains(fileName))
                    {
                        //Do not load the file if it already loaded
                        return _environmentConfigure;
                    }
                    _environmentConfigure.ConfigFiles.Add(fileName);

                    var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = configFile };
                    config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                }
            }
            catch (Exception ex)
            {
                if (_isRootConfig)
                {
                    //always throw exception in root configure file
                    throw;
                }
                LogWarning("AE:: Failed to load config file {0}. ({1})"._G(configFile ?? "default", ex.Message));
                return _environmentConfigure;
            }

            if (config != null)
            {

#if NETCORE
                var diagnosticsSection =
                    (from ConfigurationSection x in config.Sections where x.SectionInformation != null && x.SectionInformation.Name == "system.diagnostics" select x)
                    .FirstOrDefault();

                if (diagnosticsSection != null)
                {
                    var parser = new XmlDiagnosticsSection(diagnosticsSection.SectionInformation.GetRawXml());
                    parser.Parse();
                }

                var dataSection =
                    (from ConfigurationSection x in config.Sections where x.SectionInformation != null && x.SectionInformation.Name == "system.data" select x)
                    .FirstOrDefault();

                if (dataSection != null)
                {
                    var parser = new XmlDbProviderFactoriesSection(dataSection.SectionInformation.GetRawXml());
                    parser.Parse();
                }
#endif
                //Load ConnectionString sections first
                foreach (ConfigurationSection section in config.Sections)
                {
                    if (section is ConnectionStringsSection)
                    {
                        //Load connection strings
                        LoadDbConnectionStrings(section as ConnectionStringsSection);
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

            return _environmentConfigure;
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
                    LogInfo("AE.LoadDbConnectionStrings:: {0}, {1}. Overwrite={2}", c.Name, c.ConnectionString, _options.OverwriteConnectionString);

                    _environmentConfigure.AddConnectionString(new ConnectionStringElement(c.Name, c.ConnectionString, c.ProviderName),
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
            //_interceptor.RaiseOnEnterEvent(this,
            //    new InterceptorEventArgs("LoadEnvironmentSection", section
            // ));

            if (section != null)
            {
                #region Load Components
                foreach (var component in section.Components)
                {
                    LogInfo("AE.LoadEnvironmentSection:: Add '{0}' <components> section , Overwrite={1}"._G(component.Name, _options.OverwriteComponent));
                    _environmentConfigure.AddComponent(component, _options.OverwriteComponent);
                }
                #endregion

                #region Load Modules
                foreach (var module in section.Modules)
                {
                    LogInfo("AE.LoadEnvironmentSection:: Add '{0}' <modules> section, Overwrite={1}"._G(module.Name, _options.OverwriteModule));
                    _environmentConfigure.AddModule(new PlugableComponent
                    {
                        Name = module.Name,
                        ClassTypeName = module.Type,
                        ConfigureFilePath = section.CurrentConfiguration.FilePath
                    }, _options.OverwriteModule);
                }
                #endregion

                #region Load Application Settings
                foreach (var setting in section.AppSettings)
                {
                    LogInfo("AE.LoadEnvironmentSection:: Add AppSettings {0} = {1}, Overwrite={2}"._G(setting.Key, setting.Value, _options.OverwriteAppSetting));

                    //Add or Update
                    if (!_environmentConfigure.AppSettings.ContainsKey(setting.Key))
                    {
                        _environmentConfigure.AppSettings.Add(setting.Key, setting.Value);
                    }
                    if (_options.OverwriteAppSetting)
                    {
                        _environmentConfigure.AppSettings[setting.Key] = setting.Value;
                    }
                }
                #endregion

                #region Load Maps
                foreach (var maps in section.Maps)
                {
                    _environmentConfigure.AddMap(section.Maps.Name, section.Maps.Default, maps, _options.OverwriteMap);
                }
                #endregion

                #region Load other level configures
                foreach (var environment in section.Environments)
                {
                    LogInfo("AE.LoadEnvironmentSection:: Add '{0}' <environments> section."._G(environment.Name));
                    _environmentConfigure.AddEnvironment(environment);
                    //Only load environment related setting
                    if (MatchHost(environment.Host))
                    {

                        var folder = Path.GetDirectoryName(section.CurrentConfiguration.FilePath);
                        var path = UnifiedFullPath(folder, environment.Path);
                        if (!Directory.Exists(path))
                        {
                            LogWarning("AE:: Config path {0} does not exist."._G(path));
                        }
                        else if (!_environmentConfigure.ConfigureFolders.Contains(path))
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
                                        ResolveBinaryFolders(binFolder);
                                    }
                                }
                            }

                            string configFilePattern = "*.config";

                            //Find all configure files and load all
                            var files = Directory.GetFiles(path, configFilePattern);
                            if (files != null)
                            {
                                _isRootConfig = false;
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

        int _deeps = 0;
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
        /// 5. [given binary folder]/[os folder] ex: bin/win or bin/linux.
        /// 6. [given binary folder]/[one of above folder]/[one of above folder]/...
        /// </summary>
        /// <param name="binFolder">binary folder entry</param>
        /// <remark>
        /// The binary folder may contain any level of below type sub-folder. Only matched folder dlls will be loaded
        ///  binary Folder --
        ///        |-- qshine version folder: 1.0, 2.1
        ///        |-- cpu-architecture folder: x86, x64, arm, arm64
        ///        |-- target framework moniker folder: net461, netcoreapp2.1
        ///        |-- os folder: win, linux, osx
        ///        |
        /// </remark>
		void ResolveBinaryFolders(string binFolder)
        {
            if (_deeps > 50)
            {
                //The environment configure loop infinity.
                throw new ArgumentOutOfRangeException("AE.ResolveBinaryFolders:: Too many sub-folder levels."._G());
            }
            _deeps++;
            if (Directory.Exists(binFolder) && !_environmentConfigure.AssemblyFolders.Any(x => x.ObjectData == binFolder))
            {
                LogInfo("AE.ResolveBinaryFolders:: Add binary folder {0}"._G(binFolder));

                //add specified binary folder
                _environmentConfigure.AssemblyFolders.Add(new StateObject<bool, string>(false, binFolder));

                //add components based on specific qshine version.
                for (int i = GetLibraryVersionPaths().Length - 1; i >= 0; i--)
                {
                    var versionPath = Path.Combine(binFolder, GetLibraryVersionPaths()[i]);
                    if (Directory.Exists(versionPath) && !_environmentConfigure.AssemblyFolders.Any(x => x.ObjectData == versionPath))
                    {
                        //EnvironmentConfigure.AssemblyFolders.Add(new StateObject<bool, string>(false, versionPath));
                        LogInfo("AE.ResolveBinaryFolders:: Found version path {0}"._G(versionPath));
                        //Search for sub folder
                        ResolveBinaryFolders(versionPath);
                        break;
                    }
                }

                //add components built for specific cpu architecture
                var cpuArchitecturePath = Path.Combine(binFolder, EnvironmentEx.CpuArchitecture);
                if (Directory.Exists(cpuArchitecturePath) && !_environmentConfigure.AssemblyFolders.Any(x => x.ObjectData == cpuArchitecturePath))
                {
                    //EnvironmentConfigure.AssemblyFolders.Add(new StateObject<bool, string>(false, cpuArchitecturePath));
                    LogInfo("AE.ResolveBinaryFolders:: Found CPU Architecture path {0}"._G(cpuArchitecturePath));
                    //Search for sub folder
                    ResolveBinaryFolders(cpuArchitecturePath);
                }

                //add components built for specific .net library version
                if (!string.IsNullOrEmpty(EnvironmentEx.TargetFramework))
                {
                    //add cpu architecture binary folder
                    var targetDotNetFrameworkPath = Path.Combine(binFolder, EnvironmentEx.TargetFramework);
                    if (Directory.Exists(targetDotNetFrameworkPath) && !_environmentConfigure.AssemblyFolders.Any(x => x.ObjectData == targetDotNetFrameworkPath))
                    {
                        //EnvironmentConfigure.AssemblyFolders.Add(new StateObject<bool, string>(false, targetDotNetFrameworkPath));
                        LogInfo("AE.ResolveBinaryFolders:: Found DotNet target framework path {0}"._G(targetDotNetFrameworkPath));
                        //Search for sub folder
                        ResolveBinaryFolders(targetDotNetFrameworkPath);
                    }
                }

                //add components built for specific operation system
                if (!string.IsNullOrEmpty(EnvironmentEx.OSPlatform))
                {
                    //add cpu architecture binary folder
                    var osPath = Path.Combine(binFolder, EnvironmentEx.OSPlatform);
                    if (Directory.Exists(osPath) && !_environmentConfigure.AssemblyFolders.Any(x => x.ObjectData == osPath))
                    {
                        //EnvironmentConfigure.AssemblyFolders.Add(new StateObject<bool, string>(false, targetDotNetFrameworkPath));
                        LogInfo("AE.ResolveBinaryFolders:: Found OS path {0}"._G(osPath));
                        //Search for sub folder
                        ResolveBinaryFolders(osPath);
                    }
                }

            }
            _deeps--;

        }

        string[] hostTags = { "name", "ip", "cpu", "os", "framework", "version" };

        bool MatchHost(string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName)) return true;

            var tags = hostName.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var tag in tags)
            {
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    var tagKeyValue = tag.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (tagKeyValue.Length == 1)
                    {
                        //argument
                        //Non of host argument match to the tag
                        if (!(
                            string.Compare(tagKeyValue[0], Environment.MachineName, true) == 0 ||
                            string.Compare(tagKeyValue[0], EnvironmentEx.MachineIp, true) == 0 ||
                            string.Compare(tagKeyValue[0], EnvironmentEx.CpuArchitecture, true) == 0 ||
                            string.Compare(tagKeyValue[0], EnvironmentEx.OSPlatform, true) == 0 ||
                            string.Compare(tagKeyValue[0], EnvironmentEx.TargetFramework, true) == 0 ||
                            LibraryVersion.IndexOf(tagKeyValue[0]) == 0))
                        {
                            return false;
                        }
                    }
                    else if (tagKeyValue.Length == 2)
                    {
                        //tag=argument
                        //Non of argument match to the tag and value
                        if (!(
                            (string.Compare(tagKeyValue[0], hostTags[0], true) == 0 && (string.Compare(tagKeyValue[1], Environment.MachineName, true) == 0)) ||
                            (string.Compare(tagKeyValue[0], hostTags[1], true) == 0 && (string.Compare(tagKeyValue[1], EnvironmentEx.MachineIp, true) == 0)) ||
                            (string.Compare(tagKeyValue[0], hostTags[2], true) == 0 && (string.Compare(tagKeyValue[1], EnvironmentEx.CpuArchitecture, true) == 0)) ||
                            (string.Compare(tagKeyValue[0], hostTags[3], true) == 0 && (string.Compare(tagKeyValue[1], EnvironmentEx.OSPlatform, true) == 0)) ||
                            (string.Compare(tagKeyValue[0], hostTags[4], true) == 0 && (string.Compare(tagKeyValue[1], EnvironmentEx.TargetFramework, true) == 0)) ||
                            (string.Compare(tagKeyValue[0], hostTags[5], true) == 0 && (LibraryVersion.IndexOf(tagKeyValue[1]) == 0))
                            ))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //unknown
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Return absolute path for a specified path. 
        /// If the specified path is a relative path, the absolute path is the combination of specified folder and relative path.
        /// </summary>
        /// <param name="folder">Specifies a folder for full path</param>
        /// <param name="path">Full path or relative path</param>
        /// <returns></returns>
		string UnifiedFullPath(string folder, string path)
        {
            if (Path.IsPathRooted(path)) return path;

            return Path.Combine(folder, path);
        }

        string[] _versionPaths;
        string _libraryVersion;
        /// <summary>
        /// qshine library version
        /// </summary>
        string LibraryVersion
        {
            get
            {
                if (_libraryVersion == null)
                {
                    var callingAssembly = typeof(ApplicationEnvironment).Assembly;
                    var info = FileVersionInfo.GetVersionInfo(callingAssembly.Location);
                    _libraryVersion = info.FileVersion;
                }
                return _libraryVersion;
            }
        }
        /// <summary>
        /// Get qshine library version paths.
        /// bin/1
        /// bin/1.2
        /// bin/1.2.3
        /// </summary>
        /// <returns>The version paths.</returns>
        string[] GetLibraryVersionPaths()
        {
            if (_versionPaths == null)
            {
                _versionPaths = LibraryVersion.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);


                string binPath = "";
                string version = "";
                for (var index = 0; index < _versionPaths.Length; index++)
                {
                    version += index == 0 ? _versionPaths[index] : "." + _versionPaths[index];
                    _versionPaths[index] = Path.Combine(binPath, version);
                }
            }
            return _versionPaths;
        }

        void LogInfo(string format, params object[] args)
        {

        }

        void LogWarning(string format, params object[] args)
        {

        }

    }
}
