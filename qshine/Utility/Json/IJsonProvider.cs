using System;
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
		string Serialize(object instance, JsonFormat jsonFormat);
		/// <summary>
		/// Deserialize a specified formatted jsonString to a typed object.
		/// </summary>
		/// <returns>The deserialized object.</returns>
		/// <param name="jsonString">Json string.</param>
		/// <param name="type">Type of the result object.</param>
		/// <param name="jsonFormat">Json format.</param>
		object Deserialize(string jsonString, Type type, JsonFormat jsonFormat);
	}
}
