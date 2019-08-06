using qshine.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using qshine.Configuration.ConfigurationStore;

namespace qshine.Configuration
{
    /// <summary>
    /// Application Environment public static service
    /// </summary>
    public partial class ApplicationEnvironment
    {

        #region public static methods and properties

        //#region Build
        ///// <summary>
        ///// Build and Initialize current application environment from a specific configure file.
        ///// </summary>
        ///// <param name="rootConfigFile">Specifies a root configure file which contains the application environment setting. 
        ///// If the root config file is not set, a default application configure file such as app.config, web.config or others loaded by host .net application as config file.</param>
        ///// <returns>The application environment instance</returns>
        ///// <remarks>
        ///// The ApplicationEnvironment.Build() need be put in begin of the applciation execution path.
        ///// A default ApplicationEnvironment instance will be created if doesn't call ApplicationEnvironment.Build() explicitly.
        ///// </remarks>
        //public static ApplicationEnvironment Build(string rootConfigFile = "")
        //{
        //    return Build("",

        //        new EnvironmentInitializationOption()
        //        {
        //            RootConfigFile = rootConfigFile
        //        }
        //        );
        //}

        ///// <summary>
        ///// Build and Initialize current application environment instance with specific options.
        ///// </summary>
        ///// <param name="options">Specifies application environment initialization options <see cref="EnvironmentInitializationOption"/>.</param>
        ///// <returns>The application environment instance</returns>
        //public static ApplicationEnvironment Build(EnvironmentInitializationOption options)
        //{
        //    return Build("", options);
        //}

        ///// <summary>
        ///// Build and Initialize a named application environment with different options.
        ///// </summary>
        ///// <param name="name">name of application environment</param>
        ///// <param name="options">Specifies application environment initialization options <see cref="EnvironmentInitializationOption"/>.</param>
        ///// <returns>The application environment instance</returns>
        //public static ApplicationEnvironment Build(string name, EnvironmentInitializationOption options)
        //{
        //    var builder = new ApplicationEnvironmentBuilder(name);
        //    builder.Configure((context, configure) =>
        //    {
        //        configure.LoadConfigFile(options.RootConfigFile, options);
        //    });

        //    return builder.Build();
        //}

        //#endregion

        ///// <summary>
        ///// Get the default application environment. The Default application environment is a global property.
        ///// Except Default application environment, the appliction also can load different named application environment
        ///// </summary>
        //public static ApplicationEnvironment Default
        //{
        //    get
        //    {
        //        return Build();
        //    }
        //}

        //#region GetProvider
        ///// <summary>
        ///// Gets a given type provider instance from current application environment.
        ///// The provider must implemented IProvider interface.
        ///// </summary>
        ///// <returns>The provider instance.</returns>
        ///// <typeparam name="T">The type of the provider.</typeparam>
        ///// <remarks>The provider is a plugable component configured in application environment.
        ///// It will load first defined component if many components found.
        ///// </remarks>
        //public static T GetProvider<T>()
        //where T : class, IProvider
        //{
        //    return GetProvider(typeof(T)) as T;
        //}

        ///// <summary>
        ///// Gets the provider component by name from current application environment.
        ///// The provider must implemented IProvider interface.
        ///// </summary>
        ///// <returns>The a named provider instance.</returns>
        ///// <param name="name">Name of the provider.</param>
        ///// <typeparam name="T">The type of the provider.</typeparam>
        ///// <remarks>The provider is a plugable component configured in application environment with a given name.
        ///// If the named provider is not configured, it returns null.
        ///// </remarks>
        //public static T GetProvider<T>(string name)
        //where T : class, IProvider
        //{
        //    return GetProvider(typeof(T), name) as T;
        //}

        ///// <summary>
        ///// Gets a given type provider instance from current application environment.
        ///// The provider must implemented IProvider interface.
        ///// </summary>
        ///// <returns>The provider instance.</returns>
        ///// <param name="providerInterface">type of provider.</param>
        //public static IProvider GetProvider(Type providerInterface)
        //{
        //    return GetProvider(providerInterface, string.Empty);
        //}

        ///// <summary>
        ///// Gets a named given type provider instance from current application environment.
        ///// The provider must implemented IProvider interface.
        ///// </summary>
        ///// <returns>The provider instance.</returns>
        ///// <param name="providerInterface">type of provider.</param>
        /////<param name="name">provider name</param>
        //public static IProvider GetProvider(Type providerInterface, string name)
        //{
        //    return Default.CreateProvider(providerInterface, name);
        //}

        //#endregion

        //static readonly SafeDictionary<string, Type> _commonNamedType = new SafeDictionary<string, Type>();
        ///// <summary>
        ///// Gets the type by the type name. The type name could be a qualified type name accessible by the application environment.
        ///// The application environment could contain plugable assembly.
        ///// </summary>
        ///// <returns>The named type.</returns>
        ///// <param name="typeName">Type name.</param>
        //public static Type GetTypeByName(string typeName)
        //{
        //    var type = Type.GetType(typeName);
        //    if (type == null)
        //    {
        //        if (_commonNamedType.ContainsKey(typeName))
        //        {
        //            return _commonNamedType[typeName];
        //        }

        //        type = PluggableAssembly.GetType(typeName);
        //        if (type != null)
        //        {
        //            //thread-safe
        //            if (!_commonNamedType.ContainsKey(typeName))
        //            {
        //                _commonNamedType.Add(typeName, type);
        //            }
        //        }
        //    }
        //    return type;
        //}

        //#endregion

        //#region private static
        //static SafeDictionary<Type, object> _interceptHandlers = new SafeDictionary<Type, object>();
        //static readonly object lockObject = new object();



        ///// <summary>
        ///// Creates the instance by type and given arguments.
        ///// </summary>
        ///// <returns>The instance.</returns>
        ///// <param name="type">Type of object</param>
        ///// <param name="parameters">Parameters.</param>
        //static object CreateInstance(Type type, params object[] parameters)
        //{
        //    if (type == null) return null;

        //    return type.TryCreateInstance(parameters);
        //}

        //private static ApplicationEnvironment _default;
        ///// <summary>
        ///// Get default application environment
        ///// </summary>
        //public static ApplicationEnvironment Default
        //{
        //    get
        //    {

        //        return ApplicationEnvironmentContext.Default.ApplicationEnvironment;
        //    }
        //}

        #endregion
    }

}
