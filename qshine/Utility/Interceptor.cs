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

        #region Instance
        /// <summary>
        /// Get an interceptor instance of a class.
        /// Each class only has one interceptor instance.
        /// 
        /// Use interceptor instance to capture class method execution stage events (enter/exit/exception/complete).
        /// </summary>
        /// <returns>interceptor instance</returns>
        /// <param name="type">The type of class which have methods to be intercepted</param>
        public static Interceptor Get(Type type)
		{
            lock (_lockObject)
            {
                Interceptor interceptor = null;
                if (_typeRegistry.ContainsKey(type))
                {
                    interceptor = _typeRegistry[type];
                }
                else
                {
                    interceptor = new Interceptor(type);
                }
                return interceptor;
            }
        }

        /// <summary>
        /// Get an interceptor instance of a class.
        /// </summary>
        /// <typeparam name="T">type of a class</typeparam>
        /// <returns>interceptor instance</returns>
        public static Interceptor Get<T>()
        {
            return Get(typeof(T));
        }

        #endregion

        #region Register interceptor handler
        /// <summary>
        /// Registers interceptor handler which implemented IInterceptorHandler.
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
        #endregion

        #region Private
        static readonly object _lockObject = new object();
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

        #endregion

        #endregion

        /// <summary>
        /// Event handler for enter into the function
        /// </summary>
        public event EventHandler<InterceptorEventArgs> OnEnter;
        /// <summary>
        /// Event handler for the function completed without error exception
        /// </summary>
        public event EventHandler<InterceptorEventArgs> OnSuccess;
        /// <summary>
        /// Event handler for the function throw exception
        /// </summary>
        public event EventHandler<InterceptorEventArgs> OnException;
        /// <summary>
        /// Event handler for leave the function
        /// </summary>
        public event EventHandler<InterceptorEventArgs> OnExit;
        /// <summary>
        /// Event handler for iterate over each element in the loops
        /// </summary>
        public event EventHandler OnForEach;

        readonly Type _interceptClassType;

        private Interceptor(Type intereptClassType)
        {
            _interceptClassType = intereptClassType;
            //register all interceptor handlers associated to inspectClass.
            if (!_typeRegistry.ContainsKey(intereptClassType))
            {
                _typeRegistry.Add(intereptClassType, this);

            }
            else
            {
                throw new InvalidOperationException(
                    "The class interceptor must be a static instance. It only can be one instance per class."._G());
            }
        }

        /// <summary>
        /// Ctor::
        /// </summary>
        public Interceptor()
		{
		}

        /// <summary>
        /// Raise an OnEnter event for entering the method.
        /// 
        /// You can manually call RaiseOnEnterEvent() in the begin of the method to set a join point.
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
        /// Raise an OnSuccess event when a method completed successfully without exception.
        /// 
        /// You can manually call RaiseOnSuccessEvent() in end of the method to set a join point.
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
        /// Raise an OnException event when the method throw an exception.
        /// 
        /// You can manually call RaiseOnExceptionEvent() in the exception catch block to set a join point.
        /// 
        /// The event handler can pass a flag back to stop raise exception.
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
        /// Raise an OnExit event when the method exit.
        /// 
        /// You can manually call RaiseOnExitEvent() in the end of the method or end of exception to set a join point.
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
        /// Method intercept. It add joinpoint in method enrty/exit, execution and exception handler.
        /// Use OnXXX events to inject "advice procedure" into joinpoint to perform additional function.
        /// </summary>
        /// <returns>The return value from the method. The "advice procedure may modify the return value.</returns>
        /// <param name="method">The base code method which has a set of join points.</param>
        /// <param name="sender">Sender. It is usually is the method class instance</param>
        /// <param name="methodName">The method name.</param>
        /// <param name="args">The method argument.</param>
        /// <typeparam name="T">The return value type. For void method(), you can return any value.</typeparam>
        public T JoinPoint<T>(Func<T> method, object sender, string methodName, params object[] args)
		{
			T result = default(T);
            var eventArgs = new InterceptorEventArgs<T>(method, methodName, args)
            {
                Result = result
            };

            //Pointcut when method entry
            if (RaiseOnEnterEvent(sender, eventArgs))
            {
                //Stop execution if advice procedure request for cancellation.
                return (T)eventArgs.Result;
            }

            try
			{
				result = method();

                eventArgs.Result = result;
                //Pointcut when method completed
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
                //Pointcut when exception occurred
                if (RaiseOnExceptionEvent(sender, eventArgs))
                {
                    //Stop execution if advice procedure request for suppress exception thrown
                    return result;
                }

				throw actualEx;
			}
			finally
			{
                //Pointcut when method exit.
                RaiseOnExitEvent(sender, eventArgs);
			}
			return result;
		}

        /// <summary>
        /// Inspect each iterated element.
        /// </summary>
        /// <typeparam name="T">type of the element</typeparam>
        /// <param name="element">element instance</param>
        /// <example>
        /// <![CDATA[
        /// foreach(var e in list){
        ///     inspector.ForEach(e);
        ///     base code here...
        /// }
        /// ]]>
        /// </example>
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
        /// <param name="methodName">method name</param>
        /// <param name="args">method argus</param>
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
        /// <summary>
        /// Interceptor arguments
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
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
        /// <summary>
        /// Register interceptor handler
        /// </summary>
		void LoadInterceptorHandler();
	}
}
