using System;
using System.Collections.Concurrent;
#if NETCORE
#else
using System.Runtime.Remoting.Messaging;
#endif
using System.Threading;

namespace qshine
{
	/// <summary>
	/// Wrap .NET call context local data
	/// This context shared data within thread execution path.
	/// It is better than ThreadStatic to store data within same thread. 
	/// </summary>
	public class CallContextLocalStore:IContextStore
	{
		/// <summary>
		/// Gets the context data.
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="name">Name.</param>
		public object GetData(string name)
		{
			return CallContext.LogicalGetData(name);
		}

		/// <summary>
		/// Sets the data.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="data">Data.</param>
		public void SetData(string name, object data)
		{
			CallContext.LogicalSetData(name, data);
		}

		/// <summary>
		/// Frees the data.
		/// </summary>
		/// <param name="name">Name.</param>
		public void FreeData(string name)
		{
			CallContext.FreeNamedDataSlot(name);
		}

		/// <summary>
		/// Gets the name of the context type.
		/// </summary>
		/// <value>The name of the context type.</value>
		public ContextStoreType ContextType
		{
			get
			{
				return ContextStoreType.CallLocal;
			}
		}
	}

#if NETCORE
    public static class CallContext
    {
        static ConcurrentDictionary<string, object> _state = new ConcurrentDictionary<string, object>();
 
        public static void SetData<T>(string name, T data)
        {
            ((AsyncLocal<T>)_state.GetOrAdd(name, _ => new AsyncLocal<T>()))
                .Value = data;
        }

        public static T GetData<T>(string name)
        {
            object data;
            if (_state.TryGetValue(name, out data))
                return ((AsyncLocal<T>)data).Value;
 
            return default(T);
        }

        public static object GetData(string name) => GetData<object>(name);

        public static T LogicalGetData<T>(string name)
        {
            return GetData<T>(name);
        }

        public static object LogicalGetData(string name)
        {
            return GetData(name);
        }

        public static void LogicalSetData(string name, object value)
        {
            SetData(name, value);
        }

        public static void FreeNamedDataSlot(string name)
        {
            object value;
            _state.TryRemove(name, out value);
        }
    }
#endif
}
