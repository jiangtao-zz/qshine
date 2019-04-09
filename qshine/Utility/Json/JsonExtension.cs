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
    /// <summary>
    /// Json format
    /// </summary>
    [Flags]
	public enum JsonFormat:ulong
	{
        /// <summary>
        /// default format
        /// </summary>
		Default = 1,		//default
		/// <summary>
		/// The microsoft date format.
		/// 	\/Date(946645200000)\/ (Utc)
		///		\/Date(946645200000+1100)\/ (Local and Unspecified)
		/// </summary>
		MicrosoftDateFormat = 0x1, //"\"\\/Date(1335205592410)\\/\"" 
		/// <summary>
		/// The ISO 8601 date format.
		/// 	2012-04-23T18:25:43 (Unspecified)
		/// 	2012-04-23T18:25:43Z (Utc)
		/// 	2012-04-23T18:25:43-05:00 (Local)
		/// </summary>
		ISO8601DateFormat = 0x2,
		/// <summary>
		/// The ISO 8601 roundtrip date format.
		/// 	2012-04-23T18:25:43.1234567 (Unspecified)
		/// 	2012-04-23T18:25:43.1234567Z, (UTC)
		///		2012-04-23T18:25:43.1234567-05:00 (local)
		/// </summary>
		ISO8601RoundtripDateFormat = 0x4,
		/// <summary>
		/// The ISO 8601 javascript JSON.stringify date format.
		/// 	2012-04-23T18:25:43.123 (Unspecified)
		///		2012-04-23T18:25:43.007Z, (Utc)
		/// 	2012-04-23T18:25:43.007-05:00, (local)
		/// </summary>
		ISO8601JavascriptDateFormat = 0x8,
        /// <summary>
        /// Convert below type javascript date format.
        /// 	new Date(976918263055)
        /// 	new Date(2012,04)
        /// 	new Date(2012,04,23)
        /// 	new Date(2012,04,23,18)
        /// 	new Date(2012,04,23,18,25)
        ///		new Date(2012,04,23,18,25,43)
        ///		new Date(2012,04,23,18,25,43,100)
        /// </summary>
        JavascriptDateFormat = 0x10,
        /// <summary>
        /// Custom Date format
        /// </summary>
		CustomDateFormat = 0x20,
		/// <summary>
        /// 
        /// </summary>
        //UseSimpleDictionaryFormat = 0x00
		UseDictionaryFormat = 0x100,
        /// <summary>
        /// 
        /// </summary>
		//EmitTypeInformation.Never = 0x000
		EmitTypeAlways = 0x1000, //EmitTypeInformation.Always
        /// <summary>
        /// 
        /// </summary>
		EmitTypeAsNeeded = 0x2000 //EmitTypeInformation.AsNeed
	};

    /// <summary>
    /// Special custom json format setting
    /// </summary>
    public class JsonFormatSetting
    {
        /// <summary>
        /// Custom date time format
        /// </summary>
        public string DateTimeFormat { get; set; }
    }

    /// <summary>
    /// Extension methods for JSON serialization
    /// </summary>
    public static class Json
    {
		private static IJsonProvider _jsonProvider;
		private static IJsonSerializer _jsonSerializer;
		private readonly static object lockobject = new object();

		private static Interceptor _interceptor = Interceptor.Get<IJsonSerializer>();


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
        /// <param name="jsonFormat">json format</param>
        /// /// <param name="setting">json format setting</param>
        /// <returns></returns>
		public static string Serialize(this object instance, JsonFormat jsonFormat=JsonFormat.Default, JsonFormatSetting setting =null)
        {
			return _interceptor.JoinPoint<string>(() =>
			{
				return CurrentSerializer.Serialize(instance, jsonFormat, setting);
			},  CurrentSerializer, "Serialize",instance,jsonFormat);
        }

        /// <summary>
        /// Deserialize a JSON string to a given type of object
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="jsonString">JSON string</param>
        /// <param name="jsonFormat">json format</param>
        /// /// <param name="setting">json format setting</param>
        /// <returns>Returns a given type of instance.</returns>
        /// <remarks>
        /// The given type properties name must exist in serialized json string. Otherwise, it will throw exception
        /// </remarks>
        public static T Deserialize<T>(this string jsonString, JsonFormat jsonFormat=JsonFormat.Default, JsonFormatSetting setting = null)
        {
            return (T)Deserialize(jsonString, typeof(T), jsonFormat, setting);
        }

        /// <summary>
        /// Deserialize a json string to a given type of object
        /// </summary>
        /// <param name="jsonString">json string</param>
        /// <param name="type">Target object type</param>
        /// <param name="jsonFormat">json format</param>
        /// /// <param name="setting">json format setting</param>
        /// <returns>Returns a given type of instance.</returns>
        /// <remarks>
        /// The given type properties name must exist in serialized json string. Otherwise, it will throw exception
        /// </remarks>
        public static object Deserialize(this string jsonString, Type type,JsonFormat jsonFormat=JsonFormat.Default, JsonFormatSetting setting = null)
        {
			return _interceptor.JoinPoint<object>(() =>
			{
				return CurrentSerializer.Deserialize(jsonString, type, jsonFormat, setting);
			},  CurrentSerializer, "Deserialize",jsonString, type, jsonFormat);
			
        }

        /// <summary>
        /// Deserialize a json string to a given type of object by type name.
        /// </summary>
        /// <param name="jsonString">json string</param>
        /// <param name="typeName">Type name of the target object. It could be a object name, full name or a qualified object name.
		/// The type name could be a type in plugable component assembly</param>
        /// <returns>Returns a given type of instance.</returns>
        /// <param name="jsonFormat">json format</param>
        /// /// <param name="setting">json format setting</param>
        /// <remarks>
        /// The given type properties name must exist in serialized json string. Otherwise, it will throw exception.
        /// </remarks>
        public static object Deserialize(this string jsonString, string typeName, JsonFormat jsonFormat = JsonFormat.Default, JsonFormatSetting setting = null)
        {
            object result = null;
            if (!string.IsNullOrEmpty(jsonString) && !string.IsNullOrEmpty(typeName))
            {
                var type = ApplicationEnvironment.GetTypeByName(typeName);
                result = Deserialize(jsonString, type, jsonFormat, setting);
            }
            return result;
        }

        /// <summary>
        /// Deserialize a json string to a name-value dictionary.
        /// The dictionary key is the property name and the value is the property value.
        /// The value will be converted to:
        ///     string => string
        ///     number => decimal
        ///     array/list => list
        ///     class object => Dictionary [string, object]
        ///     dictionary => Dictionary [string, object]
        ///     Date => Date (depends on JsonFormat)
        /// </summary>
        /// <param name="jsonString">json format string</param>
		/// <param name="jsonFormat">Json format.</param>
        /// /// <param name="setting">json format setting</param>
        /// <returns>returns dictionary instance which contains all property names and values</returns>
        public static Dictionary<string, object> DeserializeDictionary(this string jsonString, JsonFormat jsonFormat = JsonFormat.Default, JsonFormatSetting setting = null)
        {
            return CurrentSerializer.DeserializeDictionary(jsonString, jsonFormat, setting);
        }
    }

}
