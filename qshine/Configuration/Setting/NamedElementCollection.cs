using System.Configuration;

namespace qshine.Configuration
{
	/// <summary>
	/// Represents a collection of <see cref="NamedConfigurationElement"/> objects.
	/// </summary>
	/// <typeparam name="T">A newable object that inherits from <see cref="NamedConfigurationElement"/>.</typeparam>
	public class NamedElementCollection<T> : ConfigurationElementCollection<T>
		where T : NamedConfigurationElement, new()
	{
		/// <summary>
		/// Gets the element key for a specified configuration element when overridden in a derived class. 
		/// </summary>
		/// <param name="element">The <see cref="ConfigurationElement"/> to return the key for. </param>
		/// <returns>An <see cref="object"/> that acts as the key for the specified <see cref="ConfigurationElement"/>.</returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			T namedElement = (T)element;
			return namedElement.Name;
		}

	}
}
