using System;
using System.Linq;
using System.Reflection;

namespace qshine
{
	public static class ObjectExtension
	{
		#region 1.Using()
		/// <summary>
		/// Provides a convenient way to dispose objects after execute the object action.
		/// It only dispose the object which implemented IDisposable
		/// </summary>
		/// <param name="instance">Instance.</param>
		/// <param name="action">Action.</param>
		public static void Using(this Object instance, Action action)
		{
			var disposableObject = instance as IDisposable;
			if (disposableObject != null)
			{
				using (disposableObject)
				{
					action();
				}
			}
			else
			{
				action();
			}
		}

		/// <summary>
		/// Provides a convenient way to dispose objects after execute the object method.
		/// It only dispose the object which implemented IDisposable
		/// </summary>
		/// <returns>The value from the method.</returns>
		/// <param name="instance">Instance.</param>
		/// <param name="method">Method.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <example>
		/// 	instance.Using(()=>{
		/// 	});
		/// </example>
		public static T Using<T>(this Object instance, Func<T> method)
		{
			var disposableObject = instance as IDisposable;
			if (disposableObject != null)
			{
				using (disposableObject)
				{
					return method();
				}
			}
			return method();
		}
		#endregion

		#region 2.TryCall
		/**
		 * Call public static method by name:
		 * 		type.TryCall(out result, "methodName", arg1,arg2,..);
		 * 
         * Call public static void method by name:
		 * 		type.TryCall("methodName", arg1, arg2,..);
		 * 
         * Call public method by name:
		 * 		instance.TryCall(out result, "methodName", arg1, arg2,..);
		 * 
		 * Call public void method by name:
		 * 		instance.TryCall("methodName", arg1, arg2,..);
		 * 
         * Call public method by name and explicitly provide types:
		 * 		instance.TryCall(out result, Types[] argumentTypes, "methodName", arg1, arg2,..);
		 * 
         * Call public void method by name and explicitly provide types:
		 * 		instance.TryCall(Types[] argumentTypes, "methodName", arg1, arg2,..);
		 * 
         * Call public generic method by name and explicitly provide generic types and argument types:
		 * 		instance.TryCall(out result, Types[] genericTypes, Types[] argumentTypes, "methodName", arg1, arg2,..);
		 *
		**/

		/// <summary>
		/// Tries to call a public static method by method name.
		/// </summary>
		/// <param name="type">Type of class which contains specific static method</param>
		/// <returns><c>true</c>, if call was tryed, <c>false</c> method not found or wrong argument type or numbers.</returns>
		/// <param name="result">output the method return value</param>
		/// <param name="methodName">static method name</param>
		/// <param name="args">The method arguments</param>
		/// <typeparam name="T">The type of the method return value.</typeparam>
		/// <example>
		/// 	int result;
		/// 	var hasMethod = type.TryCall(out result,"StaticMethod", 1,2);
		/// </example>
		public static bool TryCall<T>(this Type type, out T result, string methodName, params object[] args)
		{
			MethodInfo method=null;;
			if (args != null)
			{
				method = type.GetMethod(methodName, args.Select(x => x.GetType()).ToArray());
			}
			else
			{
				method = type.GetMethod(methodName);
			}

			if (method != null && method.IsStatic)
			{
				result= (T)InvokeMethod(method, null, ToParametersArrary(args));
				return true;
			}
			result = default(T);
			return false;
		}

		/// <summary>
		/// Tries to call a public static void method by method name.
		/// </summary>
		/// <returns><c>true</c>, if call was tryed, <c>false</c> method not found or wrong argument type or numbers.</returns>
		/// <param name="type">Type of class which contains specific static method</param>
		/// <param name="methodName">static method name.</param>
		/// <param name="args">The method arguments</param>
		public static bool TryCall(this Type type, string methodName, params object[] args)
		{
			MethodInfo method = null; ;
			if (args != null)
			{
				method = type.GetMethod(methodName, args.Select(x => x.GetType()).ToArray());
			}
			else
			{
				method = type.GetMethod(methodName);
			}

			if (method != null && method.IsStatic)
			{
				InvokeMethod(method, null, ToParametersArrary(args));
				return true;
			}
			return false;
		}

		/// <summary>
		/// Tries to call a public method by the method name.
		/// </summary>
		/// <param name="instance">a class instance.</param>
		/// <returns><c>true</c>, if call was tryed, <c>false</c> method not found or wrong argument type or numbers.</returns>
		/// <param name="result">output the method return value</param>
		/// <param name="methodName">The method name.</param>
		/// <param name="args">The method arguments</param>
		/// <typeparam name="T">The type of the method return value.</typeparam>
		/// <example>
		/// 	int result;
		/// 	var hasMethod = instance.TryCall(out result, "MethodName", 1,"2");
		/// </example>
		public static bool TryCall<T>(this object instance, out T result, string methodName, params object[] args)
		{
			return instance.TryCall(out result, null, methodName, args);
		}

		/// <summary>
		/// Tries to call a public void method by the method name.
		/// </summary>
		/// <returns><c>true</c>, if call was tryed, <c>false</c> otherwise.</returns>
		/// <param name="instance">Instance.</param>
		/// <param name="methodName">Method name.</param>
		/// <param name="args">Arguments.</param>
		public static bool TryCall(this object instance, string methodName, params object[] args)
		{
			return instance.TryCall(null, methodName, args);
		}

		/// <summary>
		/// Tries to call a public void method by the method name and provide method arguments types explicitly.
		/// </summary>
		/// <returns><c>true</c>, if call was tryed, <c>false</c> otherwise.</returns>
		/// <param name="instance">Instance.</param>
		/// <param name="argumentTypes">Types.</param>
		/// <param name="methodName">Method name.</param>
		/// <param name="args">Arguments.</param>
		public static bool TryCall(this object instance, Type[] argumentTypes, string methodName, params object[] args)
		{
			object result;
			return instance.TryCall(out result, argumentTypes, methodName, args);
		}

		/// <summary>
		/// Tries to call a public method by the method name and provide method arguments types explicitly.
		/// </summary>
		/// <returns><c>true</c>, if call was tryed, <c>false</c> method not found or wrong argument type or numbers.</returns>
		/// <param name="instance">Instance.</param>
		/// <param name="result">output the method return value</param>
		/// <param name="argumentTypes">Types of method arguments.</param>
		/// <param name="methodName">The method name.</param>
		/// <param name="args">The method arguments</param>
		/// <typeparam name="T">The type of the method return value.</typeparam>
		public static bool TryCall<T>(this object instance, out T result, Type[] argumentTypes, string methodName, params object[] args)
		{
			result = default(T);
			MethodInfo method = null;
			if (argumentTypes == null && args!=null)
			{
				argumentTypes = args.Select(x => x.GetType()).ToArray();
			}else if (argumentTypes== null)
			{
				argumentTypes = new Type[] { };
			}
			method = instance.GetType().GetMethod(methodName,argumentTypes);
			if (method != null && method.IsPublic)
			{
				result =(T)InvokeMethod(method, instance, ToParametersArrary(args));
				return true;
			}
			return false;
		}

        /// <summary>
        /// Call non-public method using reflection.
        /// It is available internal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="result"></param>
        /// <param name="argumentTypes"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static bool TryCallNonPublic<T>(this object instance, out T result, Type[] argumentTypes, string methodName, params object[] args)
        {
            result = default(T);
            MethodInfo method = null;
            if (argumentTypes == null && args != null)
            {
                argumentTypes = args.Select(x => x.GetType()).ToArray();
            }
            else if (argumentTypes == null)
            {
                argumentTypes = new Type[] { };
            }
            method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic, Type.DefaultBinder, argumentTypes, null);
            if (method != null)
            {
                result = (T)InvokeMethod(method, instance, ToParametersArrary(args));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to call a public generic method by method name and provide method arguments types explicitly.
        /// </summary>
        /// <returns>The generic if any.</returns>
        /// <param name="instance">Instance.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="genericTypes">Generic type arguments of the method.</param>
        /// <param name="argumentTypes">Method argument types.</param>
        /// <param name="args">Method arguments.</param>
        /// <example>
        /// 	Assume class SampleClass has following method signature
        /// 		public class SampleClass {
        /// 			int GetTicket<T1,T2,T3> (int arg1, T2 arg2, T3 arg3){...}
        /// 		}
        /// 	int result;
        /// 	var hasMethod = instance.TryCall<int>(out result, new [] {typeof(T1), typeof (T2), typeof(T3)}, new [] {typeof(int)}, "GetTicket", arg1);
        /// </example>
        /// <remarks>
        /// 	This method is not accurate to determine the method match. If more than two methods found, it will through exception.
        /// </remarks>
        public static bool TryCall<T>(this object instance, out T result, Type[] genericTypes,  Type[] argumentTypes, string methodName, params object[] args)
		{
			result = default(T);
			var parameterCount = args == null ? 0 : args.Length;
			var genericArgCount = genericTypes == null ? 0 : genericTypes.Length;

			if (argumentTypes == null && args != null)
			{
				argumentTypes = args.Select(x => x.GetType()).ToArray();
			}
			if (argumentTypes == null)
			{
				argumentTypes = new Type[] { };
			}

			var methods = 
				instance.GetType().GetMethods().Where(
					m => m.Name == methodName &&
					m.IsPublic &&
					m.IsGenericMethodDefinition 
					&& m.GetParameters().Length == parameterCount
					&& m.GetGenericArguments().Length == genericArgCount
					//&& m.GetParameters().Select(x => x.GetType()).ToArray().SequenceEqual(argumentTypes)
					//&& m.GetGenericArguments().SequenceEqual(genericTypes)
				);
			
			if(methods!=null)
			{
				foreach (var methodInfo in methods)
				{
					var method = methodInfo.MakeGenericMethod(genericTypes);
					if (method != null)
					{
						if (method.GetParameters().Select(x => x.ParameterType).ToArray().SequenceEqual(argumentTypes))
						{
							result = (T)InvokeMethod(method, instance, ToParametersArrary(args));
							return true;
						}
					}
				}
			}
			return false;
		}


		/// <summary>
		/// Force to convert parameter array to object array
		/// </summary>
		/// <param name="args">Parameter array</param>
		/// <returns>An object array</returns>
		static object[] ToParametersArrary(params object[] args)
		{
			if (args == null)
			{
				return new object[] { null };
			}
			object[] methodArgs = null;
			if (args.Length > 0)
			{
				methodArgs = new object[args.Length];
				for (var i = 0; i < args.Length; i++)
				{
					methodArgs[i] = args[i];
				}
			}
			return methodArgs;
		}

		/// <summary>
		/// Invokes the method by method info.
		/// </summary>
		/// <returns>The method.</returns>
		/// <param name="method">Method.</param>
		/// <param name="instance">Instance.</param>
		/// <param name="parms">Parms.</param>
		static object InvokeMethod(MethodInfo method, object instance, object[] parms)
		{
			try
			{
				return method.Invoke(instance, parms);
			}
			catch (TargetInvocationException ex)
			{
				//throw actual exception
				var actualException = ex.InnerException;
				while (actualException is TargetInvocationException)
				{
					actualException = actualException.InnerException;
				}
				throw actualException;
			}
		}
		#endregion

		/// <summary>
		/// Get type arguments from an open generic type.
		/// IOpenGenericType[TA1, TA2]
		/// </summary>
		/// <param name="implementationType">The type that implemented an open generic type</param>
		/// <param name="openGenericType">Open generic type</param>
		/// <returns></returns>
		public static Type[] GetOpenGenericTypes(this Type implementationType, Type openGenericType)
		{
			foreach (var type in implementationType.GetInterfaces())
			{
				if (type.IsGenericType && type.GetGenericTypeDefinition() == openGenericType)
				{
					return type.GetGenericArguments();
				}
			}
			return null;
		}

		/// <summary>
		/// Compare nullable string
		/// </summary>
		/// <returns><c>true</c>, if equal was ared, <c>false</c> otherwise.</returns>
		/// <param name="a">The a string.</param>
		/// <param name="b">The b string.</param>
		public static bool AreEqual(this string a, string b)
		{
			if (string.IsNullOrEmpty(a))
			{
				return string.IsNullOrEmpty(b);
			}

			return string.Equals(a, b);
		}
		/// <summary>
		/// Are object and string component equal.
		/// </summary>
		/// <returns><c>true</c>, if equal, <c>false</c> otherwise.</returns>
		/// <param name="a">The a component.</param>
		/// <param name="b">The b component.</param>
		public static bool AreEqual(this object a, string b)
		{
			if (a == null)
			{
				return string.IsNullOrEmpty(b);
			}

			return string.Equals(a.ToString(), b);
		}
	}
}
