using System;
using System.Collections.Generic;

using Autofac;
using Autofac.Builder;

using qshine.Configuration;

using qshine.ioc.autofac.Properties;

namespace qshine.ioc.autofac
{
    /// <summary>
    /// Implementation of IContainer for Autofac IoC component.
    /// Native interface:
    ///     public Autofac.ContainerBuilder AutofacBuilder
    ///     public Autofac.IContainer AutofacContainer
    /// </summary>
    public class Container : IocContainerBase
    {
        #region Fields

        private ContainerBuilder _builder;
        private IContainer _container;

        #endregion

        #region Constructor

        public Container()
        {
            _builder = new ContainerBuilder();
        }

        #endregion

        #region Methods

        #region Resolve
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
        public override object Resolve(Type requestedType, string name)
        {
            try
            {
				var scope = LifeTimeScope;
				if (scope == null)
				{
                    return string.IsNullOrEmpty(name)
                         ? AutofacContainer.Resolve(requestedType)
                         : AutofacContainer.ResolveNamed(name, requestedType);
				}
				return string.IsNullOrEmpty(name)
					 ? scope.Resolve(requestedType)
                     : scope.ResolveNamed(name, requestedType);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new IoCException(
                        string.Format(Resources.IoCExceptionResolveMessage, requestedType),
                        ex);
                }
                throw new IoCException(
                    string.Format(Resources.IoCExceptionResolveNamedMessage, requestedType, name),
                    ex);
            }
        }


        #endregion

        #region RegisterType

		public override IIocContainer RegisterType(Type requestedType, Type actualType, string name, IocInstanceScope instanceScopeOption, params NamedValue[] constructorParameters)
        {
            try
            {
                if (actualType == null)
                {
                    actualType = requestedType;
                }

                if (requestedType.IsGenericTypeDefinition)
                {
                    IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> dynamicInstanceScope;
                    dynamicInstanceScope = _builder.RegisterGeneric(actualType);
                    if (requestedType != actualType)
                    {

                        if (String.IsNullOrEmpty(name))
                        {
                            dynamicInstanceScope = dynamicInstanceScope.As(requestedType);
                        }
                        else
                        {
                            dynamicInstanceScope = dynamicInstanceScope.Named(name, requestedType);
                        }
                    }
                    if(constructorParameters.Length>0){
                        dynamicInstanceScope = dynamicInstanceScope.WithParameters(AutofacNamedParameters(constructorParameters));
                    }
                    SetInstanceScope(dynamicInstanceScope, instanceScopeOption);
                }
                else
                {
                    IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> instanceScope;
                    instanceScope = _builder.RegisterType(actualType);
                    if (requestedType != actualType)
                    {
                        if (String.IsNullOrEmpty(name))
                        {
                            instanceScope = instanceScope.As(requestedType);
                        }
                        else
                        {
                            instanceScope = instanceScope.Named(name, requestedType);
                        }
                    }
                    if (constructorParameters.Length > 0)
                    {
                        instanceScope = instanceScope.WithParameters(AutofacNamedParameters(constructorParameters));
                    }
                    SetInstanceScope(instanceScope, instanceScopeOption);
                }
                return this;
            }
            catch (Exception ex)
            {
                if (String.IsNullOrEmpty(name))
                {
                    throw new IoCException(
                    string.Format(Resources.IoCExceptionRegisterMessage, requestedType),
                    ex);
                }
                else
                {
                    throw new IoCException(
                        string.Format(Resources.IoCExceptionRegisterNameMessage, requestedType, name),
                        ex);
                }
            }
        }

        #endregion

        #region RegisterInstance
        public override IIocContainer RegisterInstance(Type requestedType, object instance, string name)
        {
            try
            {

                IRegistrationBuilder<object,SimpleActivatorData, SingleRegistrationStyle> instanceScope;
                if (String.IsNullOrEmpty(name))
                {
                    instanceScope = _builder.RegisterInstance(instance).As(requestedType);
                }
                else
                {
                    instanceScope = _builder.RegisterInstance(instance).Named(name, requestedType);
                }
                return this;
            }
            catch (Exception ex)
            {
                if (String.IsNullOrEmpty(name))
                {
                    throw new IoCException(
                    string.Format(Resources.IoCExceptionRegisterMessage, requestedType),
                    ex);
                }
                else
                {
                    throw new IoCException(
                        string.Format(Resources.IoCExceptionRegisterNameMessage, requestedType, name),
                        ex);
                }
            }
        }
		#endregion


		#region Context Bind/Unbind
		/// <summary>
		/// Bind this instance.
		/// </summary>
		public override void Bind()
		{
			Unbind();

			ContextManager.SetData(LifeTimeScopeName, AutofacContainer.BeginLifetimeScope());
		}

		/// <summary>
		/// Unbind the container from the context
		/// </summary>
		public override void Unbind()
		{
			var value = ContextManager.GetData(LifeTimeScopeName) as ILifetimeScope;
			if (value != null)
			{
				value.Dispose();
				//context value already exists, release it
				ContextManager.FreeData(LifeTimeScopeName);
			}
		}
		#endregion

		#region Dispose

		public override void Dispose()
        {
            if (_container != null)
            {
                _container.Dispose();
            }
        }

        #endregion

        #endregion

        #region Expose native IoC component interface for special implementation

        public ContainerBuilder AutofacBuilder
        {
            get
            {
                return this._builder;
            }
        }

        /// <summary>
        /// Get Autofac IoC container to resolve interface dependency at granularity level.
        /// </summary>
        /// <remarks>
        /// The Container property expose the Autofac container that gives user more control on the IoC container.
        /// In most cases, we should not use this property, instead, call Resolve() method to get the concrete class instance.
        /// </remarks>
        public IContainer AutofacContainer
        {
            get
            {
                if (_container == null)
                {
                    //Build top container
                    _container = _builder.Build();
                }
                return _container;
            }
        }

        #endregion

        #region Private

        private void SetInstanceScope<T, V>(IRegistrationBuilder<object, T, V> instanceScope, IocInstanceScope instanceScopeOption)
        {
            switch (instanceScopeOption)
            {
                case IocInstanceScope.Singleton:
                    instanceScope.SingleInstance();
                    break;
                case IocInstanceScope.Transient:
                    instanceScope.InstancePerDependency();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        private NamedParameter[] AutofacNamedParameters(NamedValue[] constructorParameters)
        {
            List<NamedParameter> namedParameters = new List<NamedParameter>();
            foreach (var x in constructorParameters)
            {
                namedParameters.Add(new NamedParameter(x.Name,x.Value));
            }
            return namedParameters.ToArray();
        }

		ILifetimeScope LifeTimeScope
		{
			get
			{
				return ContextManager.GetData(LifeTimeScopeName) as ILifetimeScope;
			}
		}

		string _lifetimescopeName;
		string LifeTimeScopeName
		{
			get
			{
				if (string.IsNullOrEmpty(_lifetimescopeName))
				{
					_lifetimescopeName = "autofac_" + this.GetHashCode();
				}
				return _lifetimescopeName;
			}
		}
		#endregion
	}
}


