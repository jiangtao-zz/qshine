using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace qshine
{
    /// <summary>
    /// Options for ApplicationEnvironment initialization
    /// </summary>
    public class EnvironmentInitializationOption
    {
        /// <summary>
        /// Option to overwrite named connection string from later loaded configure file
        /// </summary>
        public bool OverwriteConnectionString { get; set; }

        /// <summary>
        /// Option to overwrite key/value pair from later loaded configure file.
        /// </summary>
        public bool OverwriteAppSetting { get; set; }

        /// <summary>
        /// Option to overwrite later loaded named component
        /// </summary>
        public bool OverwriteComponent { get; set; }

        /// <summary>
        /// Option to overwrite later loaded named module
        /// </summary>
        public bool OverwriteModule { get; set; }

        string _configFilePattern = "*.config";
        /// <summary>
        /// Specifies configure file pattern.
        /// The default config file pattern is "*.config".
        /// </summary>
        public string ConfigureFilePattern {
            get { return _configFilePattern; }
            set { _configFilePattern = value; }
        }

        /// <summary>
        /// Specifies a new application root configure file.
        /// The default is .NET application config file,
        /// </summary>
        public string RootConfigFile { get; set; }

        /// <summary>
        /// Specifies application root Configuration object. 
        /// As default behavior the ApplicationEnvironment load Configuration from execution folder.
        /// But in some cases you need load configuration file based on different context.
        /// Ex: for VS built-in web server the configration must be load as:
        /// 
        ///     RootConfiguration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
        /// 
        /// The RootConfigFile and RootConfiguration can only be set one. The RootConfiguration property will take precedence.
        /// </summary>
        public System.Configuration.Configuration RootConfiguration { get; set; }

        /// <summary>
        /// Specifies a new Logger.
        /// The default logger is Log.SysLogger
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Specifies a custom function to find candidate assembly.
        /// It will be used to filter the assembly which does not want to loaded into mapping list. 
        /// </summary>
        public Func<Assembly, bool> IsCandidateAssembly { get; set; }
        /// <summary>
        /// Specifies a system run-time assemblies get from current application.
        /// As default, the RuntimeComponents will be get from AppDomain. For many reasons, it may not be available in AppDomain.
        /// In this case, it need be loaded by application manually before build ApplicationEnvironment.
        /// </summary>
        public Assembly[] RuntimeComponents { get; set; }


    }
}
