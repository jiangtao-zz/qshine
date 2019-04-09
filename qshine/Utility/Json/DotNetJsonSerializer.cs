using qshine.Configuration;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Linq;

namespace qshine
{
    /// <summary>
    /// Dotnet json serializer provider
    /// </summary>
	public class DotNetJsonSerializerProvider : IJsonProvider
	{
        /// <summary>
        /// Create a json serializer
        /// </summary>
        /// <returns></returns>
		public IJsonSerializer Create()
		{
			return new DotNetJsonSerializer();
		}
	}

    /// <summary>
    /// Dotnet json serializer
    /// </summary>
	public class DotNetJsonSerializer:IJsonSerializer
	{
        /// <summary>
        /// Deserialize the specified jsonString to a typed object.
        /// </summary>
        /// <returns>The deserialized object</returns>
        /// <param name="jsonString">Json string.</param>
        /// <param name="type">Type of the instance.</param>
        /// <param name="jsonFormat">Json format</param>
        /// <param name="setting">Json format setting</param>
        public object Deserialize(string jsonString, Type type, JsonFormat jsonFormat, JsonFormatSetting setting)
		{
			object result = null;
            if (!string.IsNullOrEmpty(jsonString) && type != null)
            {
				var serializer = new DataContractJsonSerializer(type, ConvertFormat(jsonFormat, setting));
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString.Replace("\r", "\\r").Replace("\n", "\\n"))))
                {
                    result = serializer.ReadObject(ms);
                }
            }
            return result;
        }

		/// <summary>
		/// Serialize the specified instance.
		/// </summary>
		/// <returns>The serialize.</returns>
		/// <param name="instance">Instance.</param>
        /// <param name="jsonFormat">Json format</param>
        /// <param name="setting">Json format setting</param>
        public string Serialize(object instance, JsonFormat jsonFormat, JsonFormatSetting setting)
		{
			string jsonString = null;
			var serializer = new DataContractJsonSerializer(instance.GetType(), ConvertFormat(jsonFormat, setting));
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, instance);
                jsonString = Encoding.UTF8.GetString(ms.ToArray());
            }
			return jsonString;
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
        /// <param name="setting">Json format setting</param>
        /// <returns>Dictionary instance</returns>
        public Dictionary<string, object> DeserializeDictionary(string jsonString, JsonFormat jsonFormat, JsonFormatSetting setting)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(jsonString))
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString.Replace("\r", "\\r").Replace("\n", "\\n"))))
                {
                    using (var reader = JsonReaderWriterFactory.CreateJsonReader(ms, new System.Xml.XmlDictionaryReaderQuotas()))
                    {
                        dictionary = ReadJsonXml(dictionary, reader, null, jsonFormat,setting);
                    }
                }
                return ConvertToJsonDictionary(dictionary, jsonFormat, setting);
            }
            return dictionary;
        }

        static Dictionary<string, object> ConvertToJsonDictionary(Dictionary<string, object> input, JsonFormat jsonFormat, JsonFormatSetting setting)
        {
            foreach (var v in input.Keys.ToArray())
            {
                if (input[v] is List<object> list && list.Count > 0)
                {
                    //convert "item" array to dictionary when using "UseSimpleDictionaryFormat = false"
                    if (list[0] is Dictionary<string, object> dic && dic.Count == 2 && dic.ContainsKey("Key"))
                    {
                        var convertedList = new Dictionary<string, object>();
                        foreach (Dictionary<string, object> k in list)
                        {
                            convertedList.Add(k["Key"].ToString(), k["Value"]);
                        }
                        input[v] = convertedList;
                    }
                }
                else if (input[v] is Dictionary<string, object>)
                {
                    ConvertToJsonDictionary(input[v] as Dictionary<string, object>, jsonFormat, setting);
                }
            }
            return input;
        }

        /// <summary>
        /// Json basic types
        /// </summary>
        static Dictionary<string, Type> _jsonBasicType = new Dictionary<string, Type>
        {
            //basic
            {"object", typeof(object)},
            {"array", typeof(Array)},
            {"string", typeof(string)},
            {"number", typeof(decimal)},
            {"boolean", typeof(bool)},
            {"null", typeof(DBNull)},//hack: use DBNull as null type value
            //python
            {"list", typeof(Array)},
            {"bool", typeof(bool)},
            {"dict", typeof(object)},
            {"int", typeof(int)},
            {"float", typeof(float)},
            {"None", typeof(DBNull)},//hack: use DBNull as null type value
        };

        static Type GetJsonBasicType(string typeValue)
        {
            if (_jsonBasicType.ContainsKey(typeValue))
            {
                return _jsonBasicType[typeValue];
            }
            else
            {
                //unsupportted type
                return typeof(string);
            }
        }

        static dynamic ReadJsonXml(object parent, XmlReader reader, Type valueType, JsonFormat jsonFormat, JsonFormatSetting setting)
        {
            object result = null;
            string name = "";
            Type dataType = valueType;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        name = reader.LocalName;
                        if (reader.MoveToFirstAttribute() && reader.Name == "type")
                        {
                            dataType = GetJsonBasicType(reader.Value);

                            var parray = parent as List<object>;

                            if (dataType == typeof(object))
                            {
                                //Object map to dictionary
                                result = new Dictionary<string, object>();
                                ReadJsonXml(result, reader, dataType, jsonFormat, setting);

                            }else if (dataType == typeof(Array))
                            {
                                result = new List<object>();
                                ReadJsonXml(result, reader, dataType, jsonFormat, setting);
                            }
                            else
                            {
                                result = ReadJsonXml(parent, reader, dataType, jsonFormat, setting);
                            }

                            if (parent is Dictionary<string, object> pdic)
                            {
                                pdic.Add(name, result);
                            }
                            else if (parray != null)
                            {
                                parray.Add(result);
                            }
                        }
                        break;
                    case XmlNodeType.Text:
                        if (dataType == typeof(bool))
                        {
                            result = reader.ReadContentAsBoolean();
                        }
                        else if (dataType==typeof(DBNull))
                        {
                            result = null;
                        }
                        else
                        {
                            result = reader.ReadContentAs(dataType, null);
                        }
                        if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            if (dataType == typeof(string) && result != null)
                            {
                                if (TryParseDateTime(jsonFormat, setting, result.ToString(), out DateTime d))
                                {
                                    result = d;
                                }
                            }
                            return result;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        return result;
                    default:
                        throw new InvalidDataException("Unhandled node type: " + reader.NodeType);
                }
            }
            return result;
        }

        static DotNetJsonSerializer _dotnetJsonSerialiser = new DotNetJsonSerializer();

        static bool TryParseDateTime(JsonFormat jsonFormat, JsonFormatSetting setting, string jsonDate, out DateTime date)
        {
            int len = jsonDate.Length;
            date = DateTime.MinValue;
            if(
                ((jsonFormat & JsonFormat.MicrosoftDateFormat) !=0 && len>10 && jsonDate.StartsWith("/Date(")) ||
                (len >= 17 && len <= 40 && jsonDate[4]=='-' && jsonDate[7] == '-' && jsonDate[10] == 'T' 
                && jsonDate[13] == ':' && jsonDate[16] == ':') && (
                (jsonFormat & JsonFormat.ISO8601JavascriptDateFormat) != 0 ||
                (jsonFormat & JsonFormat.ISO8601RoundtripDateFormat) != 0)
                )
            {
                //"\"\\/Date(1335205592410)\\/\""
                //2012-04-23T18:25:43
                //2012-04-23T18:25:43.123
                //2012-04-23T18:25:43.1234567
                //2012-04-23T18:25:43.1234567 - 05:00

                try
                {
                    if (_dotnetJsonSerialiser.Deserialize(string.Format("{{\"V\":\"{0}\"}}", jsonDate), 
                        typeof(__JDate), jsonFormat, setting) is __JDate v)
                    {
                        date = v.V;
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        static DataContractJsonSerializerSettings ConvertFormat(JsonFormat formatOption, JsonFormatSetting setting)
		{
			var format = new DataContractJsonSerializerSettings
			{
				EmitTypeInformation = EmitTypeInformation.Never,
				UseSimpleDictionaryFormat = true
			};

            if (((ulong)formatOption & 0xFF) == (ulong)JsonFormat.MicrosoftDateFormat)
            {
                //default
                // The microsoft date format.
                // 	\/Date(946645200000)\/ (Utc)
                //		\/Date(946645200000+1100)\/ (Local and Unspecified)
            }
            else if (((ulong)formatOption & 0xFF) == (ulong)JsonFormat.ISO8601DateFormat)
            {
                // The ISO 8601 date format.
                // 	2012-04-23T18:25:43 (Unspecified)
                // 	2012-04-23T18:25:43Z (Utc)
                // 	2012-04-23T18:25:43-05:00 (Local)
                format.DateTimeFormat = new DateTimeFormat("yyyy-MM-ddTHH:mm:ssK");//"yyyy-MM-ddTHH:mm:ssZ");
            }
            else if (((ulong)formatOption & 0xFF) == (ulong)JsonFormat.ISO8601RoundtripDateFormat)
            {
                // The ISO 8601 roundtrip date format.
                // 	2012-04-23T18:25:43.1234567 (Unspecified)
                // 	2012-04-23T18:25:43.1234567Z, (UTC)
                //	2012-04-23T18:25:43.1234567-05:00 (local)
                format.DateTimeFormat = new DateTimeFormat("o");//"yyyy-MM-dd'T'HH:mm:ss.fffffffZ");
            }
            else if (((ulong)formatOption & 0xFF) == (ulong)JsonFormat.ISO8601JavascriptDateFormat)
            {
                // The ISO 8601 javascript JSON.stringify date format.
                // 	2012-04-23T18:25:43.123 (Unspecified)
                //	2012-04-23T18:25:43.007Z, (Utc)
                // 	2012-04-23T18:25:43.007-05:00, (local)
                format.DateTimeFormat = new DateTimeFormat("yyyy-MM-ddTHH:mm:ss.fffK");
            }
            else if (((ulong)formatOption & 0xFF) == (ulong)JsonFormat.JavascriptDateFormat)
            {
                // Convert below type javascript date format.
                // 	new Date(976918263055)
                // 	new Date(2012,04)
                // 	new Date(2012,04,23)
                // 	new Date(2012,04,23,18)
                // 	new Date(2012,04,23,18,25)
                //	new Date(2012,04,23,18,25,43)
                //	new Date(2012,04,23,18,25,43,100)
                //setting.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                format.DateTimeFormat = new DateTimeFormat("\"new Date(\"yyyy,MM,dd,HH,mm,ss,fff\")\"");
            }
            else if (((ulong)formatOption & 0xFF) == (ulong)JsonFormat.CustomDateFormat)
            {
                string dateFormat = ((setting != null) && !string.IsNullOrEmpty(setting.DateTimeFormat)) ? setting.DateTimeFormat : "s";

                format.DateTimeFormat = new DateTimeFormat(dateFormat);
            }
            return format;
        }

        /// <summary>
        /// internal use only
        /// It must be a public property which allow serialize/deserialize
        /// </summary>
        public class __JDate
        {
            /// <summary>
            /// Using DateTime type for json date
            /// </summary>
            public DateTime V { get; set; }
        }

        /*
        static DateTime ConvertJsonDate(string jsonDateString)
        {
            var match = Regex.Match(jsonDateString, @"/Date\((-?\d+)([+-]\d{2})?(\d{2})?.*", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var dt = new DateTime(1970, 1, 1); // Epoch date, used by the JavaScriptSerializer to represent starting point of datetime in JSON.
                dt = dt.AddMilliseconds(long.Parse(match.Groups[1].Value));
                dt = dt.AddHours(long.Parse(match.Groups[2].Value));
                return dt;
            }
            return DateTime.MinValue;
        }
        */
    }
}
