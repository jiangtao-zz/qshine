using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
#if NETCORE
#else
using System.Runtime.Remoting.Messaging;
#endif
using System.Threading;

namespace qshine
{
	/// <summary>
	/// This context shared data within thread.
    /// Do not use ThreadLocal. The ThreadLocal do not clean up previous thread context if the thread id is same.
	/// </summary>
	public class LocalContextStore:IContextStore
	{
        [ThreadStatic]
        static Dictionary<string, object> _state;

        static Dictionary<string, object> Store
        {
            get
            {
                if (_state == null) _state = new Dictionary<string, object>();
                return _state;
            }
        }

        /// <summary>
        /// Gets the local thread context data.
        /// </summary>
        /// <returns>The data.</returns>
        /// <param name="name">Name.</param>
        public object GetData(string name)
		{
            if (Store.ContainsKey(name))
                return Store[name];
            return null;
        }

        /// <summary>
        /// Sets the data.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="data">Data.</param>
        public void SetData(string name, object data)
		{
            Store[name] = data;
            //((ThreadLocal<object>)_state.GetOrAdd(name, _ => new ThreadLocal<object>()))
                //.Value = data;
        }

        /// <summary>
        /// Frees the data.
        /// </summary>
        /// <param name="name">Name.</param>
        public void FreeData(string name)
		{
            if (Store.ContainsKey(name))
                Store.Remove(name);
        }

        /// <summary>
        /// Gets the name of the context type.
        /// </summary>
        /// <value>The name of the context type.</value>
        public ContextStoreType ContextType
		{
			get
			{
				return ContextStoreType.ThreadLocal;
			}
		}
	}

#if NETCORE
    //NETCORE doesn't support logical call context.
    public static class CallContext
    {
        static ConcurrentDictionary<string, object> _state = new ConcurrentDictionary<string, object>();
 
        public static void SetData<T>(string name, T data)
        {
            ((AsyncLocal<T>)_state.GetOrAdd(name, _ => new AsyncLocal<T>()))
                .Value = data;
        }

        private static T GetData<T>(string name)
        {
            object data;
            if (_state.TryGetValue(name, out data))
                return ((AsyncLocal<T>)data).Value;
 
            return default(T);
        }

        public static object GetData(string name) => GetData<object>(name);

        private static T LogicalGetData<T>(string name)
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
