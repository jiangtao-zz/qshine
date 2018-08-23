using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace qshine
{
	public class DotNetJsonSerializerProvider : IJsonProvider
	{
		public IJsonSerializer Create()
		{
			return new DotNetJsonSerializer();
		}
	}

	public class DotNetJsonSerializer:IJsonSerializer
	{
		/// <summary>
		/// Deserialize the specified jsonString to a typed object.
		/// </summary>
		/// <returns>The deserialized object</returns>
		/// <param name="jsonString">Json string.</param>
		/// <param name="type">Type of the instance.</param>
		public object Deserialize(string jsonString, Type type, JsonFormat jsonFormat)
		{
			object result = null;
            if (!string.IsNullOrEmpty(jsonString) && type != null)
            {
				var serializer = new DataContractJsonSerializer(type, ConvertFormat(jsonFormat));
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
		public string Serialize(object instance, JsonFormat jsonFormat)
		{
			string jsonString = null;
			var serializer = new DataContractJsonSerializer(instance.GetType(), ConvertFormat(jsonFormat));
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, instance);
                jsonString = Encoding.UTF8.GetString(ms.ToArray());
            }
			return jsonString;
		}

		private static DataContractJsonSerializerSettings ConvertFormat(JsonFormat formatOption)
		{
			var format = new DataContractJsonSerializerSettings
			{
				EmitTypeInformation = EmitTypeInformation.Never,
				UseSimpleDictionaryFormat = true
			};

			if (((ulong)formatOption & 0xFF) == (ulong)JsonFormat.ISO8601DateFormat)
			{
				format.DateTimeFormat = new DateTimeFormat("yyyy-MM-ddTHH:mm:ssK");//"yyyy-MM-ddTHH:mm:ssZ");
			}
			else if (((ulong)formatOption & 0xFF) == (ulong)JsonFormat.ISO8601RoundtripDateFormat)
			{
				format.DateTimeFormat = new DateTimeFormat("o");//"yyyy-MM-dd'T'HH:mm:ss.fffffffZ");
			}
			else if (((ulong)formatOption & 0xFF) == (ulong)JsonFormat.ISO8601JavascriptDateFormat)
			{
				format.DateTimeFormat = new DateTimeFormat("yyyy-MM-ddTHH:mm:ss.fffK");
			}
			return format;
		}

	}
}
