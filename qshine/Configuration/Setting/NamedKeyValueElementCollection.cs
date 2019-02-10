using System.Configuration;
namespace qshine.Configuration.Setting
{
    /// <summary>
    /// Key value element collection.
    /// </summary>
    public class NamedKeyValueElementCollection : ConfigurationElementCollection<KeyValueElement>
    {
        protected override object GetElementKey(ConfigurationElement element)
        {
            KeyValueElement namedElement = (KeyValueElement)element;
            return namedElement.Key;
        }

        const string nameProperty = "name";
        const string defaultProperty = "default";
        /// <summary>
        /// Gets the name of the element.
        /// </summary>
        [ConfigurationProperty(nameProperty, IsRequired = false)]
        public string Name
        {
            get {
                return (string)CollectionProperty(nameProperty);
            }
        }

        /// <summary>
        /// Gets the default property.
        /// </summary>
        [ConfigurationProperty(defaultProperty, IsRequired = false)]
        public string Default
        {
            get
            {
                return (string)CollectionProperty(defaultProperty);
            }
        }
    }

}
