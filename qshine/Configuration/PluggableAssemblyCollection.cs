using qshine.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace qshine.Configuration
{
    /// <summary>
    /// Pluggable assembly collection
    /// </summary>
    public class PluggableAssemblyCollection
    {

        /// <summary>
        /// pluggable assemblies
        /// </summary>
        private SafeDictionary<string, PluggableAssembly> _pluggableAssemblies = new SafeDictionary<string, PluggableAssembly>();

        /// <summary>
        /// Add a pluggable assembly into the collection
        /// </summary>
        /// <param name="name">simple assembly name</param>
        /// <param name="assembly">Pluggable assembly compoenent (may not loaded yet)</param>
        /// <returns>returns true if assembly added into the collection</returns>
        public bool Add(string name, PluggableAssembly assembly)
        {
            if (!_pluggableAssemblies.ContainsKey(name))
            {
                _pluggableAssemblies.Add(name, assembly);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the pluggable assembly in the collection.
        /// </summary>
        /// <param name="name">The pluggable assembly name.</param>
        /// <returns>return true if the assembly exists in the collection.</returns>
        public bool Contains(string name)
        {
            return _pluggableAssemblies.ContainsKey(name);
        }

        /// <summary>
        /// Get pluggable assembly by name
        /// </summary>
        /// <param name="name">The pluggable assembly name.</param>
        /// <returns>Returns a pluggable assembly by name. Or, returns null if the assembly not found.</returns>
        public PluggableAssembly this[string name]
        {
            get
            {
                if (_pluggableAssemblies.ContainsKey(name))
                {
                    return _pluggableAssemblies[name];
                }
                return null;
            }
        }

        /// <summary>
        /// Get assembly from assembly collection
        /// </summary>
        /// <param name="assemblyName">simple assembly name</param>
        /// <returns>Loaded Assembly</returns>
        public Assembly GetAssembly(string assemblyName)
        {
            if (_pluggableAssemblies.ContainsKey(assemblyName) && _pluggableAssemblies[assemblyName].Assembly != null)
            {
                return _pluggableAssemblies[assemblyName].Assembly;
            }
            return null;
        }

        /// <summary>
        /// Find all types which implemented a specific interface or base class type from all loaded assemblies. 
        /// </summary>
        /// <param name="interfaceType">Specifies interface or base class type.</param>
        /// <returns>A list of implementation class types.</returns>
        public IList<Type> SafeGetInterfacedTypes(Type interfaceType)
        {
            var selectedTypes = new List<Type>();
            foreach (var a in _pluggableAssemblies.Values)
            {
                if (a.Assembly != null)
                {
                    var types = a.Assembly.SafeGetInterfacedTypes(interfaceType);
                    if (types != null && types.Count > 0)
                    {
                        selectedTypes.AddRange(types);
                    }
                }
            }
            return selectedTypes;
        }

        readonly SafeDictionary<string, Type> _commonNamedType = new SafeDictionary<string, Type>();
        /// <summary>
        /// Gets the type by the type name. The type name could be a qualified type name accessible by the application environment.
        /// The application environment could contain plugable assembly.
        /// </summary>
        /// <returns>The named type.</returns>
        /// <param name="typeName">Type name.</param>
        public Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
            {
                if (_commonNamedType.ContainsKey(typeName))
                {
                    return _commonNamedType[typeName];
                }

                type = GetTypeByName(typeName);
                if (type != null)
                {
                    //thread-safe
                    if (!_commonNamedType.ContainsKey(typeName))
                    {
                        _commonNamedType.Add(typeName, type);
                    }
                }
            }
            return type;
        }

        /// <summary>
        /// Get type by given assembly name and object type name from a pluggable assembly Map.
        /// To avoid pluggable component dll hell, it only allow one version be loaded for pluggable assembly.
        /// </summary>
        /// <param name="assemblyName">Simple assembly name. </param>
        /// <param name="typeName">Simple type name.</param>
        /// <param name="throwError">throw exception if it is true.</param>
        /// <returns></returns>
        public Type GetType(string assemblyName, string typeName, bool throwError)
        {
            Type result = null;
            if (_pluggableAssemblies.ContainsKey(assemblyName))
            {
                var assembly = _pluggableAssemblies[assemblyName].Assembly;
                if (assembly != null)
                {
                    result = assembly.GetType(typeName, throwError);
                }
            }
            return result;
        }

        /// <summary>
        /// Get type by type name. It finds first found type from assembly Map
        /// </summary>
        /// <param name="typeName">Simple type name.</param>
        /// <returns></returns>
        public Type GetTypeByName(string typeName)
        {
            if (typeName.Contains(","))
            {
                //Get type by a qualified type name.
                return Type.GetType(typeName);
            }

            foreach (var a in _pluggableAssemblies.Values)
            {
                if (a.Assembly != null)
                {
                     var type =  a.Assembly.GetType(typeName);
                    if (type != null) return type;
                }
            }
            return null;
        }

    }
}
