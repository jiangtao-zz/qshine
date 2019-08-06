using qshine.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace qshine.Configuration
{
    /// <summary>
    /// Application environment context.
    /// The running application could have more than one application environment settings.
    /// Same named application environments shared same application environment context.
    /// </summary>
    public class ApplicationEnvironmentContext
    {
        /// <summary>
        /// Ctor
        /// </summary>
        internal ApplicationEnvironmentContext(string name)
        {
            EnvironmentConfigure = new EnvironmentConfigure();
            Properties = new SafeDictionary<string, object>();

            //Get default possible Runtime plugable assemblies
            RuntimeAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            //Get default filter
            PlugableAssemblyFilter = IsPlugableAssembly;

            //Create plugable assemblies instance
            PlugableAssemblies = new PluggableAssemblyCollection();
            Name = name;
        }

        ///// <summary>
        ///// application environment
        ///// </summary>
        //public ApplicationEnvironment ApplicationEnvironment { get; set; }

        /// <summary>
        /// Application environment configure setting
        /// </summary>
        public EnvironmentConfigure EnvironmentConfigure { get; set; }

        /// <summary>
        /// Context properties
        /// </summary>
        public SafeDictionary<string, object> Properties { get; private set; }

        /// <summary>
        /// Get/Set Runtime Assemblies.
        /// The default runtime assemblies is from AppDomain.CurrentDomain.GetAssemblies() 
        /// </summary>
        public Assembly[] RuntimeAssemblies { get; set; }

        /// <summary>
        /// A delegate to filter out non-plugable assemblies from RuntimeAssemblies.
        /// The delegate returns true if it is a plugable (service) assembly.
        /// </summary>
        public Func<Assembly, bool> PlugableAssemblyFilter { get; set; }

        /// <summary>
        /// Plugable assemblies
        /// </summary>
        public PluggableAssemblyCollection PlugableAssemblies { get; private set; }

        /// <summary>
        /// Get application environment context name
        /// </summary>
        public string Name
        {
            get; private set;
        }

        #region public static methods and properties

        /// <summary>
        /// Get default application environment context
        /// </summary>
        public static ApplicationEnvironmentContext Default
        {
            get
            {
                return GetContext(string.Empty);
            }
        }

        /// <summary>
        /// Get named application environment context
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ApplicationEnvironmentContext GetContext(string name)
        {
            var contextName = GetEnvironmentContextName(name);
            var context = ContextStore.GetData(contextName);
            if (! (context is ApplicationEnvironmentContext))
            {
                lock (lockObject)
                {
                    context = ContextStore.GetData(contextName) as ApplicationEnvironmentContext;
                    if (context == null)
                    {
                        context = new ApplicationEnvironmentContext(name);
                    }
                }
            }
            return context as ApplicationEnvironmentContext;
        }
        /// <summary>
        /// Get/Set current environment context store.
        /// The default current environment context store is Static Context store. It allows single current application environment.
        /// You may want to have a different context store to have current application environment based on different context. 
        /// In this case you can implement a special EnvironmentContextStore.
        /// The EnvironmentContextStore need be set in application Startup before ApplicationEnvironment.Boot()
        /// </summary>
        public static IContextStore ContextStore
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

        static string GetEnvironmentContextName(string name)
        {
            var contextName = environmentContextStoreName;
            if (!string.IsNullOrEmpty(name))
            {
                contextName += "_" + name;
            }
            return contextName;
        }

        static readonly object lockObject = new object();
        static IContextStore _contextStore;
        /// <summary>
        /// Default environment context store name.
        /// </summary>
        const string environmentContextStoreName = "_$$envContext.current";

        /// <summary>
        /// Determine whether an assembly could be a plugable (service) assembly
        /// </summary>
        /// <param name="assembly">assembly object</param>
        /// <returns></returns>
        bool IsPlugableAssembly(Assembly assembly)
        {
            if (assembly == null) return false;

            //Ignore dynamic assembly
            if (assembly.IsDynamic) return false;

            string location = assembly.Location;
            var fullName = assembly.FullName;
            bool ignoreAssembly = String.IsNullOrEmpty(location) || //ignore byte array loaded assembly
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

            return !ignoreAssembly;
        }


        #endregion
    }
}
