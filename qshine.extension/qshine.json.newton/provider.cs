using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace qshine.json.newton
{
    public class Provider : IJsonProvider
    {
        public IJsonSerializer Create()
        {
            return new JsonSerializer();
        }
    }

    public class JsonSerializer : IJsonSerializer
    {
        /// <summary>
        /// Serialize object to json string
        /// </summary>
        /// <param name="instance">object instance</param>
        /// <param name="jsonFormat">Json format options</param>
        /// <returns></returns>
        public string Serialize(object instance, JsonFormat jsonFormat, JsonFormatSetting jsonFormatSetting)
        {
            return JsonConvert.SerializeObject(instance, ConvertFormat(jsonFormat, jsonFormatSetting));
        }

        /// <summary>
        /// Deserialize json string to typed object.
        /// </summary>
		/// <param name="jsonString">Json string.</param>
		/// <param name="type">Type of the instance.</param>
        /// <param name="jsonFormat">Json format options</param>
        /// <returns>returns converted typed object</returns>
        public object Deserialize(string jsonString, Type type, JsonFormat jsonFormat, JsonFormatSetting jsonFormatSetting)
        {
            return JsonConvert.DeserializeObject(jsonString, type, ConvertFormat(jsonFormat, jsonFormatSetting));
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
        /// <returns>Dictionary instance</returns>
        public Dictionary<string, object> DeserializeDictionary(string jsonString, JsonFormat jsonFormat, JsonFormatSetting jsonFormatSetting)
        {
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString, ConvertFormat(jsonFormat, jsonFormatSetting));
            if (dictionary!=null)
            {
                return ConvertToJsonDictionary(jsonString, jsonFormat, jsonFormatSetting);
            }
            return new Dictionary<string, object>();
        }

        Dictionary<string, object> ConvertToJsonDictionary(string jsonString, JsonFormat jsonFormat, JsonFormatSetting jsonFormatSetting)
        {
            Dictionary<string, object> input = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString, ConvertFormat(jsonFormat, jsonFormatSetting));
            Dictionary<string, object> result = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> d in input)
            {
                // if (d.Value.GetType().FullName.Contains("Newtonsoft.Json.Linq.JObject"))
                if (d.Value is JObject)
                {
                    result.Add(d.Key, ConvertToJsonDictionary(d.Value.ToString(), jsonFormat, jsonFormatSetting));
                }
                else if(d.Value is JArray)
                {
                    //convert "item" array to dictionary when using "UseSimpleDictionaryFormat = false"
                    var dic = d.Value as JArray;
                    if (dic != null && dic.Count>0 && dic[0].Count()==2 && dic[0]["Key"]!=null && dic[0]["Value"] != null)
                    {

                        result.Add(d.Key, ConvertToKeyValueDictionary(dic));
                    }
                    else
                    {
                        result.Add(d.Key, d.Value);
                    }
                }
                else
                {
                    result.Add(d.Key, d.Value);
                }
            }
            return result;
        }

        private Dictionary<string, object> ConvertToKeyValueDictionary(JArray dic)
        {
            var result = new Dictionary<string, object>();
            foreach(JObject dv in dic)
            {
                string key = (string)dv["Key"];
                var v = dv["Value"] as JValue;
                object value = v;
                if (v != null)
                {
                    value = v.Value;
                }
                result.Add(key, value);
            }
            return result;
        }

        private JsonSerializerSettings ConvertFormat(JsonFormat formatOption, JsonFormatSetting jsonFormatSetting)
        {
            JsonSerializerSettings setting = new JsonSerializerSettings();

            if ((formatOption & JsonFormat.MicrosoftDateFormat) == JsonFormat.MicrosoftDateFormat)
            {
                setting.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            }
            else if ((formatOption & JsonFormat.ISO8601DateFormat) == JsonFormat.ISO8601DateFormat)
            {
                // The ISO 8601 date format.
                // 	2012-04-23T18:25:43 (Unspecified)
                // 	2012-04-23T18:25:43Z (Utc)
                // 	2012-04-23T18:25:43-05:00 (Local)
                var converter = new IsoDateTimeConverter()
                {
                    DateTimeFormat= "yyyy-MM-ddTHH:mm:ssK"
                };

                setting.Converters.Add(converter);
            }
            else if ((formatOption & JsonFormat.ISO8601RoundtripDateFormat) == JsonFormat.ISO8601RoundtripDateFormat)
            {
                // The ISO 8601 roundtrip date format.
                // 	2012-04-23T18:25:43.1234567 (Unspecified)
                // 	2012-04-23T18:25:43.1234567Z, (UTC)
                //	2012-04-23T18:25:43.1234567-05:00 (local)
                var converter = new IsoDateTimeConverter()
                {
                    DateTimeFormat = "o"
                };
                setting.Converters.Add(converter);
            }
            else if ((formatOption & JsonFormat.ISO8601JavascriptDateFormat) == JsonFormat.ISO8601JavascriptDateFormat)
            {
                // The ISO 8601 javascript JSON.stringify date format.
                // 	2012-04-23T18:25:43.123 (Unspecified)
                //	2012-04-23T18:25:43.007Z, (Utc)
                // 	2012-04-23T18:25:43.007-05:00, (local)
                setting.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                setting.Converters.Add(new IsoDateTimeConverter());
            }
            else if ((formatOption & JsonFormat.JavascriptDateFormat) == JsonFormat.JavascriptDateFormat)
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
                setting.Converters.Add(new JavaScriptDateTimeConverter());
            }
            else if (((ulong)formatOption & 0xFF) == (ulong)JsonFormat.CustomDateFormat)
            {
                string dateFormat = ((jsonFormatSetting != null) 
                    && !string.IsNullOrEmpty(jsonFormatSetting.DateTimeFormat)) 
                    ? jsonFormatSetting.DateTimeFormat : "s";

                var converter = new IsoDateTimeConverter()
                {
                    DateTimeFormat = dateFormat
                };
                setting.Converters.Add(converter);
            }

            return setting;
        }
    }
}
