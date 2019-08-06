using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace qshine.Utility
{
    /// <summary>
    /// Assembly extension
    /// </summary>
    public static class AssemblyExtension
    {
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

        /// <summary>
        /// Find all class types which inplemented a given interface type
        /// </summary>
        /// <param name="assembly">Assembly may contain interfaced class</param>
        /// <param name="interfacedType">Specifies an interface to be lookup.</param>
        /// <returns>A list of class types which implemented a given interface or base class.</returns>
        public static IList<Type> SafeGetInterfacedTypes(this Assembly assembly, Type interfacedType)
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (FileNotFoundException)
            {
                types = new Type[] { };
            }
            catch (NotSupportedException)
            {
                types = new Type[] { };
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null).ToArray();
            }
            return types.Where(t => interfacedType.IsAssignableFrom(t) && t.IsClass).ToList();
        }

    }
}
