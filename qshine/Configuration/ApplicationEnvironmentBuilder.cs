using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
#if NETCORE
using System.Runtime.Loader;
#endif
using System.Security;
using System.Text;
using qshine.Configuration.ConfigurationStore;
using qshine.Globalization;
using qshine.Utility;


namespace qshine.Configuration
{
    /// <summary>
    /// Build application environment instance
    /// </summary>
    /// <example>
    /// <![CDATA[
    ///     
    ///     var options = new EnvironmentInitializationOption {
    ///         OverwriteConnectionString = true
    ///     };
    ///     var builder = new ApplicationEnvironmentBuilder();
    ///     
    ///     builder
    ///         .Config((appContext, config)=>{
    ///             config.LoadConfigFile("app.config", options);
    ///             config.AddCommandLine(args);
    ///             appContext.PlugableAssemblyFilter((assembly)=>{
    ///                     return assembly.FullName.Contains("MyService.P1"));
    ///                 }
    ///             })
    ///         .Build()
    ///         .AddComponent<IMyService, MyService>()
    ///         .AddComponent<IMyService, MyService2>(name)
    ///         .AddComponent<IMyService, MyService3>(name, arg1,arg2,arg3)
    ///         .StartUp<Bootstrapper>()
    /// 
    /// 
    /// ]]>
    /// 
    ///     ApplicationEnvironment(1) => EnvironmentConfigure(*)
    ///     ApplicationEnvironmentContext(1) ==> EnvironmentConfigure(1)
    ///     ApplicationEnvironmentBuilder(1) ==> ApplicationEnvironmentContext(as parameter)
    ///     PlugableAssembly => assembly
    ///     PlugableComponent => Type, assembly, instance
    ///     
    /// </example>
    public class ApplicationEnvironmentBuilder
    {
        private ApplicationEnvironmentContext _context;
        private EnvironmentConfigure _environmentConfigure;
        private string environmentName;

        /// <summary>
        /// Build default ApplicationEnvironment.
        /// </summary>
        public ApplicationEnvironmentBuilder()
            :this("")
        { }

        /// <summary>
        /// Build a named ApplicationEnvironment.
        /// </summary>
        /// <param name="name"></param>
        public ApplicationEnvironmentBuilder(string name)
            :this(ApplicationEnvironmentContext.GetContext(name))
        {
            environmentName = name;
        }

        /// <summary>
        /// Build ApplicationEnvironment with given application environment context.
        /// </summary>
        /// <param name="context"></param>
        public ApplicationEnvironmentBuilder(ApplicationEnvironmentContext context)
        {
            _context = context;
            _environmentConfigure = _context.EnvironmentConfigure;
        }

        /// <summary>
        /// Configure application environment
        /// </summary>
        /// <param name="configureSetting">application environment configure delegate</param>
        /// <returns></returns>
        public ApplicationEnvironmentBuilder Configure(Action<ApplicationEnvironmentContext, EnvironmentConfigure> configureSetting)
        {
            configureSetting(_context, _environmentConfigure);
            return this;
        }

        /// <summary>
        /// Build applicationEnvironment
        /// </summary>
        /// <returns></returns>
        public ApplicationEnvironment Build()
        {
            //Load runtime assemblies
            LoadRuntimeComponents();

            //Set application assembly resolver for find pluggable assembly
            SetAssemblyResolve();


            //Raise enter event
            var eventArg = new InterceptorEventArgs("Build");
            _interceptor.RaiseOnEnterEvent(this, eventArg);

            //Load binary setting
            LoadBinaryFiles();

            //Load components from binary assembly
            LoadComponents();

            //Load modules
            LoadModules();

            //Load intercept handlers after all plugin components loaded
            Interceptor.LoadInterceptors();

            //Raise completion event
            _interceptor.RaiseOnSuccessEvent(this, eventArg);

            return new ApplicationEnvironment(_context);
        }


        ///// <summary>
        ///// Add additional component into application environment
        ///// </summary>
        ///// <param name="component"></param>
        ///// <returns></returns>
        //public ApplicationEnvironment AdditionalComponent(PlugableComponent component)
        //{
        //    if (!EnvironmentConfigure.Components.Any(x => x.Key == component.Name))
        //    {
        //        EnvironmentConfigure.Components.Add(component.Name, component);
        //        LoadComponent(component);
        //    }
        //    return this;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public ApplicationEnvironmentBuilder AddConfigure(IConfigurationStore config)
        {
            throw new NotFiniteNumberException();
        }

        /// <summary>
        /// Load components from application configuration file
        /// </summary>
		void LoadComponents()
        {
            foreach (var c in _environmentConfigure.Components)
            {
                LoadComponent(c);
            }
        }

        /// <summary>
        /// Load modules from application configuration file
        /// </summary>
		void LoadModules()
        {
            foreach (var c in _environmentConfigure.Modules)
            {
                LoadModule(c);
            }
        }

        /// <summary>
        /// Try to load a module.
        /// </summary>
        /// <param name="module">Module is a plugin component that will be auto loaded through application eenvironment.
        /// The module initialization could be implemented in type constructor or type static constructor (If initialization call only once.).
        /// </param>
        /// <remarks>
        /// The module must have a public constructor without parameter. 
        /// </remarks>
        void LoadModule(PluggableComponent module)
        {
            if (module.Instantiate(_context))
            {
                //All module instance will be created when the component is loaded into the environment.
                //The module usually is a singleton instance.
                module.CreateInstance();
                LogInfo("AE.LoadModule:: {0}"._G(module.FormatObjectValues()));
            }
            else
            {
                //do not raise exception.
                InnerError("1002", "{0}:: {1}"._G(module.InvalidReason, module.FormatObjectValues()));
            }
        }

        void LoadComponent(PluggableComponent component)
        {
            if (component.Instantiate(_context))
            {
                LogInfo("AE.LoadComponent:: {0}"._G(component.FormatObjectValues()));
            }
            else
            {
                //do not raise exception.
                InnerError("1003", "{0}:: {1}"._G(component.InvalidReason, component.FormatObjectValues()));
            }
        }

        #region Resolver

        [SecuritySafeCritical]
        void SetAssemblyResolve()
        {
#if NETCORE
            AssemblyLoadContext.Default.Resolving += OnResolving;
#else
            AppDomain.CurrentDomain.AssemblyResolve += ApplicationAssemblyResolve;
#endif
        }

#if NETCORE
        private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
        {
            return ApplicationAssemblyResolve(context, new ResolveEventArgs(name.FullName));
        }

#endif

        /// <summary>
        /// Resolve assembly location when lookup type by a qualified type name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Assembly ApplicationAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Ignore missing resources
            if (args.Name.Contains(".resources")) return null;

            var assemblyParts = args.Name.Split(',');
            //Should never happen
            if (assemblyParts.Length == 0) return null;

            var assemblyName = assemblyParts[0];

            //1. ** If assembly alreay in mapping list, directly load the assembly from the map**
            //**Note:** If the application contains more than one application environments, all those environments run under default app domain. 
            //Only single version assembly will be loaded to the mapping list.
            //Load sequence is based on searching order starting from application configuration entrypoint.
            //To avoid version conflict, DO NOT add multiple versions in configuration binary folder.
            Assembly assembly = _context.PlugableAssemblies.GetAssembly(assemblyName);

            if (assembly != null) return assembly;

            ////2. ** try to load assembly from default load context ***
            ////The default load context runtime components should be loaded first before load other components to "load-from context".
            ////The "default load context" component cannot load dependency from other context. if a dependency assembly already exists 
            ////in other configuration binary folder, try to load dependency dll earlier before load plugable component. 
            //// check for assemblies already loaded by the current application domain. It is necessary. See. 
            //// https://docs.microsoft.com/en-us/dotnet/framework/deployment/best-practices-for-assembly-loading
            {
                var pluggableAssembly = _context.PlugableAssemblies[assemblyName];

                //Try to load assembly from different application configuration folders
                if (pluggableAssembly == null)
                {
                    //Try to load configuration binary folders into mapping list, if not loaded yet
                    LoadBinaryFiles();
                }

                //3. ** Try get assembly from "load-from context" and put in mapping list
                pluggableAssembly = _context.PlugableAssemblies[assemblyName];

                if (pluggableAssembly != null)
                {
                    //Assembly already in "load-from context" 
                    if (pluggableAssembly.Assembly != null)
                    {
                        return pluggableAssembly.Assembly;
                    }
                    //Load assembly from configured path and add in "load-from context"
                    //This may throw exception. The exception will be captured when call assemby Load()
#if NETCORE
                    assembly = ApplicationAssemblyResolver.Resolve(pluggableAssembly.Path);
#else
                    assembly = Assembly.LoadFrom(pluggableAssembly.Path);
#endif
                    if (assembly != null)
                    {
                        pluggableAssembly.Assembly = assembly;
                    }
                }
                else
                {
                    //Couldn't find assembly
                    InnerWarning("1000", "Couldn't resolve assembly {0}."._G(args.Name));
                }
            }

            return assembly;
        }

        List<string> _externalDepsJsonPaths = new List<string>();


        /// <summary>
        /// Add all assembly files from binary folders into application environment.
        /// The AssemblyResolveHandler try to resolve assembly from those files.
        /// </summary>
		bool LoadBinaryFiles()
        {
            bool hasNewBinaryFile = false;

            foreach (var binPath in _environmentConfigure.AssemblyFolders)
            {
                if (!binPath.State && Directory.Exists(binPath.ObjectData))
                {
#if NETCORE
                    foreach (var depFile in new DirectoryInfo(binPath.ObjectData).GetFiles("*.deps.json"))
                    {
                        if (!_externalDepsJsonPaths.Contains(depFile.FullName))
                        {
                            _externalDepsJsonPaths.Add(depFile.FullName);
                        }
                    }
#endif

                    //bool hasBinary = false;
                    foreach (var dll in new DirectoryInfo(binPath.ObjectData).GetFiles("*.dll"))
                    {
                        var assemblyName = Path.GetFileNameWithoutExtension(dll.FullName);
                        var pluggableAssembly = _context.PlugableAssemblies[assemblyName];

                        if (pluggableAssembly == null)
                        {
                            _context.PlugableAssemblies.Add(assemblyName, new PluggableAssembly
                            {
                                Path = dll.FullName,
                                Assembly = SafeLoadAssembly(dll.FullName)

                            });
                            //Found new binary file.
                            hasNewBinaryFile = true;
                        }
                        else if (pluggableAssembly.Path != dll.FullName)
                        {
                            InnerWarning("1001", "Assembly {0} dll already loaded."._G(dll.FullName));
                        }
                        //hasBinary = true;
                    }
                    //Mark the flag that assembly folder dlls loaded in folder mapping list
                    binPath.State = true;
                }
            }
            return hasNewBinaryFile;
        }

        /// <summary>
        /// Load assembly from plugin folder
        /// </summary>
        /// <param name="path">path of plugin component</param>
        /// <returns></returns>
        Assembly SafeLoadAssembly(string path)
        {
            try
            {
#if NETCORE
                var assembly = ApplicationAssemblyResolver.Resolve(path);
#else
            var assembly = Assembly.LoadFrom(path);
#endif
                return assembly;
            }
            catch (Exception ex)
            {
                InnerError("2000", "Failed to load assembly {0}. ({1})"._G(path, ex.Message));
                return null;
            }
        }

        /// <summary>
        /// Load application components from runtime location to application environment.
        /// Those components types could be resolved directly from run-time.
        /// The mapped runtime application components will be part of accessable types for plugable application environment.
        /// It will not include most system or common share components.
        /// </summary>
        void LoadRuntimeComponents()
        {
            if (_context.RuntimeAssemblies == null)
            {
                return;
            }

            foreach (var a in _context.RuntimeAssemblies)
            {
                //Skip system assemblies
                if (_context.PlugableAssemblyFilter != null && !_context.PlugableAssemblyFilter(a))
                {
                    continue;
                }

                //Get assembly name without version and culture info.
                var assemblyNameObject = new AssemblyName(a.FullName);
                var assemblyName = assemblyNameObject.Name;

                if (!_context.PlugableAssemblies.Contains(assemblyName))
                {
                    _context.PlugableAssemblies.Add(assemblyName, new PluggableAssembly
                    {
                        Path = a.Location,
                        Assembly = a
                    });
                }
            }
        }

        private MessageCode _messageCode = new MessageCode("AEB");
        void InnerError(string code, string message)
        {
            _environmentConfigure.BuildErrorHandler?.Invoke(_messageCode.ToString(code), message);
        }

        void InnerWarning(string code, string message)
        {
            _environmentConfigure.BuildErrorHandler?.Invoke(_messageCode.ToString(code), message);
        }

        #endregion

        void LogInfo(string message)
        {

        }

        #region Static private

        static Interceptor _interceptor;

        /// <summary>
        /// Static ctor:: pre-initialization when first touch the ApplicationEnvironmentBuilder component
        /// </summary>
        static ApplicationEnvironmentBuilder()
        {
            //Register ApplicationEnvironment interceptor
            _interceptor = Interceptor.Get<ApplicationEnvironmentBuilder>();
        }

        #endregion

    }
}
