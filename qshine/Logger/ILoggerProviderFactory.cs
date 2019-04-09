using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Logger
{
    /// <summary>
    /// LogProviderFactory interface.
    /// </summary>
    public interface ILoggerProviderFactory
    {
        /// <summary>
        /// Create a new provider instance by a given category name
        /// </summary>
        /// <param name="category">logger category name</param>
        /// <returns>return log provider instance</returns>
        ILoggerProvider CreateProvider(string category);

        /// <summary>
        /// Register a logger framework provider and associate it to a logging category in the Logging system. 
        /// </summary>
        /// <param name="provider">logger provider instance</param>
        /// <param name="category">logger category name</param>
        void RegisterProvider(ILoggerProvider provider, string category);
    }
}
