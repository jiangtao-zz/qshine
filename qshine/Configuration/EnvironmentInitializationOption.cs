using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using qshine.Logger;

namespace qshine
{
    /// <summary>
    /// Options for ApplicationEnvironment initialization
    /// </summary>
    public class EnvironmentInitializationOption
    {
        /// <summary>
        /// Option to overwrite named connection string from later loaded configure file.
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

        /// <summary>
        /// Option to overwrite later loaded named maps
        /// </summary>
        public bool OverwriteMap { get; set; }

        /// <summary>
        /// Option to throw exception for any error. 
        /// If the option is not set, only fatal error will be thrown. 
        /// The non-fatal error will be hold in property InnerException.
        /// </summary>
        public bool ThrowException { get; set; }

        /// <summary>
        /// Specifies a new application root configure file.
        /// A blank value indicates the default application config file,
        /// </summary>
        public string RootConfigFile { get; set; }

        /// <summary>
        /// Specifies a new Logger.
        /// The default logger instance for load application configure
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Specifies a custom function used to find the candidate assemblies.
        /// The unwanted pluggable assemblies should be filtered out. 
        /// </summary>
        static public Func<Assembly, bool> IsCandidateAssembly { get; set; }
        /// <summary>
        /// Specifies a system run-time assemblies get from current application.
        /// As default, the RuntimeComponents will be get from AppDomain. For many reasons, those components may not be able to load from AppDomain.
        /// In this case, the application need manually load them into this property before build the ApplicationEnvironment.
        /// </summary>
        static public Assembly[] RuntimeComponents { get; set; }

        /// <summary>
        /// Hold non-fatal exceptions occurred during application environment initialization.
        /// The application can ignore those invalid plug-in components. 
        /// Try to set ThrowException option to true if the application want to throw exception for any error.
        /// </summary>
        public Configuration.ConfigurationException InnerException { get; set; }

    }
}
