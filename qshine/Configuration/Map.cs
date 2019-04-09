using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Configuration
{
    /// <summary>
    /// A named map collection
    /// </summary>
    public class Map
    {
        /// <summary>
        /// Map name or category
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Default map item
        /// </summary>
        public string Default { get; set; }

        readonly Dictionary<string, string> _mapItems = new Dictionary<string, string>();
        /// <summary>
        /// Map items
        /// </summary>
        public Dictionary<string, string> Items
        {
            get
            {
                return _mapItems;
            }
        }
        /// <summary>
        /// Map item by name
        /// </summary>
        public string this[string name]
        {
            get {
                return _mapItems[name];
            }

            set
            {
                if (_mapItems.ContainsKey(name))
                {
                    _mapItems[name] = value;
                }
                else
                {
                    _mapItems.Add(name, value);
                }
            }
        }

        /// <summary>
        /// Check whether a map key exists
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true if map contains the key</returns>
        public bool ContainsKey(string key)
        {
            return _mapItems.ContainsKey(key);
        }

        /// <summary>
        /// Get number of map item
        /// </summary>
        public int Count
        {
            get
            {
                return _mapItems.Count;
            }
        }

        /// <summary>
        /// Get Map name by the type.
        /// </summary>
        /// <param name="type">Type of component</param>
        /// <returns></returns>
        public static string GetMapName(Type type)
        {
            return type.FullName;
        }


    }
}
