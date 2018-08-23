using qshine.Configuration;

namespace qshine
{
	/// <summary>
	/// The context is used to hold a set of data within a particular operation.
	/// The context data is accessable from different layers without pass a reference.
	/// The operation period lifecycle could be a request, thread, session or others which bound to one given execution path.
	/// The well-known context has: HttpContext, OperationContext, CallContext.
	/// 
	/// The context manager can only access current context object accessable by the thread. (The same context may pass around between threads)
	/// 
	/// 
	/// 
	/// </summary>
	public class ContextManager
	{
		static IContextStore _currentContextStore;
		static object lockObject = new object();
		/// <summary>
		/// Gets or sets the name of the context type.
		/// </summary>
		/// <value>The name of the context type.</value>
		static IContextStore Current
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
							_currentContextStore = EnvironmentManager.GetProvider<IContextStore>();
							if (_currentContextStore == null)
							{
								//load default
								_currentContextStore = new CallContextLocalStore();
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

	}
}
