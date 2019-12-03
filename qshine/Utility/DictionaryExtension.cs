using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Utility
{
    /// <summary>
    /// IDictionary extension
    /// </summary>
    public static class DictionaryExtension
    {
        /// <summary>
        /// Add a new dictionary item or update existing item by the key.
        /// </summary>
        /// <typeparam name="TKey">Type of key</typeparam>
        /// <typeparam name="TValue">Type of value</typeparam>
        /// <param name="dictionary">dictionary instance</param>
        /// <param name="key">item key</param>
        /// <param name="value">item value</param>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key,value);
            }
        }

        /// <summary>
        /// Get a string value from dictionary regardless key exists or not.
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">value type</typeparam>
        /// <param name="dictionary">Dictionary</param>
        /// <param name="key">dictionary key.</param>
        /// <param name="defaultValue">returns default value if key doesn't exist.</param>
        /// <returns></returns>
        public static string GetString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, string defaultValue="")
        {
            if (dictionary.ContainsKey(key))
            {
                if (dictionary[key] == null) return null;

                return dictionary[key].ToString();
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Cast a dictionary value to given type value regardless key exists or not.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type</typeparam>
        /// <typeparam name="TValue">Dictionary value type</typeparam>
        /// <typeparam name="T">Cast result type</typeparam>
        /// <param name="dictionary">Dictionary</param>
        /// <param name="key">dictionary key.</param>
        /// <param name="defaultValue">returns default value if key doesn't exist.</param>
        /// <returns></returns>
        public static T GetCastValue<TKey, TValue, T>(this IDictionary<TKey, TValue> dictionary, TKey key, T defaultValue)
        {
            if (dictionary.ContainsKey(key))
            {
                if (dictionary[key] == null) return default(T);

                return (T)Convert.ChangeType(dictionary[key], typeof(T));
            }
            else
            {
                return defaultValue;
            }
        }

    }
}
