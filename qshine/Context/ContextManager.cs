using qshine.Configuration;
using qshine.Utility;
using System;

namespace qshine
{
	/// <summary>
	/// The context is used to hold a set of data within a particular operation scope.
	/// The context data is accessable from different layers without pass a reference.
	/// The operation period lifecycle could be a request, thread, session or others which bound to one given execution path. 
	/// The well-known context has: HttpContext, OperationContext, CallContext.
	/// The static context is a global storage accross running application.
    /// 
	/// The context manager provider service to access current context store object based on initial setting. 
    /// It also allow access different context store by a given context type unique name 
	/// 
    ///     ContextManager.Current.SetData(key, data);
    ///     ContextManager.Current.GetData(key);
    ///     IContextStore contextStore = ContextManager.GetContextStore(ContextStoreType.Static);
    ///     contextStore.SetData(key,data)
	/// 
	/// 
	/// </summary>
	public class ContextManager
	{
        static SafeDictionary<ContextStoreType, Type> _registry = new SafeDictionary<ContextStoreType, Type>();
        static IContextStore _currentContextStore;
        static object lockObject = new object();
        static ContextManager()
        {
            _registry[ContextStoreType.Static] = typeof(StaticContextStore);
            _registry[ContextStoreType.ThreadLocal] = typeof(LocalContextStore);
            _registry[ContextStoreType.CallContext] = typeof(CallContextStore);
        }

        /// <summary>
        /// Get a specific type of context store
        /// </summary>
        /// <param name="storeType">type of context store</param>
        /// <returns></returns>
        public static IContextStore GetContextStore(ContextStoreType storeType)
        {
            Type type;
            if(_registry.TryGetValue(storeType, out type))
            {
                return Activator.CreateInstance(type) as IContextStore;
            }

            throw new NotImplementedException(string.Format("The context of ContextStoreType {0} is not implemented.", storeType));
        }

		/// <summary>
		/// Gets or sets the name of the context type.
		/// </summary>
		/// <value>The name of the context type.</value>
        /// <remarks>
        /// It should only one type of Current context exists in application execution path.
        /// Othere type context store can be found using GetContextStore()
        /// </remarks>
		public static IContextStore Current
		{
			get
			{
				if (_currentContextStore == null)
				{
					lock(lockObject)
					{
						if (_currentContextStore == null)
						{
							//Load from configure
							_currentContextStore = ApplicationEnvironment.Default.Services.GetProvider<IContextStore>();
							if (_currentContextStore == null)
							{
								//load default
								_currentContextStore = new CallContextStore();
                            }
                            else
                            {
                                _registry[_currentContextStore.ContextType] = _currentContextStore.GetType();
                            }
						}
					}
				}
				return _currentContextStore;
			}
			set{
				_currentContextStore=value;
			}
		}

		/// <summary>
		/// Sets the data.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="data">Data.</param>
		public static void SetData(string name, object data)
		{
			Current.SetData(name, data);
		}

		/// <summary>
		/// Gets the data.
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="name">Name.</param>
		public static object GetData(string name)
		{
			return Current.GetData(name);
		}

		/// <summary>
		/// Frees the data.
		/// </summary>
		/// <param name="name">Name.</param>
		public static void FreeData(string name)
		{
			Current.FreeData(name);
		}

        /// <summary>
        /// Get call context
        /// </summary>
        public static IContextStore CallContext
        {
            get
            {
                return GetContextStore(ContextStoreType.CallContext);
            }
        }

        /// <summary>
        /// Get Static context
        /// </summary>
        public static IContextStore StaticContext
        {
            get
            {
                return GetContextStore(ContextStoreType.Static);
            }
        }


        /// <summary>
        /// Get call context
        /// </summary>
        public static IContextStore ThreadContext
        {
            get
            {
                return GetContextStore(ContextStoreType.ThreadLocal);
            }
        }


    }
}
