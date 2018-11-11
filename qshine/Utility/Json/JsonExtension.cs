using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Json;
using qshine.Configuration;

namespace qshine
{
	public enum JsonFormat:ulong
	{
		Default = 0,		//default
		/// <summary>
		/// The microsoft date format.
		/// 	\/Date(946645200000)\/ (Utc)
		///		\/Date(946645200000+1100)\/ (Local and Unspecified)
		/// </summary>
		MicrosoftDateFormat = 0x0, //"\"\\/Date(1335205592410)\\/\"" 
		/// <summary>
		/// The ISO 8601 date format.
		/// 	2012-04-23T18:25:43 (Unspecified)
		/// 	2012-04-23T18:25:43Z (Utc)
		/// 	2012-04-23T18:25:43-05:00 (Local)
		/// </summary>
		ISO8601DateFormat = 0x1,
		/// <summary>
		/// The ISO 8601 roundtrip date format.
		/// 	2012-04-23T18:25:43.1234567 (Unspecified)
		/// 	2012-04-23T18:25:43.1234567Z, (UTC)
		///		2012-04-23T18:25:43.1234567-05:00 (local)
		/// </summary>
		ISO8601RoundtripDateFormat = 0x2,
		/// <summary>
		/// The ISO 8601 javascript JSON.stringify date format.
		/// 	2012-04-23T18:25:43.123 (Unspecified)
		///		2012-04-23T18:25:43.007Z, (Utc)
		/// 	2012-04-23T18:25:43.007-05:00, (local)
		/// </summary>
		ISO8601JavascriptDateFormat = 0x4,

		CustomDateFormat = 0x8,
		//UseSimpleDictionaryFormat = 0x00
		UseDictionaryFormat = 0x10,
		//EmitTypeInformation.Never = 0x000
		EmitTypeAlways = 0x100, //EmitTypeInformation.Always
		EmitTypeAsNeeded = 0x200 //EmitTypeInformation.Always
	};
    /// <summary>
    /// Extension methods for JSON serialization
    /// </summary>
    public static class Json
    {
		private static IJsonProvider _jsonProvider;
		private static IJsonSerializer _jsonSerializer;
		private static object lockobject = new object();

		private static Interceptor _interceptor = Interceptor.Register(typeof(IJsonSerializer));


		/// <summary>
		/// Gets or sets the current json serializer.
		/// </summary>
		/// <value>The current serializer.</value>
		public static IJsonSerializer CurrentSerializer {
			get {
				if (_jsonSerializer == null)
				{
					_jsonSerializer = Provider.Create();
				}
				return _jsonSerializer;
			}
		}
		/// <summary>
		/// Gets or sets the provider.
		/// </summary>
		/// <value>The provider.</value>
		public static IJsonProvider Provider
		{
			get
			{
				if (_jsonProvider == null)
				{
					var provider = ApplicationEnvironment.GetProvider<IJsonProvider>();
					if (provider == null)
					{
						provider = new DotNetJsonSerializerProvider();
					}
					lock(lockobject)
					{
						if (_jsonProvider==null)
						{
							_jsonProvider = provider;
						}
					}
				}
				return _jsonProvider;
			}
			set
			{
				lock(lockobject)
				{
					_jsonProvider = value;
					if (_jsonProvider != null)
					{
						_jsonSerializer = _jsonProvider.Create();
					}
				}
			}
		}

        /// <summary>
        /// Serializes object into JSON string
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
		public static string Serialize(this object instance, JsonFormat jsonFormat=JsonFormat.Default)
        {
			return _interceptor.JoinPoint<string>(() =>
			{
				return CurrentSerializer.Serialize(instance, jsonFormat);
			},  CurrentSerializer, "Serialize",instance,jsonFormat);
        }

        /// <summary>
        /// Deserialize a JSON string to a given type of object
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="jsonString">JSON string</param>
        /// <returns>Returns a given type of instance.</returns>
        /// <remarks>
        /// The given type properties name must exist in serialized json string. Otherwise, it will throw exception
        /// </remarks>
        public static T Deserialize<T>(this string jsonString, JsonFormat jsonFormat=JsonFormat.Default)
        {
            return (T)Deserialize(jsonString, typeof(T), jsonFormat);
        }

        /// <summary>
        /// Deserialize a json string to a given type of object
        /// </summary>
        /// <param name="jsonString">json string</param>
        /// <param name="type">Target object type</param>
        /// <returns>Returns a given type of instance.</returns>
        /// <remarks>
        /// The given type properties name must exist in serialized json string. Otherwise, it will throw exception
        /// </remarks>
        public static object Deserialize(this string jsonString, Type type,JsonFormat jsonFormat=JsonFormat.Default)
        {
			return _interceptor.JoinPoint<object>(() =>
			{
				return CurrentSerializer.Deserialize(jsonString, type, jsonFormat);
			},  CurrentSerializer, "Deserialize",jsonString, type, jsonFormat);
			
        }

        /// <summary>
        /// Deserialize a json string to a given type of object by type name.
        /// </summary>
        /// <param name="jsonString">json string</param>
        /// <param name="typeName">Type name of the target object. It could be a object name, full name or a qualified object name.
		/// The type name could be a type in plugable component assembly</param>
        /// <returns>Returns a given type of instance.</returns>
        /// <remarks>
        /// The given type properties name must exist in serialized json string. Otherwise, it will throw exception.
        /// </remarks>
        public static object Deserialize(this string jsonString, string typeName, JsonFormat jsonFormat=JsonFormat.Default)
        {
            object result = null;
            if (!string.IsNullOrEmpty(jsonString) && !string.IsNullOrEmpty(typeName))
            {
                var type = ApplicationEnvironment.GetNamedType(typeName);
                result = Deserialize(jsonString, type, jsonFormat);
            }
            return result;
        }

        /// <summary>
        /// Deserialize a JSON string to a key-value pair array. The key contains all object property name.
        /// The value is only available for c# built-in type. The other type value will direct show the json text.
        /// </summary>
        /// <param name="jsonString">json string</param>
        /// <returns>
        /// It returns a key-value pair array that contains all object property names and corresponding value.
        /// </returns>
        public static Dictionary<string, object> DeserializeDictionary(this string jsonString)
        {
            var obj = Deserialize<StringObjectDictionary>(jsonString);
            return obj.dict;
        }
    }

    /// <summary>
    /// Serialize an object to a key-value pair array.
    /// </summary>
    [Serializable]
    internal class StringObjectDictionary : ISerializable
    {
        internal Dictionary<string, object> dict;

        public StringObjectDictionary()
        {
            dict = new Dictionary<string, object>();
        }

        protected StringObjectDictionary(SerializationInfo info, StreamingContext context)
        {
            dict = new Dictionary<string, object>();
            foreach (var entry in info)
            {
                var dateString = entry.Value as string;
                if (dateString != null && dateString.IndexOf("/Date(", StringComparison.Ordinal) == 0)
                {
                    dict.Add(entry.Name, ConvertJsonDate(dateString));
                }
                else
                {
                    dict.Add(entry.Name, entry.Value);
                }
            }
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (var key in dict.Keys)
            {
                info.AddValue(key, dict[key]);
            }
        }

        public static DateTime ConvertJsonDate(string jsonDateString)
        {
            var match = Regex.Match(jsonDateString, @"/Date\((-?\d+)([+-]\d{2})?(\d{2})?.*", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var dt = new DateTime(1970, 1, 1); // Epoch date, used by the JavaScriptSerializer to represent starting point of datetime in JSON.
                dt = dt.AddMilliseconds(long.Parse(match.Groups[1].Value));
                dt = dt.AddHours(long.Parse(match.Groups[2].Value));
                //dt = dt.ToLocalTime();
                return dt;
            }
            return DateTime.MinValue;
        }
    }
}
