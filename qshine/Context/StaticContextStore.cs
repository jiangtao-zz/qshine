using System.Collections.Concurrent;

namespace qshine
{
    /// <summary>
    /// Wrap a static key/valye pair
    /// This context shared data globally.
    /// </summary>
    public class StaticContextStore : IContextStore
    {
        static ConcurrentDictionary<string, object> _state = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Gets the context data.
        /// </summary>
        /// <returns>The data.</returns>
        /// <param name="name">Name.</param>
        public object GetData(string name)
        {
            object data;
            if (_state.TryGetValue(name, out data))
            {
                return data;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the data.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="data">Data.</param>
        public void SetData(string name, object data)
        {
            _state[name] = data;
            //_state.AddOrUpdate(name, data, (k,v)=>v);
        }

        /// <summary>
        /// Frees the data.
        /// </summary>
        /// <param name="name">Name.</param>
        public void FreeData(string name)
        {
            object value;
            _state.TryRemove(name, out value);
        }

        /// <summary>
        /// Gets the name of the context type.
        /// </summary>
        /// <value>The name of the context type.</value>
        public ContextStoreType ContextType
        {
            get
            {
                return ContextStoreType.Static;
            }
        }
    }

}
