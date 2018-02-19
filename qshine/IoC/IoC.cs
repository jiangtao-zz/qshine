using System;
using System.Collections.Generic;
using qshine.Configuration;
using qshine.Globalization;

namespace qshine.IoC
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
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Io")]
    public class IoC
    {

        #region Fields

		private static object _providerLockObject = new Object();
        private static IIoCProvider _provider;

		private static object _containerLockObject = new Object();
		private static IIoCContainer _container;

		private static IoCLifetimeScope defaultLifetimeScope = IoCLifetimeScope.Transient;

        #endregion

		#region Get/Set default lifetime scope
		/// <summary>
		/// get/set global level default lifetime scope for container
		/// </summary>
		public static IoCLifetimeScope DefaultLifetimeScope
		{
			get
			{
				return defaultLifetimeScope;
			}
			set
			{
				defaultLifetimeScope = value;
				Current.DefaultLifetimeScope = value;
			}
		}
		#endregion


        #region Container provider


		public static IIoCProvider Provider{
			get{
				if(_provider==null){
					lock(_providerLockObject){
						if(_provider==null){
							_provider = EnvironmentManager.GetProvider(typeof(IIoCProvider)) as IIoCProvider;
							if(_provider==null){
								_provider = new TinyIoCProvider();
							}
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
                
        #region Get current container


        /// <summary>
        /// Get IoC container to resolve interface dependency at granularity level.
        /// </summary>
        /// <remarks>
        /// The Container property expose the Autofac container that gives user more control on the IoC container.
        /// In most cases, we should not use this property, instead, call Resolve() method to get the concrete class instance.
        /// </remarks>
        static public IIoCContainer Current
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

        static public IIoCContainer RegisterType<IT, T>()
            where IT : class
            where T : class, IT
        {
            return Current.RegisterType<IT, T>();
        }

        static public IIoCContainer RegisterType<IT, T>(string name)
            where IT : class
            where T : class, IT
        {
            return Current.RegisterType<IT, T>(name);
        }

        static public IIoCContainer RegisterType<IT, T>(IoCLifetimeScope lifetimeScope)
            where IT : class
            where T : class, IT
        {
            return Current.RegisterType<IT, T>(lifetimeScope);
        }

        static public IIoCContainer RegisterType<IT, T>(string name, IoCLifetimeScope lifetimeScope)
            where IT : class
            where T : class, IT
        {
            return Current.RegisterType(typeof(IT),typeof(T),name,lifetimeScope);
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

        static public T Resolve<T>(string name) where T : class
        {
            return Current.Resolve<T>(name);
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


