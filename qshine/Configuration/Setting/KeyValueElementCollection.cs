using System.Configuration;
namespace qshine.Configuration.Setting
{
    /// <summary>
    /// Key value element collection.
    /// </summary>
    public class KeyValueElementCollection<T> : ConfigurationElementCollection<T>
	where T : KeyValueElement, new()
	{
        /// <summary>
        /// Ctro.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			T namedElement = (T)element;
			return namedElement.Key;
		}

	}

}
