using System;
using System.Collections.Generic;
using qshine.Configuration;
using qshine.Globalization;

namespace qshine
{

//    public delegate IIoCContainer IoCProvider();

    /// <summary>
    /// IoC class provides a common interface wrap for the third party components of Inversion of control/Dependency Injection.
    /// 
    /// Usage:
    ///   Step 1: Set your IoC provider if choose a third party IoC component.
    ///     This step could be skip if you don't have a particular choice. In this case, a TinyIoC will be used.
    ///     The framework implemented several common IoC providers. A custom IoC provider is accepted.
    ///     
    ///   Step 2: Register all types that need be used by DI. The best place doing this is in application initialization.
    ///   
    ///   Step 3: Call Resolve() to get the instance for particular type of class which is registered early.
	/// 
	///   To auto release the instances created by IoC container you can use following technique:
	/// 	IoC.Bind(httpContext);
	/// 	IoC.Unbind(httpContext);
	/// 
	/// or iocContainer.Bind(httpContext)/Unbind(httpContext;
    ///   
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Io")]
    public class IoC
    {

        #region Fields

		private static object _providerLockObject = new Object();
        private static IIocProvider _provider;

		private static object _containerLockObject = new Object();
		private static IIocContainer _container;

		private static IocInstanceScope _defaultInstanceScope = IocInstanceScope.Transient;

        #endregion

		#region Get/Set default lifetime scope
		/// <summary>
		/// get/set default insatnce scope
		/// </summary>
		public static IocInstanceScope DefaultInstanceScope
		{
			get
			{
				return _defaultInstanceScope;
			}
			set
			{
				_defaultInstanceScope = value;
				Current.DefaultInstanceScope = value;
			}
		}
		#endregion


		#region Container provider

		static IIocProvider _internalIoCProvider = new TinyIocProvider();
		/// <summary>
		/// Gets or sets IoC provider.
		/// </summary>
		/// <value>The provider.</value>
		public static IIocProvider Provider{
			get{
				if(_provider==null){
					lock(_providerLockObject){
						if(_provider==null){
                            //|| _provider == _internalIoCProvider
                            var provider = ApplicationEnvironment.Default.Services.GetProvider<IIocProvider>();
                            if (provider != null)
                            {
                                _provider = provider;
                            }
                            //try to load a plugable IoC provider
                            return _provider??_internalIoCProvider;
						}
					}
				}
				return _provider;
			}
			set{
				_provider = value;
			}
		}

		#endregion

		/// <summary>
		/// Create a new container.
		/// </summary>
		/// <returns>The container.</returns>
		public static IIocContainer CreateContainer()
		{
			return Provider.CreateContainer();
		}
                
        #region Get current container


        /// <summary>
        /// Get IoC container to resolve interface dependency at granularity level.
        /// </summary>
        /// <remarks>
        /// The Container property expose the Autofac container that gives user more control on the IoC container.
        /// In most cases, we should not use this property, instead, call Resolve() method to get the concrete class instance.
        /// </remarks>
        static public IIocContainer Current
        {
            get
            {
                if (_container == null)
                {
                    lock (_containerLockObject)
                    {
                        //try to get default IoC setting
                        if (_container == null)
                        {
                            //get container
							_container = Provider.CreateContainer();
                        }
                    }
                }
                return _container;
            }
        }

        #endregion


        #region Shortcut of Current.RegisterType

        /// <summary>
        /// Register a service within default scope
        /// </summary>
        /// <typeparam name="IT">service interface</typeparam>
        /// <typeparam name="T">service implementation class</typeparam>
        /// <returns>current container</returns>
        static public IIocContainer RegisterType<IT, T>()
            where IT : class
            where T : class, IT
        {
            return Current.RegisterType<IT, T>();
        }

        /// <summary>
        /// Register a named service within default scope
        /// </summary>
        /// <typeparam name="IT">service interface</typeparam>
        /// <typeparam name="T">service implementation class</typeparam>
        /// <param name="name">Specifies the service name</param>
        /// <returns>current container</returns>
        static public IIocContainer RegisterType<IT, T>(string name)
            where IT : class
            where T : class, IT
        {
            return Current.RegisterType<IT, T>(name);
        }

        /// <summary>
        /// Register a service within a scope. 
        /// </summary>
        /// <typeparam name="IT">service interface</typeparam>
        /// <typeparam name="T">service implementation class</typeparam>
        /// <param name="instanceScope">service instance scope</param>
        /// <returns>current container</returns>
        static public IIocContainer RegisterType<IT, T>(IocInstanceScope instanceScope)
            where IT : class
            where T : class, IT
        {
            return Current.RegisterType<IT, T>(instanceScope);
        }

        /// <summary>
        /// Register a named scope within a scope
        /// </summary>
        /// <typeparam name="IT">service interface</typeparam>
        /// <typeparam name="T">service implementation class</typeparam>
        /// <param name="instanceScope">service instance scope</param>
        /// <param name="name">Specifies the service name</param>
        /// <returns>current container</returns>
        static public IIocContainer RegisterType<IT, T>(string name, IocInstanceScope instanceScope)
            where IT : class
            where T : class, IT
        {
            return Current.RegisterType(typeof(IT),typeof(T),name,instanceScope);
        }

        /// <summary>
        /// Add a transient service
        /// </summary>
        /// <typeparam name="IT">service interface</typeparam>
        /// <typeparam name="T">service implementation class</typeparam>
        /// <returns>current container</returns>
        static public IIocContainer AddTransient<IT, T>()
        {
            return Current.RegisterType(typeof(IT), typeof(T), IocInstanceScope.Transient);
        }

        /// <summary>
        /// Add a named transient service
        /// </summary>
        /// <typeparam name="IT">service interface</typeparam>
        /// <typeparam name="T">service implementation class</typeparam>
        /// <param name="name">Specifies the service name</param>
        /// <returns>current container</returns>
        static public IIocContainer AddTransient<IT, T>(string name)
        {
            return Current.RegisterType(typeof(IT), typeof(T), name, IocInstanceScope.Transient);
        }

        /// <summary>
        /// Add a singleton service
        /// </summary>
        /// <typeparam name="IT">service interface</typeparam>
        /// <typeparam name="T">service implementation class</typeparam>
        /// <returns>current container</returns>
        static public IIocContainer AddSingleton<IT, T>()
        {
            return Current.RegisterType(typeof(IT), typeof(T), IocInstanceScope.Singleton);
        }

        /// <summary>
        /// Add a named singleton service
        /// </summary>
        /// <typeparam name="IT">service interface</typeparam>
        /// <typeparam name="T">service implementation class</typeparam>
        /// <param name="name">Specifies the service name</param>
        /// <returns>current container</returns>
        static public IIocContainer AddSingleton<IT, T>(string name)
        {
            return Current.RegisterType(typeof(IT), typeof(T), name, IocInstanceScope.Singleton);
        }

        /// <summary>
        /// Add a scoped service
        /// </summary>
        /// <typeparam name="IT">service interface</typeparam>
        /// <typeparam name="T">service implementation class</typeparam>
        /// <returns>current container</returns>
        static public IIocContainer AddScoped<IT, T>()
        {
            return Current.RegisterType(typeof(IT), typeof(T), IocInstanceScope.Scoped);
        }

        /// <summary>
        /// Add a named scoped service
        /// </summary>
        /// <typeparam name="IT">service interface</typeparam>
        /// <typeparam name="T">service implementation class</typeparam>
        /// <param name="name">Specifies the service name</param>
        /// <returns>current container</returns>
        static public IIocContainer AddScoped<IT, T>(string name)
        {
            return Current.RegisterType(typeof(IT), typeof(T), name, IocInstanceScope.Scoped);
        }

        #endregion

        #region Shortcut of Current.Resolve
        /// <summary>
        /// Wireup IoC interfaces with corresponding concrete implementation through Autofac components Resolve corresponding repository interface
        /// </summary>
        /// <typeparam name="T">Type of interface ready for consume</typeparam>
        /// <returns>A concrete class instance associate with specified type of interface</returns>
        /// <remarks>
        /// This is a common and generic method to resolve the IoC dependency. 
        /// Call this method you don't need add any reference points to specific IoC/DI components (in this case
        /// it's the Autofac).
        /// But when the IoC become more complex, suggest using Builder and Container properties to register and resolve
        /// interface and dependency.
        /// 
        /// Pre-condition:
        /// The type of interface to be invoked should be register earier. We suggest using structed module to register 
        /// the IoC for concrete classes that could minimize the dependency between concrete class and interface consumer.
        /// </remarks>
        static public T Resolve<T>() where T : class
        {
            return Current.Resolve<T>();
        }
        /// <summary>
        /// Resolve a named service
        /// </summary>
        /// <typeparam name="T">service interface</typeparam>
        /// <param name="name">The service name</param>
        /// <returns>A concrete class instance associate with specified type of interface</returns>
        static public T Resolve<T>(string name) where T : class
        {
            return Current.Resolve<T>(name);
        }

		#endregion

		#region Bind/Unbind

		/// <summary>
		/// bind injected instances to context
		/// </summary>
		static public void Bind()
		{
			Current.Bind();
		}

		/// <summary>
		/// Unbind/release all bound injected instances
		/// </summary>
		static public void Unbind()
		{
			Current.Unbind();
		}

		#endregion

		#region Private

		/// <summary>
		/// Hide constructor
		/// </summary>
		private IoC() { }

        #endregion

    }

}


