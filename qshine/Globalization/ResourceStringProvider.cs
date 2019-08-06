using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Text;

namespace qshine.Globalization
{
    /// <summary>
    /// Implement ILocalStringProvider interface using Resource Manager.
    /// 
    /// Get localized strings from the resource file through Resource manager.
    /// 
    /// </summary>
    public class ResourceStringProvider : ILocalStringProvider
    {
        Assembly _resourceAssembly;

        /// <summary>
        /// load local resource from current assembly
        /// </summary>
        public ResourceStringProvider()
        {
            _resourceAssembly = Assembly.GetCallingAssembly();
        }
        /// <summary>
        /// load resource from given assembly file path.
        /// 
        /// Ex:
        ///     var provider = new ResourceStringProvider("");
        /// </summary>
        /// <param name="resourceAssemblyFile">assembly resource file</param>
        public ResourceStringProvider(string resourceAssemblyFile)
        {
            _resourceAssembly = Assembly.LoadFrom(resourceAssemblyFile);
        }


        /// <summary>
        /// load resource from a given assembly
        /// </summary>
        /// <param name="resourceAssembly">The resource assembly</param>
        public ResourceStringProvider(Assembly resourceAssembly)
        {
            _resourceAssembly = resourceAssembly;
        }

        /// <summary>
        /// Get resource store by the resource name.
        /// The local string will search from all related resource files.
        /// It could be one of:
        ///     name.resources.fr-CA.resx
        ///     name.resources.fr.resx
        ///     name.resources.resx
        ///     name.fr-CA.resx
        ///     name.fr.resx
        ///     name.resx
        ///     
        /// </summary>
        /// <param name="name">The name is the base name of the resource (Neutral Resources).</param>
        /// <returns></returns>
        public ILocalString Create(string name)
        {
            return new ResourceStringStore(_resourceAssembly, name);
        }
    }

    /// <summary>
    /// Resource string store
    /// </summary>
    public class ResourceStringStore : ILocalString
    {
        ResourceManager _resourceManager;
        /// <summary>
        /// Ctor.
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.resources.resourcemanager.-ctor?view=netframework-4.8"/>
        /// </summary>
        /// <param name="assembly">resource assembly</param>
        /// <param name="name">The base name of the resource such as "Resources".</param>
        public ResourceStringStore(Assembly assembly, string name)
        {
            _resourceManager = new ResourceManager(name, assembly);
        }

        /// <summary>
        /// Get localized string
        /// </summary>
        /// <param name="format">format string in Neutral resource language</param>
        /// <param name="arguments">format arguments</param>
        /// <returns></returns>
        public string GetString(string format, params object[] arguments)
        {
            var nativeFormat = _resourceManager.GetString(format);

            if (arguments.Length == 0) return nativeFormat;

            return string.Format(nativeFormat, arguments);
        }
    }
}
