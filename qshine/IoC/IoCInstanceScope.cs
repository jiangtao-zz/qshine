using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qshine
{
    /// <summary>
    /// The options to control instance lifetime scope.
    /// </summary>
    public enum IocInstanceScope
    {
        /// <summary>
        /// Signle instance per container.
        /// </summary>
        Singleton,  
        /// <summary>
        /// New instance for each request.
        /// </summary>
        Transient,
        /// <summary>
        /// New instance per call context scoped. It could be a web request, logic call context or any scope of the context implemented IContextStore.
        /// Use context bind/unbind to scope and release instance per-context request.
        /// </summary>
        Scoped,
    }
}
