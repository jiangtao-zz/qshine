using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qshine.IoC
{
    /// <summary>
    /// The options to control life time scope of IoC container instance.
    /// If the instance has registered through RegisterInstance(), the same instance will be returned 
    /// regardless how LefttimeScope setting.
    /// </summary>
    public enum IoCLifetimeScope
    {
        /// <summary>
        /// Signle instance per container.
        /// </summary>
        Singleton,  
        /// <summary>
        /// New instance created when calling Resolve() method.
        /// </summary>
        Transient,
        /// <summary>
        /// Single instance per Http request. It's only available in web environment. Otherwise, the Singleton option
        /// will be used instead
        /// </summary>
        HttpRequest
        //Thread      //single instance per http request
    }
}
