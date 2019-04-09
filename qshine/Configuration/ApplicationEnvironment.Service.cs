using qshine.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace qshine.Configuration
{
    /// <summary>
    /// Application Environment public static service
    /// </summary>
    public partial class ApplicationEnvironment
    {
        static Interceptor _interceptor;

        /// <summary>
        /// Static ctor:: pre-initialization when first touch the ApplicationEnvironment component
        /// </summary>
        static ApplicationEnvironment()
        {
            //Register ApplicationEnvironment interceptor
            _interceptor = Interceptor.Get<ApplicationEnvironment>();

            //Load runtime assemblies
            LoadRuntimeComponents();

        }


        #region public static methods and properties
        /// <summary>
        /// Get/Set current environment context store.
        /// The default current environment context store is Static Context store. It allows single current application environment.
        /// You may want to have a different context store to have current application environment based on different context. 
        /// In this case you can implement a special EnvironmentContextStore.
        /// The EnvironmentContextStore need be set in application Startup before ApplicationEnvironment.Boot()
        /// </summary>
        public static IContextStore EnvironmentContext
        {
            get
            {
                if (_contextStore == null)
                {
                    lock (lockObject)
                    {
                        if (_contextStore == null)
                        {
                            _contextStore = ContextManager.StaticContext;
                        }
                    }
                }
                return _contextStore;
            }
            set
            {
                lock (lockObject)
                {
                    _contextStore = value;
                }
            }
        }

        /// <summary>
        /// Get current application environment. The current application environment is a global property
        /// </summary>
        public static ApplicationEnvironment Current
        {
            get
            {
                if (!(EnvironmentContext.GetData(environmentContextStoreName) is ApplicationEnvironment current))
                {
                    //Create default environment manager
                    lock (lockObject)
                    {
                        current = EnvironmentContext.GetData(environmentContextStoreName) as ApplicationEnvironment;
                        if (current == null)
                        {
                            current = new ApplicationEnvironment();
                        }
                    }
                }
                return current;
            }
        }

        #region Build
        /// <summary>
        /// Build and Initialize current application environment from a specific configure file.
        /// </summary>
        /// <param name="rootConfigFile">Specifies a root configure file which contains the application environment setting. 
        /// If the root config file is not set, a default application configure file such as app.config, web.config or others loaded by host .net application as config file.</param>
        /// <returns>The application environment instance</returns>
        /// <remarks>
        /// The ApplicationEnvironment.Build() need be put in begin of the applciation execution path.
        /// A default ApplicationEnvironment instance will be created if doesn't call ApplicationEnvironment.Build() explicitly.
        /// </remarks>
        public static ApplicationEnvironment Build(string rootConfigFile = "")
        {
            return Build(
                new EnvironmentInitializationOption()
                {
                    RootConfigFile = rootConfigFile
                },
                ""
                );
        }

        /// <summary>
        /// Build and Initialize current application environment instance with specific options.
        /// </summary>
        /// <param name="options">Specifies application environment initialization options <see cref="EnvironmentInitializationOption"/>.</param>
        /// <returns>The application environment instance</returns>
        public static ApplicationEnvironment Build(EnvironmentInitializationOption options)
        {
            return Build(options,"");
        }

        /// <summary>
        /// Build and Initialize a named application environment with different options.
        /// </summary>
        /// <param name="name">name of application environment</param>
        /// <param name="options">Specifies application environment initialization options <see cref="EnvironmentInitializationOption"/>.</param>
        /// <returns>The application environment instance</returns>
        public static ApplicationEnvironment Build(EnvironmentInitializationOption options, string name)
        {
            var contextName = GetEnvironmentContextName(name);
            if (!(EnvironmentContext.GetData(contextName) is ApplicationEnvironment appEnvironment))
            {
                lock (lockObject)
                {
                    appEnvironment = EnvironmentContext.GetData(contextName) as ApplicationEnvironment;
                    if (appEnvironment == null)
                    {
                        appEnvironment = new ApplicationEnvironment(name, options);
                    }
                }
            }
            return appEnvironment;
        }

        #endregion

        #region GetProvider
        /// <summary>
        /// Gets a given type provider instance from current application environment.
        /// The provider must implemented IProvider interface.
        /// </summary>
        /// <returns>The provider instance.</returns>
        /// <typeparam name="T">The type of the provider.</typeparam>
        /// <remarks>The provider is a plugable component configured in application environment.
        /// It will load first defined component if many components found.
        /// </remarks>
        public static T GetProvider<T>()
        where T : class, IProvider
        {
            return GetProvider(typeof(T)) as T;
        }

        /// <summary>
        /// Gets the provider component by name from current application environment.
        /// The provider must implemented IProvider interface.
        /// </summary>
        /// <returns>The a named provider instance.</returns>
        /// <param name="name">Name of the provider.</param>
        /// <typeparam name="T">The type of the provider.</typeparam>
        /// <remarks>The provider is a plugable component configured in application environment with a given name.
        /// If the named provider is not configured, it returns null.
        /// </remarks>
        public static T GetProvider<T>(string name)
        where T : class, IProvider
        {
            return GetProvider(typeof(T), name) as T;
        }

        /// <summary>
        /// Gets a given type provider instance from current application environment.
        /// The provider must implemented IProvider interface.
        /// </summary>
        /// <returns>The provider instance.</returns>
        /// <param name="providerInterface">type of provider.</param>
        public static IProvider GetProvider(Type providerInterface)
        {
            return GetProvider(providerInterface, string.Empty);
        }

        /// <summary>
        /// Gets a named given type provider instance from current application environment.
        /// The provider must implemented IProvider interface.
        /// </summary>
        /// <returns>The provider instance.</returns>
        /// <param name="providerInterface">type of provider.</param>
        ///<param name="name">provider name</param>
        public static IProvider GetProvider(Type providerInterface, string name)
        {
            return Current.CreateProvider(providerInterface, name);
        }

        #endregion

        static readonly SafeDictionary<string, Type> _commonNamedType = new SafeDictionary<string, Type>();
        /// <summary>
        /// Gets the type by the type name. The type name could be a qualified type name accessible by the application environment.
        /// The application environment could contain plugable assembly.
        /// </summary>
        /// <returns>The named type.</returns>
        /// <param name="typeName">Type name.</param>
        public static Type GetTypeByName(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
            {
                if (_commonNamedType.ContainsKey(typeName))
                {
                    return _commonNamedType[typeName];
                }

                type = PluggableAssembly.GetType(typeName);
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

        #endregion

        #region private static
        static SafeDictionary<Type, object> _interceptHandlers = new SafeDictionary<Type, object>();
        static IContextStore _contextStore;
        static readonly object lockObject = new object();

        static string GetEnvironmentContextName(string name)
        {
            var contextName = environmentContextStoreName;
            if (!string.IsNullOrEmpty(name))
            {
                contextName += "_" + name;
            }
            return contextName;
        }

        /// <summary>
        /// Filter assemblies before add it into candidate assembly list
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        static bool IsCandidateAssembly(Assembly assembly)
        {
            //Ignore dynamic assembly
            if (assembly.IsDynamic) return true;

            string location = assembly.Location;
            var fullName = assembly.FullName;
            bool isCandidateAssembly = String.IsNullOrEmpty(location) || //ignore byte array loaded assembly
                assembly.ManifestModule.Name == "<In Memory Module>" || //ignore in memory module
                location.IndexOf("App_Web", StringComparison.Ordinal) > -1 || //ignore web dynamic compile dlls
                location.IndexOf("App_global", StringComparison.Ordinal) > -1 || //ignore web app resource dlls
                location.IndexOf("Microsoft", StringComparison.OrdinalIgnoreCase) > -1 || //ignore microsoft resource dlls
                fullName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) || //ignore Microsoft dlls
                fullName.StartsWith("System.", StringComparison.OrdinalIgnoreCase) || //ignore Microsoft System dlls
                fullName.StartsWith("mscorlib,", StringComparison.Ordinal) || //ignore Run-time Core
                fullName.StartsWith("runtime.", StringComparison.OrdinalIgnoreCase) || //ignore Run-time Core
                fullName.IndexOf("CppCodeProvider", StringComparison.Ordinal) > -1 || //ignore code dom provider
                fullName.IndexOf("SMDiagnostics", StringComparison.Ordinal) > -1 || //WCF
                fullName.StartsWith("WebMatrix.", StringComparison.Ordinal) || // ignore MS web secuirty dll
                fullName.StartsWith("WindowsBase.", StringComparison.Ordinal) || //WPF
                fullName.StartsWith("NETStandard.Library", StringComparison.OrdinalIgnoreCase) || //ignore .NET Core
                fullName.StartsWith("WindowsAzure.Storage", StringComparison.OrdinalIgnoreCase) || //ignore Azure storage
                fullName.StartsWith("nunit,", StringComparison.Ordinal) ||
                fullName.StartsWith("Ninject,", StringComparison.Ordinal) ||
                fullName.StartsWith("Castle.", StringComparison.Ordinal) ||
                fullName.StartsWith("Typemock", StringComparison.OrdinalIgnoreCase) ||
                fullName.StartsWith("Telerik.", StringComparison.Ordinal);

            if (!isCandidateAssembly && EnvironmentInitializationOption.IsCandidateAssembly != null)
            {
                isCandidateAssembly = !EnvironmentInitializationOption.IsCandidateAssembly(assembly);
            }
            return isCandidateAssembly;
        }

        /// <summary>
        /// Load application components from runtime location to application environment.
        /// Those components types could be resolved directly from run-time.
        /// The mapped runtime application components will be part of accessable types for plugable application environment.
        /// It will not include most system or common share components.
        /// </summary>
        static void LoadRuntimeComponents()
        {
            Assembly[] runtimeAssemblies = EnvironmentInitializationOption.RuntimeComponents;

            if (runtimeAssemblies == null)
            {
                runtimeAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            foreach (var a in runtimeAssemblies)
            {
                //Skip system assemblies
                if (IsCandidateAssembly(a))
                {
                    continue;
                }

                //Get assembly name without version and culture info.
                var assemblyNameObject = new AssemblyName(a.FullName);
                var assemblyName = assemblyNameObject.Name;

                var assembly = PluggableAssembly.GetPluggableAssembly(assemblyName);

                if (assembly == null)
                {
                    PluggableAssembly.AddAssembly(assemblyName, new PluggableAssembly
                    {
                        Path = a.FullName,
                        Assembly = a
                    });
                }
            }
        }

        static void LoadInterceptHandlers()
        {
            var types = PluggableAssembly.SafeGetInterfacedTypes(typeof(IInterceptorHandler));
            foreach (var type in types)
            {
                Interceptor.RegisterHandlerType(type);
            }
        }

        /// <summary>
        /// Creates the instance by type and given arguments.
        /// </summary>
        /// <returns>The instance.</returns>
        /// <param name="type">Type of object</param>
        /// <param name="parameters">Parameters.</param>
        static object CreateInstance(Type type, params object[] parameters)
        {
            if (type == null) return null;

            return type.TryCreateInstance(parameters);
        }


        #endregion

    }

}
