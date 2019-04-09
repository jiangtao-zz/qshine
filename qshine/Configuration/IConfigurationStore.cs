using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Configuration
{
    /// <summary>
    /// Application environment configuration store interface,
    /// </summary>
    public interface IConfigurationStore
    {
        /// <summary>
        /// Load EnvironmentConfigure data from specific formatted config file store. 
        /// </summary>
        /// <param name="option">EnvironmentInitializationOption object to indicate how to load the the config files.</param>
        /// <returns>Environment configure instance</returns>
        EnvironmentConfigure LoadConfig(EnvironmentInitializationOption option);
    }
}
