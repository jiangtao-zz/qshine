using System;
using System.Linq;
using qshine.Utility;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace qshine
{
    /// <summary>
    /// Pluggable assembly component
    /// </summary>
	public class PluggableAssembly
	{
		/// <summary>
		/// Gets or sets the path of assembly.
		/// </summary>
		/// <value>The path.</value>
		public string Path { get; set; }

		/// <summary>
		/// Gets or sets the assembly.
		/// </summary>
		/// <value>The assembly.</value>
		public Assembly Assembly { get; set; }

        /// <summary>
        /// Indicates the assembly has been initialized
        /// </summary>
        public ulong Initialized { get; set; }

        #region static 

        ///// <summary>
        ///// AssemblyMaps
        ///// </summary>
        //static SafeDictionary<string, PluggableAssembly> _assemblyMaps = new SafeDictionary<string, PluggableAssembly>();

        ///// <summary>
        ///// Pluggable assembly maps
        ///// </summary>
        //public static SafeDictionary<string, PluggableAssembly> Maps
        //{
        //    get
        //    {
        //        return _assemblyMaps;
        //    }
        //}

        ///// <summary>
        ///// Get type by given assembly name and object type name from a pluggable assembly Map.
        ///// To avoid pluggable component dll hell, it only allow one version be loaded for pluggable assembly.
        ///// </summary>
        ///// <param name="assemblyName">Simple assembly name. </param>
        ///// <param name="typeName">Simple type name.</param>
        ///// <param name="throwError">throw exception if it is true.</param>
        ///// <returns></returns>
        //public static Type GetType(string assemblyName, string typeName, bool throwError)
        //{
        //    Type result = null;
        //    if (_assemblyMaps.ContainsKey(assemblyName))
        //    {
        //        var assembly = _assemblyMaps[assemblyName].Assembly;
        //        if (assembly != null)
        //        {
        //            result = assembly.GetType(typeName, throwError);
        //        }
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// Get type by type name. It finds first found type from assembly Map
        ///// </summary>
        ///// <param name="typeName">Simple type name.</param>
        ///// <returns></returns>
        //public static Type GetType(string typeName)
        //{
        //    return _assemblyMaps.Values.Where(x => x.Assembly != null)
        //    .Select(a => a.Assembly.GetType(typeName))
        //    .FirstOrDefault(t => t != null);
        //}

        ///// <summary>
        ///// Get assembly from assembly Map
        ///// </summary>
        ///// <param name="assemblyName">simple assembly name</param>
        ///// <returns>Loaded Assembly</returns>
        //public static Assembly GetAssembly(string assemblyName)
        //{
        //    if (_assemblyMaps.ContainsKey(assemblyName) && _assemblyMaps[assemblyName].Assembly != null)
        //    {
        //        return _assemblyMaps[assemblyName].Assembly;
        //    }
        //    return null;
        //}

        ///// <summary>
        ///// Get pluggable assembly component by name
        ///// </summary>
        ///// <param name="assemblyName">simple assembly name</param>
        ///// <returns>Pluggable assembly compoenent (may not loaded yet)</returns>
        //public static PluggableAssembly GetPluggableAssembly(string assemblyName)
        //{
        //    if (_assemblyMaps.ContainsKey(assemblyName))
        //    {
        //        return _assemblyMaps[assemblyName];
        //    }
        //    return null;
        //}

        ///// <summary>
        ///// Add a pluggable assembly into Map
        ///// </summary>
        ///// <param name="assemblyName">simple assembly name</param>
        ///// <param name="assembly">Pluggable assembly compoenent (may not loaded yet)</param>
        ///// <returns>returns true if assembly added in the Map</returns>
        //public static bool AddAssembly(string assemblyName, PluggableAssembly assembly)
        //{
        //    if (!_assemblyMaps.ContainsKey(assemblyName))
        //    {
        //        _assemblyMaps.Add(assemblyName, assembly);
        //        return true;
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// Find all types which implemented a specific interface or base class type from all loaded assemblies. 
        ///// </summary>
        ///// <param name="interfaceType">Specifies interface or base class type.</param>
        ///// <returns>A list of implementation class types.</returns>
        //public static IList<Type> SafeGetInterfacedTypes(Type interfaceType)
        //{
        //    var selectedTypes = new List<Type>();
        //    foreach (var a in _assemblyMaps.Values)
        //    {
        //        if (a.Assembly != null)
        //        {
        //            var types = SafeGetInterfacedTypes(a.Assembly, interfaceType);
        //            if (types != null && types.Count > 0)
        //            {
        //                selectedTypes.AddRange(types);
        //            }
        //        }
        //    }
        //    return selectedTypes;
        //}

        ///// <summary>
        ///// Load a qualified type without throw exception
        ///// </summary>
        ///// <returns>The type or null.</returns>
        ///// <param name="typeValue">Type value.</param>
        //public static Type SafeLoadType(string typeValue)
        //{
        //    Type type = null;
        //    try
        //    {
        //        //try to get type from loaded assembly
        //        //if it has not loaded yet, ask resolver to load the assembly
        //        type = Type.GetType(typeValue);

        //        if (type == null)
        //        {
        //            //This should not happen. Just in case type cannot be resolved by previous assembly_resolver, need load type from assembly directly
        //            var assemblyNameParts = typeValue.Split(',');
        //            if (assemblyNameParts.Length > 1)
        //            {
        //                var assemblyName = assemblyNameParts[1].Trim();
        //                var typeNameOnly = assemblyNameParts[0].Trim();

        //                type = GetType(assemblyName, typeNameOnly, true);
        //            }
        //        }
        //    }
        //    catch //(Exception ex)
        //    {
        //        //Logger("AE.SafeLoadType:: {0} throw exception: {1}", typeValue, ex.Message);
        //    }
        //    return type;
        //}

        ///// <summary>
        ///// Find all class types which inplemented a given interface type
        ///// </summary>
        ///// <param name="assembly">Assembly may contain interfaced class</param>
        ///// <param name="interfacedType">Specifies an interface to be lookup.</param>
        ///// <returns>A list of class types which implemented a given interface or base class.</returns>
        //static IList<Type> SafeGetInterfacedTypes(Assembly assembly, Type interfacedType)
        //{
        //    Type[] types;

        //    try
        //    {
        //        types = assembly.GetTypes();
        //    }
        //    catch (FileNotFoundException)
        //    {
        //        types = new Type[] { };
        //    }
        //    catch (NotSupportedException)
        //    {
        //        types = new Type[] { };
        //    }
        //    catch (ReflectionTypeLoadException e)
        //    {
        //        types = e.Types.Where(t => t != null).ToArray();
        //    }
        //    return types.Where(t => interfacedType.IsAssignableFrom(t) && t.IsClass).ToList();
        //}
        #endregion
    }
}
