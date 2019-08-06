using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using qshine.Utility;

namespace qshine.Configuration
{
    /// <summary>
    /// A named map collection is a list of key value pair associated with a particular name.
    ///The name usually is a tag to group or categorize a set of value mapping.
    ///Each value mapping item represets a key associated with a meaningful value.
    ///
    ///For example, an interface type name can be used to group a set of interface type implementations.
    ///One mapping item could be used to map a namespace to a particular interface implementation.
    ///
    /// </summary>
    public class Map
    {
        /// <summary>
        /// Map name or category.
        /// Example, "qshine.Messaging.IEventBusFactory" represents a collection of event bus factory class implementations.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies a default map item value.
        /// A map collection only list a number of map items. This default value will be selected if map key is not in the collection.
        /// </summary>
        public string Default { get; set; }

        readonly Dictionary<string, string> _mapItems = new Dictionary<string, string>();
        /// <summary>
        /// Key/Value pair map items
        /// The Key could be a regular expression pattern used to match given key map item.
        /// </summary>
        public Dictionary<string, string> Items
        {
            get
            {
                return _mapItems;
            }
        }

        /// <summary>
        /// Get/Set map item value
        /// </summary>
        public string this[string key]
        {
            get {
                return _mapItems[key];
            }

            set
            {
                if (_mapItems.ContainsKey(key))
                {
                    _mapItems[key] = value;
                }
                else
                {
                    _mapItems.Add(key, value);
                }
            }
        }

        /// <summary>
        /// Match key to one of map collection item.
        /// </summary>
        /// <param name="key">A key value to be match</param>
        /// <returns>Returns a matched mapping item or default value if nothing found.
        /// The default value could be null or empty if the default value is not set.</returns>
        public string MatchKeyValue(string key)
        {
            //match exactly key
            if (_mapItems.ContainsKey(key))
            {
                return _mapItems[key];
            }

            //Match through reg expression
            var matchKey = _mapItems.Keys.FirstOrDefault(x => key.Match(x));
            if (!string.IsNullOrEmpty(matchKey))
            {
                return _mapItems[matchKey];
            }
            //returns default item if nothing found.
            return Default;
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
