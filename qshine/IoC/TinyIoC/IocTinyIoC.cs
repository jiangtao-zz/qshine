using System;
using System.Collections.Generic;
using qshine.Configuration;
using qshine.Globalization;
using qshine;
using TinyIoC;


namespace qshine
{
	public class TinyIocProvider: IIocProvider
	{
		public IIocContainer CreateContainer()
		{
			return new IocTinyIoc();
		}
	}

    /// <summary>
    /// Implementation of IContainer for TinyIoC component.
    /// Native interface:
    ///     public TinyIoCContainer TinyContainer
    /// </summary>
	public class IocTinyIoc : IocContainerBase
    {
        #region internal class
        internal sealed class NamedIoCType
        {
            public Type RegisteredType;
            public string Name = String.Empty;
            public NamedIoCType(Type type, string name)
            {
                RegisteredType = type;
                Name = name;
            }
            public override bool Equals(object obj)
            {
                NamedIoCType compareTo = obj as NamedIoCType;
                if (compareTo != null)
                {
                    return Name == compareTo.Name && RegisteredType == compareTo.RegisteredType;
                }
                return false;
            }

            public override int GetHashCode()
            {
                if(String.IsNullOrEmpty(Name)){
                    return RegisteredType.FullName.GetHashCode();
                }else{
                    return (RegisteredType.FullName+Name).GetHashCode();
                }
            }
        }
        #endregion

        #region Fields

        TinyIoCContainer container;
        //The TinyIoC doesn't implement the registration of named parameters, 
        //so we have to keep the constructor named parameters here
        Dictionary<NamedIoCType, NamedValue[]> typeWithCtorParameters = new Dictionary<NamedIoCType, NamedValue[]>(); 

        #endregion

        #region constructor

        public IocTinyIoc()
        {
            this.ProviderName = "TinyIoC";
            container = new TinyIoCContainer();
        }

        #endregion

        #region Resolve

        public override object Resolve(Type interfaceType, string name)
        {
            NamedParameterOverloads overloadParameter = null;
            if (name == null)
            {
                name = String.Empty;
            }
            if (typeWithCtorParameters.Count > 0)
            {
                NamedIoCType namedIoCType = new NamedIoCType(interfaceType, name);

                if (typeWithCtorParameters.ContainsKey(namedIoCType))
                {
                    Dictionary<string, object> overloads = new Dictionary<string, object>();
                    foreach (var parm in typeWithCtorParameters[namedIoCType])
                    {
                        overloads.Add(parm.Name, parm.Value);
                    }
                    overloadParameter = new NamedParameterOverloads(overloads);
                }
            }

            if (string.IsNullOrEmpty(name))
            {
                //container.Resolve<>();
                try
                {
                    if (overloadParameter!=null && overloadParameter.Count > 0)
                    {
                        return container.Resolve(interfaceType,overloadParameter);
                    }
                    return container.Resolve(interfaceType);
                }
                catch (Exception ex)
                {
                    throw new IoCException(
                        string.Format(Resource.IoCExceptionResolveMessage, interfaceType),
                        ex);
                }
            }
            else
            {
                try
                {
                    if (overloadParameter != null && overloadParameter.Count > 0)
                    {
                        return container.Resolve(interfaceType,name,overloadParameter);
                    }
                    return container.Resolve(interfaceType, name);
                }
                catch (Exception ex)
                {
                    throw new IoCException(
                        string.Format(Resource.IoCExceptionResolveNamedMessage, interfaceType, name),
                        ex);
                }
            }
        }


        #endregion

        #region RegisterType
		public override IIocContainer RegisterType(Type requestedType, Type actualType, string name, IocInstanceScope instanceScopeOption, params NamedValue[] constructorParameters)
        {
            if (name == null)
            {
                name = String.Empty;
            }

            try
            {
                if (constructorParameters.Length > 0 && instanceScopeOption == IocInstanceScope.Singleton)
                {
                    //The TinyIoC do not support singleton parameter constructor
                    //we need implement one for it.
                    List<Type> paramTypes = new List<Type>();
                    List<object> paramValues = new List<object>();

                    //ConstructorInfo constructor =
                    //    (from ctor in actualType.GetConstructors()
                    //         let parameters = ctor.GetParameters()
                    //         where parameters.Length>=constructorParameters.Length
                    //         && MatchParameters(parameters,constructorParameters)
                    //         select ctor ).SingleOrDefault<ConstructorInfo>();
                    foreach (var ctor in actualType.GetConstructors())
                    {
                        var parameters = ctor.GetParameters();
                        if (parameters.Length == constructorParameters.Length)
                        {
                            paramValues.Clear();
                            for (int index = 0; index < constructorParameters.Length; index++)
                            {
                                if (parameters[index].Name != constructorParameters[index].Name)
                                {
                                    break;
                                }
                                paramValues.Add(Convert.ChangeType(constructorParameters[index].Value, parameters[index].ParameterType));
                            }
                            if (paramValues.Count == constructorParameters.Length)
                            {
                                container.Register(requestedType, ctor.Invoke(paramValues.ToArray()), name);
                                return this;
                            }
                        }
                    }
                    //throw exception if no match found
                    throw new IoCException(
                        string.Format(Resource.IoCExceptionRegisterNameMessage, requestedType, name));
                }
                else
                {
                    if (constructorParameters.Length > 0)
                    {
                        typeWithCtorParameters.Add(new NamedIoCType(requestedType, name), constructorParameters);
                    }
                    this.SetInstanceScope(container.Register(requestedType, actualType, name), instanceScopeOption);
                }
                return this;
            }
            catch (IoCException newEx)
            {
                throw newEx;
            }
            catch (Exception ex)
            {
                throw new IoCException(
                    string.Format(Resource.IoCExceptionRegisterNameMessage, requestedType, name),
                    ex);
            }
        }

        #endregion

        #region RegisterInstance
        public override IIocContainer RegisterInstance<IT>(IT instance)
        {
            container.Register<IT>(instance);

            return this;
        }

        public override IIocContainer RegisterInstance(Type requestedType, object instance, string name)
        {
            container.Register(requestedType, instance, name);

            return this;
        }
        #endregion

        #region Dispose

        public override void Dispose()
        {
            container.Dispose();
        }

        #endregion
        
        #region Private

        private void SetInstanceScope(TinyIoCContainer.RegisterOptions instanceScope, IocInstanceScope instanceScopeOption)
        {

            switch (instanceScopeOption)
            {
                case IocInstanceScope.Singleton:
                    instanceScope.AsSingleton();
                    break;
                case IocInstanceScope.Transient:
                    instanceScope.AsMultiInstance();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

		public override void Bind()
		{
		}

		public override void Unbind()
		{
		}

		//private object InvokeGenericRegisterInstance(Type requestedType, object instance, string name)
		//{
		//    if (string.IsNullOrEmpty(name))
		//    {
		//        return InvokeGenericMethod(container, "Register", new string[] { "RegisterType" },
		//            new Type[] { requestedType },
		//            new Type[] { instance.GetType()}, new object[] { instance});
		//    }
		//    else
		//    {
		//        return InvokeGenericMethod(container, "Register", new string[] { "RegisterType" },
		//            new Type[] { requestedType },
		//            new Type[] { instance.GetType(), typeof(string) }, new object[] { instance, name });
		//    }
		//}

		#endregion

		//#region Special private
		////This class in for TinyIoC only, 
		////it should be remove if TinyIoC has Register(Type,Type) method available
		//public object InvokeGenericMethod(object instance, string methodName, string[] genericTypeNames, Type[] genericTypes, Type[] parameterTypes, object[] parameters) 
		//{
		//    Type type = instance.GetType();

		//    MethodInfo invokeMethod = (from method in type.GetMethods()
		//                  where method.Name == methodName 
		//                        && method.IsGenericMethodDefinition
		//                  let methodParameters = method.GetParameters()
		//                  where methodParameters.Length == parameterTypes.Length
		//                        && MatchTypes(methodParameters, parameterTypes, genericTypeNames, genericTypes)
		//                  let genericArguments = method.GetGenericArguments()
		//                    where genericArguments.Length == genericTypeNames.Length
		//                        && MatchTypes(genericArguments, genericTypeNames)
		//                            select method).SingleOrDefault <MethodInfo>()
		//                  ;
		//    if (invokeMethod!=null)
		//    {
		//        return invokeMethod.MakeGenericMethod(genericTypes)
		//            .Invoke(instance, parameters);
		//    }
		//    else
		//    {
		//        throw new ArgumentNullException("method is not found.");
		//    }
		//}


		//private bool MatchTypes(Type[] genericTypes, string[] targetNames)
		//{
		//    for (int i = 0; i < genericTypes.Length; i++)
		//    {
		//        if (!genericTypes[i].Name.Equals(targetNames[i], StringComparison.InvariantCultureIgnoreCase))
		//        {
		//            return false;
		//        }
		//    }
		//    return true;
		//}

		//private bool MatchTypes(ParameterInfo[] source, Type[] target,string[] genericTypeNames, Type[] genericTypes)
		//{
		//    for (int i = 0; i < source.Length; i++)
		//    {
		//        if(source[i].ParameterType.IsGenericParameter)
		//        {
		//            int j;
		//            for (j = 0; j < genericTypeNames.Length; j++)
		//            {
		//                if (source[i].ParameterType.Name == genericTypeNames[j])
		//                {
		//                    if (!genericTypes[j].IsAssignableFrom(target[i]))
		//                    {
		//                        return false;
		//                    }
		//                    else
		//                    {
		//                        break;
		//                    }
		//                }
		//            }
		//            if (j == genericTypeNames.Length)
		//            {
		//                return false;
		//            }
		//        }else{
		//            if (!source[i].ParameterType.IsAssignableFrom(target[i]))
		//            {
		//                return false;
		//            }
		//        }
		//    }
		//    return true;
		//}
		//#endregion

		#region Expose native IoC component interface for special implementation
		/// <summary>
		/// Get TinyIoC container to resolve interface dependency at granularity level.
		/// </summary>
		/// <remarks>
		/// The Container property expose the TinyIoC container that gives user more control on the IoC container.
		/// In most cases, we should not use this property, instead, call Resolve() method to get the concrete class instance.
		/// </remarks>
		public TinyIoCContainer TinyContainer
        {
            get
            {
                return container;
            }
        }
        #endregion
    }

}
