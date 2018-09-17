using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace qshine.Utility
{
    /// <summary>
    /// A simple version Thread-Safe Collections
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    public class SafeDictionary<TKey, TValue>
    {
        private ConcurrentDictionary<TKey, TValue> _dictionary;

        private object _safeLocker = new object();

        public SafeDictionary()
        {
            _dictionary = new ConcurrentDictionary<TKey, TValue>();
        }

        /// <summary>
        /// Determines whether the Dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the Dictionary.</param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }
        /// <summary>
        /// Gets the number of key/value pairs
        /// </summary>
        public int Count
        {
            get { return _dictionary.Count; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                return _dictionary.Values;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return _dictionary.Keys;
            }
        }

        /// <summary>
        /// Get value by given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get
            {
                return _dictionary[key];
            }
            set
            {
                _dictionary[key] = value;
            }
        }

        /// <summary>
        /// Add a key value if the key not exists.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            _dictionary.TryAdd(key, value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            return _dictionary.Contains(keyValuePair);
        }


        public void Clear()
        {
            _dictionary.Clear();
        }

        /// <summary>
        /// Remove an existing key valye pair
        /// It should not be used for public. It could cause Keys and Values collection thread-unsafe
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal bool Remove(TKey key)
        {
            TValue value;
            return _dictionary.TryRemove(key, out value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
}
