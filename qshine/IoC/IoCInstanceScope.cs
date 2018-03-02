using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qshine.IoC
{
    /// <summary>
    /// The options to control instance life time scope.
	/// It do not have per-request scope. Use context bind/unbind to release instance per-request.
    /// </summary>
    public enum IoCInstanceScope
    {
        /// <summary>
        /// Signle instance per container.
        /// </summary>
        Singleton,  
        /// <summary>
        /// New instance for each resolve.
        /// </summary>
        Transient,
    }
}
