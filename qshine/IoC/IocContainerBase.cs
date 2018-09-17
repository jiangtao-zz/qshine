using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Globalization;
using qshine.Configuration;
using qshine.Globalization;
using System.Runtime.Remoting.Messaging;

namespace qshine

{
	/// <summary>
	/// Base implementation of IIoCContainer for IoC.
	/// The actual concrete IoC container should be inherited from this base
	/// </summary>
	public abstract class IocContainerBase : IIocContainer
	{

		#region fields
		IocInstanceScope _defaultInstanceScope = IoC.DefaultInstanceScope;
		#endregion

		#region IIoCContainer Members

		#region DefaultInstanceScope

		public virtual IocInstanceScope DefaultInstanceScope
		{
			get
			{
				return _defaultInstanceScope;
			}
			set
			{
				_defaultInstanceScope = value;
			}
		}

		#endregion

		#region Resolve

		/// <summary>
		/// Get actual implementation class instance of the requested type (interface) from container
		/// </summary>
		/// <typeparam name="T">Requested type. usually, a interface or base class</typeparam>
		/// <returns>The actual implementation class instance object to be return</returns>
		/// <remarks>
		/// If no any matched type found, it raised exception. The instance could be a singleton or transient object
		/// </remarks>
		public virtual T Resolve<T>()
			where T : class
		{
			var instance = (T)Resolve(typeof(T), String.Empty);
			return instance;
		}

		/// <summary>
		/// Get actual named implementation class instance of the requested type (interface) from container
		/// </summary>
		/// <typeparam name="T">Requested type of object. usually, a interface or class</typeparam>
		/// <param name="name">The name of IoC registration for the given requested type</param>
		/// <returns>The actual implementation class instance object to be return</returns>
		public virtual T Resolve<T>(string name) where T : class
		{
			var instance = (T)Resolve(typeof(T), name);
			return instance;
		}

		/// <summary>
		/// Get actual named implementation class instance of the requested type (interface) from container
		/// </summary>
		/// <param name="requestedType">Requested type. usually, a interface or base class</param>
		/// <param name="name">The name of IoC registration for the given requested type</param>
		/// <returns>The actual implementation class instance object to be return</returns>
		public abstract object Resolve(Type requestedType, string name);

		/// <summary>
		/// Get actual implementation class instance of the requested type (interface) from container
		/// </summary>
		/// <param name="requestedType">Requested type. usually, a interface or base class</param>
		/// <returns>The actual implementation class instance object to be return</returns>
		/// <remarks>
		/// If no any matched type found, it raised exception. The instance could be a singleton or transient object
		/// </remarks>
		public virtual object Resolve(Type requestedType)
		{
			return Resolve(requestedType, String.Empty);
		}

        #endregion

        #region RegisterType

        public virtual IIocContainer RegisterType(Type actualType, params NamedValue[] constructorParameters)
        {
            return RegisterType(actualType, null, string.Empty, constructorParameters);
        }

        public virtual IIocContainer RegisterType<T>(params NamedValue[] constructorParameters)
        {
            return RegisterType(typeof(T), constructorParameters);
        }


        public virtual IIocContainer RegisterType<IT, T>(params NamedValue[] constructorParameters)
			where IT : class
			where T : class, IT
		{
			return RegisterType(typeof(IT), typeof(T), string.Empty, constructorParameters);
		}

		public virtual IIocContainer RegisterType<IT, T>(string name, params NamedValue[] constructorParameters)
			where IT : class
			where T : class, IT
		{
			return RegisterType(typeof(IT), typeof(T), name, constructorParameters);
		}

		public virtual IIocContainer RegisterType<IT, T>(IocInstanceScope instanceScopeOption, params NamedValue[] constructorParameters)
			where IT : class
			where T : class, IT
		{
			return RegisterType(typeof(IT), typeof(T), string.Empty, instanceScopeOption, constructorParameters);
		}

		public virtual IIocContainer RegisterType<IT, T>(string name, IocInstanceScope instanceScopeOption, params NamedValue[] constructorParameters)
			where IT : class
			where T : class, IT
		{
			return RegisterType(typeof(IT), typeof(T), name, instanceScopeOption, constructorParameters);
		}

		public virtual IIocContainer RegisterType<IT>(Type actualType, params NamedValue[] constructorParameters)
			where IT : class
		{
			return RegisterType(typeof(IT), actualType, string.Empty, constructorParameters);
		}

		public virtual IIocContainer RegisterType<IT>(Type actualType, string name, params NamedValue[] constructorParameters)
			where IT : class
		{
			return RegisterType(typeof(IT), actualType, name, constructorParameters);
		}

		public virtual IIocContainer RegisterType<IT>(Type actualType, IocInstanceScope instanceScopeOption, params NamedValue[] constructorParameters)
			where IT : class
		{
			return RegisterType(typeof(IT), actualType, string.Empty, instanceScopeOption, constructorParameters);
		}

		public virtual IIocContainer RegisterType<IT>(Type actualType, string name, IocInstanceScope instanceScopeOption, params NamedValue[] constructorParameters)
			where IT : class
		{
			return RegisterType(typeof(IT), actualType, name, instanceScopeOption, constructorParameters);
		}

		public virtual IIocContainer RegisterType(Type requestedType, Type actualType, params NamedValue[] constructorParameters)
		{
			return RegisterType(requestedType, actualType, string.Empty, constructorParameters);
		}

		public virtual IIocContainer RegisterType(Type requestedType, Type actualType, string name, params NamedValue[] constructorParameters)
		{
			return RegisterType(requestedType, actualType, name, DefaultInstanceScope, constructorParameters);
		}

		public virtual IIocContainer RegisterType(Type requestedType, Type actualType, IocInstanceScope instanceScopeOption, params NamedValue[] constructorParameters)
		{
			return RegisterType(requestedType, actualType, string.Empty, instanceScopeOption, constructorParameters);
		}

		/// <summary>
		/// This method must be implemented in each IoC IIoCContainer provider class
		/// </summary>
		/// <param name="requestedType">requested type</param>
		/// <param name="actualType">actual type of instance to be created later</param>
		/// <param name="name">A name associate to particular registration. null for default registration</param>
		/// <param name="instanceScopeOption">Option to manage the life time of instance creating.</param>
		/// <returns>Current container instance</returns>
		public virtual IIocContainer RegisterType(Type requestedType, Type actualType, string name, IocInstanceScope instanceScopeOption, params NamedValue[] constructorParameters)
		{
			throw new NotImplementedException();
		}

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
		public virtual IIocContainer RegisterInstance<IT>(IT instance)
			where IT : class
		{
			return RegisterInstance(typeof(IT), instance);
		}

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
		public virtual IIocContainer RegisterInstance<IT>(IT instance, string name)
			where IT : class
		{
			return RegisterInstance(typeof(IT), instance, name);
		}

		/// <summary>
		/// Register a named requested type (interface) mapping to an actual implementation class instance.
		/// </summary>
		/// <param name="requestedType">Requested type</param>
		/// <param name="instance">Object to be returned</param>
		/// <returns>Current container</returns>
		/// <remarks>
		/// The same instance will be returned regardless how the lifetime option choose.
		/// The instance dispose way may different for each IoC implemention. Some container may have a reference 
		/// to the instance, some may not.
		/// </remarks>
		public virtual IIocContainer RegisterInstance(Type requestedType, object instance)
		{
			return RegisterInstance(requestedType, instance, String.Empty);
		}

		/// <summary>
		/// Register a named requested type mapping to a particular instance.
		/// </summary>
		/// <param name="requestedType">Requested type</param>
		/// <param name="instance">Object to be returned</param>
		/// <param name="name">A name associate to a particular registration</param>
		/// <returns>Current container</returns>
		public virtual IIocContainer RegisterInstance(Type requestedType, object instance, string name)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region RegisterModule

		public virtual IIocContainer RegisterModule(object module)
		{
			try
			{
				var registerModule = module as IIocModule;
				registerModule.Load(this);
			}
			catch (Exception ex)
			{
				throw new IoCException(Resources.IoCExceptionRegisterModuleMessage, ex);
			}
			return this;
		}

		public virtual IIocContainer RegisterModule(Assembly assembly)
		{
			throw new NotImplementedException();
		}

		#endregion


		#endregion

		#region IDisposable Members

		public virtual void Dispose()
		{
		}

		#endregion

		#region Convert Life time scope
		protected IocInstanceScope GetLifetimeScope(string instanceScopeOption)
		{
			IocInstanceScope result;

			switch (instanceScopeOption)
			{
				case "Singleton":
					result = IocInstanceScope.Singleton;
					break;
				case "Transient":
				case "":
					result = IocInstanceScope.Transient;
					break;
				default:
					throw new NotImplementedException();
			}
			return result;
		}
		#endregion

		#region Utilities for Reflection
		/// <summary>
		/// Load a class type from a qualified type name or assembly
		/// </summary>
		/// <param name="typeName">Qualified type name. Such as "class_name_space, assembly"</param>
		/// <param name="assembly">Assembly contains specified type.</param>
		/// <returns>Type of class</returns>
		protected Type LoadType(string typeName, Assembly assembly)
		{
			Type type = Type.GetType(typeName);
			if (type == null && assembly != null)
			{
				type = assembly.GetType(typeName, false);
			}

			if (type == null)
			{
				throw new IoCException(
					string.Format(CultureInfo.CurrentCulture,
					Resource.TypeNotFound, typeName));
			}
			return type;
		}

		/// <summary>
		/// Convert a value to a specific type object
		/// </summary>
		/// <param name="value">The value to be convert</param>
		/// <param name="requestedType">The result type of object </param>
		/// <returns>converted object </returns>
		protected object ResolveTypedValue(object value, Type requestedType)
		{
			if (value == null)
			{
				if (requestedType.IsValueType)
				{
					return Activator.CreateInstance(requestedType);
				}
				return null;
			}
			if (requestedType.IsAssignableFrom(value.GetType()))
			{
				return value;
			}

			return TypeDescriptor.GetConverter(requestedType).ConvertFrom(value);
		}

		/// <summary>
		/// Get constructor parameter type by parameter name
		/// </summary>
		/// <param name="classType">type of class which has constructor parameter to be resolve</param>
		/// <param name="parameterName">Constructor parameter name</param>
		/// <returns>type of constructor parameter</returns>
		protected Type GetConstructorParameterType(Type classType, string parameterName)
		{
			var parmInfo = classType.GetConstructors()
									.Where(x => x.GetParameters()
									.Count(param => param.Name == parameterName) > 0)
									.FirstOrDefault();

			if (parmInfo != null)
			{
				var paramType = (from param in parmInfo.GetParameters()
								 where param.Name == parameterName
								 select param.ParameterType
								).FirstOrDefault();
				if (paramType != null)
				{
					return paramType;
				}
			}

			return null;
		}
		#endregion

		#region ProviderName
		/// <summary>
		/// Gets or sets the name of the provider.
		/// </summary>
		/// <value>The name of the provider.</value>
		public virtual string ProviderName
		{
			get;
			protected set;
		}
		#endregion

		#region Bind/Unbind disposable IoC objects created within same thread
		/// <summary>
		/// Bind the life time scope to the context.
		/// </summary>
		public abstract void Bind();
		/// <summary>
		/// Unbind the container
		/// </summary>
		public abstract void Unbind();
		#endregion
	}
}
