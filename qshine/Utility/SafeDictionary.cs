using System.Collections.Generic;
using System.Linq;

namespace qshine.Utility
{
    public class SafeDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _dictionary;

        private object _safeLocker = new object();

        public SafeDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public bool ContainsKey(TKey key)
        {
            lock (_safeLocker)
            {
                return _dictionary.ContainsKey(key);
            }
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (_safeLocker)
                {
                    return _dictionary.Values;
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (_safeLocker)
                {
                    return _dictionary.Keys;
                }
            }
        }


        public TValue this[TKey key]
        {
            get
            {
                lock (_safeLocker)
                {
                    return _dictionary[key];
                }
            }
            set
            {
                lock (_safeLocker)
                {
                    _dictionary[key] = value;
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (_safeLocker)
            {
                _dictionary.Add(key, value);
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            lock (_safeLocker)
            {
                return _dictionary.Contains(keyValuePair);
            }
        }


        public void Clear()
        {
            lock (_safeLocker)
            {
                _dictionary.Clear();
            }
        }

        public bool ContainsValue(TValue value)
        {
            lock (_safeLocker)
            {
                return _dictionary.ContainsValue(value);
            }
        }

        public bool Remove(TKey key)
        {
            lock (_safeLocker)
            {
                return _dictionary.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_safeLocker)
            {
                return _dictionary.TryGetValue(key, out value);
            }
        }
    }
}
