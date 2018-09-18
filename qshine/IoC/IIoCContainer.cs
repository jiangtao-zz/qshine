using System;
using System.Reflection;
using qshine.Configuration;

namespace qshine

{
    /// <summary>
    /// IoC/DI Interface: Provide a common interface for DI implementation.
    /// NOTE: IoC/DI is a great pattern for SoC. Use it with caution. Make sure you fully understanding the concept of IoC/DI. 
    /// See Martin Fowler doc for more detail of concept: https://martinfowler.com/articles/injection.html
    /// and explain https://blogs.endjin.com/2014/04/understanding-dependency-injection/
    /// Beware the negative impact of the DI.
    ///     1. Lack of lifecycle control
    ///     2. Introduce extra complexity
    ///     3. Missing type compile time validation
    ///     4. Abuse interface-concrete class DI
    /// As a best practice:
    /// 1. Keep IoC in high level program (composition root or close to root) 
    ///    or, in low level infrastructure component (bootstrap or config file) to avoid coupling with your class implementation.
    /// 2. Use constructor injection only. (use in final class, avoid for class hierarchies)
    /// 3. Avoid setter or property injection (optional dependencies)
    /// 4. Avoid to use DI if it has only one interface implementation.
    /// </summary>	
    public interface IIocContainer:IDisposable
    {
        #region Resolve
        /// <summary>
        /// Get an instance of the requested type with the given name.
        /// </summary>
        /// <param name="requestedType">Requested type of object. usually, a interface or high level class</param>
        /// <param name="name">The name of the object to be retrieved from the container. 
        /// Different named requested type can instantiate a different type of implementation class,
        /// or a different parameterized class constructor.</param>
        /// <returns>The instance object to be return</returns>
        /// <remarks>
        /// The same requested type could be associated to many different actual types (polymorphism, class implementation). 
        /// Each of them should be named differently.
        /// For example:
        ///  In a multi-tenants application, most tenants consume a common service, but certain tenants require a special service. 
        ///  We create a interface ITenantService and implement two CommonTenantService and SpecialTenantService. 
        ///  IoC registered two service type registration, one is default and other is named "specialService".
        ///  Now, we can consume named service for special service: 
        ///     var specialService = container.Resolve&lt;ITenantService&gt;(specialTenantKey).
        ///  or, just get default service without name:
        ///     var service = container.Resolve&lt;ITenantService&gt;().
        ///  
        /// If no any registered type found, it raised exception. The returned instance could be a singleton or transient object depend on registered type lifescope.
        /// </remarks>
        object Resolve(Type requestedType, string name);

        /// <summary>
        /// Get an instance of the requested type from the container.
        /// </summary>
        /// <param name="requestedType">Requested type of object. The requested type could be an interface or class</param>
        /// <returns>The instance object to be return</returns>
        object Resolve(Type requestedType);

        /// <summary>
        /// Get an instance of the requested type from the container.
        /// </summary>
        /// <typeparam name="T">Requested type of object. The requested type could be an interface or class</typeparam>
        /// <returns>The instance object to be return</returns>
        /// <remarks>
        /// It's the most common way to get instance through IoC.
        /// </remarks>
        T Resolve<T>() where T : class;

        /// <summary>
        /// Get an instance of the requested type with the given name.
        /// </summary>
        /// <typeparam name="T">Requested type of object. usually, a interface or class</typeparam>
        /// <param name="name">The name of the object to be retrieved from container</param>
        /// <returns>The instance object to be return</returns>
        T Resolve<T>(string name) where T : class;

        #endregion

        #region RegisterType
        /// <summary>
        /// Register a named requested type (interface) and implementation class type association with a lifetime option.
        /// </summary>
        /// <param name="requestedType">requested type or type interface</param>
        /// <param name="actualType">Actual type of implementation class to be instanciated later by Resolve()</param>
        /// <param name="name">A name associate to a particular registration of interface and type calss association. 
        /// Null or String.Empty value indicates a default registration</param>
        /// <param name="lifetimeOption">Option to manage the life time of IoC instance creation. 
        /// valid values: 
        ///     Singleton -- single instance live forever
        ///     Transient -- new instance per resolver (default)
        /// </param>
        /// <param name="constructorParameters">Optional constructor parameters for class instanciate</param>
        /// <returns>Current container instance</returns>
        IIocContainer RegisterType(Type requestedType, Type actualType, string name, IocInstanceScope lifetimeOption, params NamedValue[] constructorParameters);

		/// <summary>
		/// Register a named requested type (interface) and implementation class type association.
		/// </summary>
		/// <returns>Current container instance</returns>
		/// <param name="requestedType">requested type or type interface</param>
		/// <param name="actualType">Actual type of implementation class to be instanciated later by Resolve()</param>
		/// <param name="name">Name.</param>
		/// <param name="constructorParameters">Optional constructor parameters for class instanciate</param>
		/// <remarks>
		/// The default liftime option is Singleton. The DefaultLifetime property could change this behavior.
		/// </remarks>
        IIocContainer RegisterType(Type requestedType, Type actualType, string name, params NamedValue[] constructorParameters);

		/// <summary>
		/// Register default requested type (interface) and implementation class type association with a lifetime option. 
		/// </summary>
		/// <returns>Current container instance</returns>
		/// <param name="requestedType">requested type or type interface</param>
		/// <param name="actualType">Actual type of implementation class to be instanciated later by Resolve()</param>
		/// <param name="lifetimeOption">Option to manage the life time of IoC instance creation.</param>
		/// <param name="constructorParameters">Optional constructor parameters for class instanciate</param>
        IIocContainer RegisterType(Type requestedType, Type actualType, IocInstanceScope lifetimeOption, params NamedValue[] constructorParameters);

		/// <summary>
		/// Register default requested type (interface) and implementation type class association. 
		/// </summary>
		/// <returns>Current container instance</returns>
		/// <param name="requestedType">requested type or type interface</param>
		/// <param name="actualType">Actual type of implementation class to be instanciated later by Resolve()</param>
		/// <param name="constructorParameters">Optional constructor parameters for class instanciate</param>
        IIocContainer RegisterType(Type requestedType, Type actualType, params NamedValue[] constructorParameters);

        /// <summary>
        /// Register a high level class type which will be resolved in component root to kick off chain of dependencies
        /// In most cases, this is not necessary. The Resolve wiring could be happen in configuration file module/component setting.
        /// </summary>
        /// <param name="actualType">High level class.</param>
		/// <param name="constructorParameters">Optional constructor parameters for class. 
        /// Do not put interface parameters in. They will be injected automatically.</param>
        /// <returns></returns>
        IIocContainer RegisterType(Type actualType, params NamedValue[] constructorParameters);

        /// <summary>
        /// Register a high level class type which will be resolved in component root to kick off chain of dependencies
        /// In most cases, this is not necessary. The Resolve wiring could be happen in configuration file module/component setting.
        /// </summary>
        /// <typeparam name="T">High level class.</typeparam>
        /// <param name="constructorParameters">Optional constructor parameters for class. 
        /// Do not put interface parameters in. They will be injected automatically.</param>
        /// <returns></returns>
        IIocContainer RegisterType<T>(params NamedValue[] constructorParameters);

        /// <summary>
        /// Register a named requested type (interface) and implementation class type association with a lifetime option.
        /// </summary>
        /// <typeparam name="IT">requested type or interface</typeparam>
        /// <typeparam name="T">Actual type of implementation class to be instanciated later by Resolve()</typeparam>
        /// <param name="name">A name associate to particular registration of interface and type calss association. null or String.Empty for default registration</param>
        /// <param name="lifetimeOption">Option to manage the life time of IoC instance creation.</param>
        /// <param name="constructorParameters">Optional constructor parameters for class instanciate</param>
        /// <returns>Current container instance</returns>
        IIocContainer RegisterType<IT, T>(string name, IocInstanceScope lifetimeOption, params NamedValue[] constructorParameters)
            where IT : class
            where T : class, IT;

		/// <summary>
		/// Register a named requested type (interface) and implementation class type association.
		/// </summary>
		/// <typeparam name="IT">requested type or interface</typeparam>
		/// <typeparam name="T">Actual type of implementation class to be instanciated later by Resolve()</typeparam>
		/// <param name="name">A name associate to particular registration of interface and type calss association. null or String.Empty for default registration</param>
		/// <param name="constructorParameters">Optional constructor parameters for class instanciate</param>
		/// <returns>Current container instance</returns>
        IIocContainer RegisterType<IT, T>(string name, params NamedValue[] constructorParameters)
            where IT : class
            where T : class, IT;

		/// <summary>
		/// Register default requested type (interface) and implementation class type association with a lifetime option. 
		/// </summary>
		/// <returns>Current container instance</returns>
		/// <typeparam name="IT">requested type or interface</typeparam>
		/// <typeparam name="T">Actual type of implementation class to be instanciated later by Resolve()</typeparam>
		/// <param name="lifetimeOption">Option to manage the life time of IoC instance creation.</param>
		/// <param name="constructorParameters">Optional constructor parameters for class instanciate</param>
        IIocContainer RegisterType<IT, T>(IocInstanceScope lifetimeOption, params NamedValue[] constructorParameters)
            where IT : class
            where T : class, IT;

		/// <summary>
		/// Register default requested type (interface) and implementation type class association. 
		/// </summary>
		/// <returns>Current container instance</returns>
		/// <typeparam name="IT">requested type or interface</typeparam>
		/// <typeparam name="T">Actual type of implementation class to be instanciated later by Resolve()</typeparam>
		/// <param name="constructorParameters">Optional constructor parameters for class instanciate</param>
        IIocContainer RegisterType<IT, T>(params NamedValue[] constructorParameters)
            where IT : class
            where T : class, IT;

		/// <summary>
		/// Register a named requested type (interface) and implementation class type association with a lifetime option.
		/// </summary>
		/// <typeparam name="IT">requested type or interface</typeparam>
		/// <param name="actualType">Actual type of implementation class to be instanciated later by Resolve()</param>
		/// <param name="name">A name associate to particular registration of interface and type calss association. null or String.Empty for default registration</param>
		/// <param name="lifetimeOption">Option to manage the life time of IoC instance creation.</param>
		/// <param name="constructorParameters">Optional constructor parameters for class instanciate</param>
		/// <returns>Current container instance</returns>
        IIocContainer RegisterType<IT>(Type actualType, string name, IocInstanceScope lifetimeOption, NamedValue[] constructorParameters)
            where IT : class;

		/// <summary>
		/// Register a named requested type (interface) and implementation class type association.
		/// </summary>
		/// <typeparam name="IT">requested type or interface</typeparam>
		/// <param name="actualType">Actual type of implementation class to be instanciated later by Resolve()</param>
		/// <param name="name">A name associate to particular registration of interface and type calss association. null or String.Empty for default registration</param>
		/// <param name="constructorParameters">Optional constructor parameters for class instanciate</param>
		/// <returns>Current container instance</returns>
        IIocContainer RegisterType<IT>(Type actualType, string name, params NamedValue[] constructorParameters)
            where IT : class;

		/// <summary>
		/// Register default requested type (interface) and implementation class type association with a lifetime option. 
		/// </summary>
		/// <returns>Current container instance</returns>
		/// <typeparam name="IT">requested type or interface</typeparam>
		/// <param name="actualType">Actual type of implementation class to be instanciated later by Resolve()</param>
		/// <param name="lifetimeOption">Option to manage the life time of IoC instance creation.</param>
		/// <param name="constructorParameters">Optional constructor parameters for class instanciate</param>
        IIocContainer RegisterType<IT>(Type actualType, IocInstanceScope lifetimeOption, NamedValue[] constructorParameters)
            where IT : class;

		/// <summary>
		/// Register default requested type (interface) and implementation type class association. 
		/// </summary>
		/// <returns>Current container instance</returns>
		/// <typeparam name="IT">requested type or interface</typeparam>
		/// <param name="actualType">Actual type of implementation class to be instanciated later by Resolve()</param>
		/// <param name="constructorParameters">Optional constructor parameters for class instanciate</param>
        IIocContainer RegisterType<IT>(Type actualType, params NamedValue[] constructorParameters)
            where IT : class;

        #endregion

        #region RegisterInstance
        /// <summary>
        /// Register a requested type (interface) mapping to an actual implementation class instance.
        /// </summary>
        /// <typeparam name="IT">requested type or interface</typeparam>
        /// <param name="instance">Object to be returned</param>
        /// <returns>Current container</returns>
        /// <remarks>
        /// The same instance will be returned regardless how the lifetime option choose.
        /// The instance dispose way may different for each IoC implemention. Some container may have a reference 
        /// to the instance, some may not.
        /// </remarks>
        IIocContainer RegisterInstance<IT>(IT instance) where IT : class;

		/// <summary>
		/// Register a named requested type (interface) mapping to an actual implementation class instance.
		/// </summary>
		/// <typeparam name="IT">requested type or interface</typeparam>
		/// <param name="instance">Object to be returned</param>
		/// <param name="name">A name associate to particular registration of interface and type calss instance association.</param>
		/// <returns>Current container</returns>
		/// <remarks>
		/// The same instance will be returned regardless how the lifetime option choose.
		/// The instance dispose way may different for each IoC implemention. Some container may have a reference 
		/// to the instance, some may not.
		/// </remarks>
        IIocContainer RegisterInstance<IT>(IT instance, string name) where IT : class;

        /// <summary>
        /// Register a named requested type mapping to a particular instance.
        /// </summary>
        /// <param name="requestedType">Requested type</param>
        /// <param name="instance">Object to be returned</param>
        /// <param name="name">A name associate to a particular registration</param>
        /// <returns>Current container</returns>
        IIocContainer RegisterInstance(Type requestedType, object instance,string name);
        #endregion


        /// <summary>
        /// Register required types and implementation classes from a separated module insatnce
        /// </summary>
        /// <returns>Current container</returns>
        /// <param name="module">IoC registration module instance.</param>
        /// <remarks>
        /// This is a convenient way to register all the types IoC suppose to resolve. A specific assembly module will be loaded
        /// automatically based on configuration setting or directly get from assembly.
        /// </remarks>
        IIocContainer RegisterModule(object module);

		/// <summary>
		/// Register required types and implementation classes from an assembly which implemented IoC registration module
		/// </summary>
		/// <returns>Current container</returns>
		/// <param name="assembly">Assembly contains IoC registration module</param>
        IIocContainer RegisterModule(Assembly assembly);

        /// <summary>
        /// Get/Set default IoC life time scope for an instance get from container
        /// </summary>
		IocInstanceScope DefaultInstanceScope {get;set;}

		/// <summary>
		/// Bind the container life time scope to current context. 
		/// The life time scope will hold disposable objects reference and release them when the context life time scope end.
		/// </summary>
		void Bind();

		/// <summary>
		/// Unbound the life time scope from current context.
		/// It will release all bound disposable objects.
		/// </summary>
		void Unbind();

    }
}
