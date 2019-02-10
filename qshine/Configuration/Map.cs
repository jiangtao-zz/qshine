using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Configuration
{
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

        public bool ContainsKey(string name)
        {
            return _mapItems.ContainsKey(name);
        }

    }
}
