using System.Configuration;
namespace qshine.Configuration.Setting
{
    /// <summary>
    /// Key value element collection.
    /// </summary>
    public class KeyValueElementCollection<T> : ConfigurationElementCollection<T>
	where T : KeyValueElement, new()
	{
		protected override object GetElementKey(ConfigurationElement element)
		{
			T namedElement = (T)element;
			return namedElement.Key;
		}

	}

}
