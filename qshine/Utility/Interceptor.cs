using qshine.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace qshine
{
	/// <summary>
	/// Interceptor class used to intercept a method executing.
	/// It is a simple way for AOP.
	/// Join point: add join point around a method to intercept method information. The method call information (include method name, parameters and return value) will be pass to intercept handler.
	/// Like AOP Advice, the interceptor handler can implement crosscutting behavior for given join point.
	/// Not like AOP Point cut, the interceptor do not need define Point cut. The event handler can determine join point method by method name and arguments directly.
	/// Using generic T to monitor which class to be intercept.
	/// The first argument of event handler always be a target object which is the executing method object instance.
	/// The second argument of event handler (InterceptorEventArgs) will contain method name, arguments and return value. 
	/// No AOP proxy in this simple implementation. add join point in method directly to intercept method data.
	/// </summary>
	public class Interceptor
	{
        #region static
        /// <summary>
        /// The registry of all interceptors.
        /// One registry per type.
        /// </summary>
        static SafeDictionary<Type, Interceptor> _typeRegistry = new SafeDictionary<Type, Interceptor>();
        /// <summary>
        /// The registry of all interceptor event handlers.
        /// One handler deal with one particular interceptor event.
        /// The plugin handler instance will be created automatically by ApplicationEnvironment.Build.
        /// </summary>
		static SafeDictionary<Type, IInterceptorHandler> _handlerRegistry = new SafeDictionary<Type, IInterceptorHandler>();

		/// <summary>
		/// Register an interceptor for particular class type.
        /// Only one interceptor is allow for particular class type.
		/// </summary>
		/// <returns>registered interceptor</returns>
		/// <param name="type">The type of class method to be intercept</param>
		public static Interceptor Register(Type type)
		{
			Interceptor interceptor = null;
			if (Registry.ContainsKey(type))
			{
				interceptor = Registry[type];
			}
			else
			{
                interceptor = new Interceptor(type);
			}
			return interceptor;
		}

		/// <summary>
		/// Registers interceptor handler.
		/// </summary>
		/// <returns><c>true</c>, if handler was registered, <c>false</c> otherwise.</returns>
		/// <param name="interceptorHandlerType">Handler type to process registered interceptor"/> "/> .</param>
		public static void RegisterHandlerType(Type interceptorHandlerType)
		{
			if (!_handlerRegistry.ContainsKey(interceptorHandlerType))
			{
				var handler = Activator.CreateInstance(interceptorHandlerType) as IInterceptorHandler;
				_handlerRegistry.Add(interceptorHandlerType,handler);
				handler.LoadInterceptorHandler();
			}
		}

		/// <summary>
		/// Get registered type of interceptor.
		/// </summary>
		/// <returns>registered interceptor</returns>
		/// <typeparam name="T">The type of inspect class.</typeparam>
		public static Interceptor Get<T>()
		where T:class
		{
			var type = typeof(T);
			if (!Registry.ContainsKey(type))
			{
				Register(type);
			}
			return Registry[type];
		}

		public static SafeDictionary<Type, Interceptor> Registry
		{
			get
			{
				return _typeRegistry;
			}

			set
			{
				_typeRegistry = value;
			}
		}
        #endregion


        public event EventHandler<InterceptorEventArgs> OnEnter;
        public event EventHandler<InterceptorEventArgs> OnSuccess;
        public event EventHandler<InterceptorEventArgs> OnException;
        public event EventHandler<InterceptorEventArgs> OnExit;
        public event EventHandler OnForEach;

        Type _interceptClassType;

        public Interceptor()
		{
		}

		public Interceptor(Type intereptClassType)
		{
			_interceptClassType = intereptClassType;
			//register all interceptor handlers associated to inspectClass.
			if (!Registry.ContainsKey(intereptClassType))
			{
				Registry.Add(intereptClassType, this);

			}
			else
			{
				throw new InvalidOperationException("The class interceptor must be a static instance. It only can be one instance per class.");
			}
		}

        /// <summary>
        /// Raise a function OnEnter event.
        /// </summary>
        /// <param name="sender">Usually is the caller instance object</param>
        /// <param name="eventArgs">InterceptorEventArgs type argument.</param>
        /// <returns>Indicates a StopExecution flag from event argument has been set.</returns>
        public bool RaiseOnEnterEvent(object sender, InterceptorEventArgs eventArgs)
        {
            if (OnEnter != null)
            {
                eventArgs.EnterTime = DateTime.Now;
                OnEnter.Invoke(sender, eventArgs);
                if (eventArgs.StopExecution)
                {
                    //reset stop execution
                    eventArgs.StopExecution = false;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Raise OnSuccess Event for function completed successfully without exception.
        /// </summary>
        /// <param name="sender">Usually is the caller instance object</param>
        /// <param name="eventArgs">InterceptorEventArgs type argument from source.</param>
        /// <returns>Returns true if </returns>
        public bool RaiseOnSuccessEvent(object sender, InterceptorEventArgs eventArgs)
        {
            if (OnSuccess != null)
            {
                OnSuccess.Invoke(sender, eventArgs);
                if (eventArgs.StopExecution) return true;
            }
            return false;
        }

        /// <summary>
        /// Raise OnException Event and return a flag to indicate not thrown current Exception pass from eventArgs.
        /// </summary>
        /// <param name="sender">Usually is the caller instance object</param>
        /// <param name="eventArgs">InterceptorEventArgs type argument from source.</param>
        /// <returns></returns>
        public bool RaiseOnExceptionEvent(object sender, InterceptorEventArgs eventArgs)
        {
            if (OnException != null)
            {
                OnException.Invoke(sender, eventArgs);
                if (eventArgs.StopExecution)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Raise a function exit event
        /// </summary>
        /// <param name="sender">Usually is the caller instance object</param>
        /// <param name="eventArgs">InterceptorEventArgs type argument from source.</param>
        public void RaiseOnExitEvent(object sender, InterceptorEventArgs eventArgs)
        {
            if (OnExit != null)
            {
                eventArgs.ExitTime = DateTime.Now;
                OnExit.Invoke(sender, eventArgs);
            }
        }

        /// <summary>
        /// Interceptor of method execution.
        /// </summary>
        /// <returns>The method return value.</returns>
        /// <param name="method">Method.</param>
        /// <param name="sender">Sender. It is usually the method class instance</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="args">Arguments.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T JoinPoint<T>(Func<T> method, object sender, string methodName, params object[] args)
		{
			T result = default(T);
			var eventArgs = new InterceptorEventArgs<T>(method, methodName, args);
            eventArgs.Result = result;

            if (RaiseOnEnterEvent(sender, eventArgs))
            {
                //Stop execution
                return (T)eventArgs.Result;
            }

            try
			{
				result = method();

                eventArgs.Result = result;
                RaiseOnSuccessEvent(sender, eventArgs);
			}
			catch (Exception ex)
			{
				Exception actualEx = ex;
				if (ex is TargetInvocationException && ex.InnerException!=null)
				{
					actualEx = ex.InnerException;
				}

                eventArgs.Exception = actualEx;
                if (RaiseOnExceptionEvent(sender, eventArgs))
                {
                    //Stop execution
                    return result;
                }

				throw actualEx;
			}
			finally
			{
                RaiseOnExitEvent(sender, eventArgs);
			}
			return result;
		}

		public void ForEach<T>(T element)
		{
			if (OnForEach != null)
			{
				OnForEach.Invoke(element, EventArgs.Empty);
			}
		}
	}

    /// <summary>
    /// Interceptor event arguments for particular intercept class
    /// </summary>
    /// <typeparam name="T">Interceptor event for specific type of class</typeparam>

    public class InterceptorEventArgs<T> : InterceptorEventArgs
    {
        /// <summary>
        /// Ctor::
        /// </summary>
        /// <param name="internalMethod">Method is being called and captured by the interceptor</param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        public InterceptorEventArgs(Func<T> internalMethod, string methodName, params object[] args)
            :base(methodName,args)
        {
            InternalMethod = internalMethod;
        }

        /// <summary>
        /// Current calling method.
        /// </summary>
        public Func<T> InternalMethod { get; set; }
    }

    /// <summary>
    /// Interceptor event arguments
    /// </summary>
    public class InterceptorEventArgs : EventArgs
	{
		public InterceptorEventArgs(string methodName, params object[] args)
		{
			MethodName = methodName;
			Args = new List<object>(args);
			DataBag = new Dictionary<string, object>();
		}

        /// <summary>
        /// The time the method being enter
        /// </summary>
		public DateTime EnterTime { get; set; }
        /// <summary>
        /// The time the method being exit.
        /// </summary>
		public DateTime ExitTime { get; set; }
			
		/// <summary>
		/// Gets the name of the method.
		/// </summary>
		/// <value>The name of the method.</value>
		public string MethodName { get; private set; }

		/// <summary>
		/// Gets the method arguments.
		/// </summary>
		/// <value>The arguments.</value>
		public List<object> Args { get; private set; }

		/// <summary>
		/// Gets the exception from calling method.
		/// </summary>
		/// <value>The exception.</value>
		public Exception Exception { get; set; }

		/// <summary>
		/// Flag to indictae whether a method should be stop execution.
        /// In case of Exception event it can stop exception to be thrown.
		/// </summary>
		/// <value><c>true</c> if stop execution; otherwise, <c>false</c>.</value>
		public bool StopExecution { get; set; }

		/// <summary>
		/// Gets the return value from the method.
		/// </summary>
		/// <value>The returned value from the method.</value>
		public object Result { get; set; }

        /// <summary>
        /// A generic data bag could be used as communication data between each events.
        /// </summary>
		public Dictionary<string, object> DataBag { get; set;}
	}

	/// <summary>
	/// Interface of interceptor event handler for particular type of class.
	/// </summary>
	public interface IInterceptorHandler<T>:IInterceptorHandler
	where T:class
	{
	}

    /// <summary>
    /// Interface of interceptor event handler
    /// </summary>
	public interface IInterceptorHandler
	{
		void LoadInterceptorHandler();
	}
}
