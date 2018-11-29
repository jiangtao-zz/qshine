using System;
using System.Collections.Generic;

namespace qshine
{
	/// <summary>
	/// Json provider interface.
	/// </summary>
	public interface IJsonProvider:IProvider
	{
		/// <summary>
		/// Create a json serializer instance.
		/// </summary>
		/// <returns>The json serializer.</returns>
		IJsonSerializer Create();
	}

	/// <summary>
	/// Json serializer interface.
	/// </summary>
	public interface IJsonSerializer
	{
        /// <summary>
        /// Serialize the specified instance to formatted json string.
        /// </summary>
        /// <returns>The serialized json string.</returns>
        /// <param name="instance">Instance of object</param>
        /// <param name="jsonFormat">Json format.</param>
        /// <param name="setting">Json custom format.</param>
        string Serialize(object instance, JsonFormat jsonFormat, JsonFormatSetting setting);
        /// <summary>
        /// Deserialize a specified formatted jsonString to a typed object.
        /// </summary>
        /// <returns>The deserialized object.</returns>
        /// <param name="jsonString">Json string.</param>
        /// <param name="type">Type of the result object.</param>
        /// <param name="jsonFormat">Json format.</param>
        /// <param name="setting">Json custom format.</param>
        object Deserialize(string jsonString, Type type, JsonFormat jsonFormat, JsonFormatSetting setting);

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
        /// <param name="setting">Json custom format.</param>
        /// <returns>Dictionary instance</returns>
        Dictionary<string, object> DeserializeDictionary(string jsonString, JsonFormat jsonFormat, JsonFormatSetting setting);
    }
}
