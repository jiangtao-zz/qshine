using System.Configuration;

namespace qshine.Configuration.Setting
{
    /// <summary>
    /// Represents a named <see cref="ConfigurationElement"/> where the name is the key to a collection.
    /// </summary>
    public class NamedConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Name of the property that holds the name of <see cref="NamedConfigurationElement"/>.
        /// </summary>
        private const string nameProperty = "name";

        /// <summary>
        /// Initialize a new instance of a <see cref="NamedConfigurationElement"/> class.
        /// </summary>
        public NamedConfigurationElement()
        { }

        /// <summary>
        /// Intialize a new instance of a <see cref="NamedConfigurationElement"/> class with a name.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        public NamedConfigurationElement(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the element.
        /// </summary>
        /// <value>
        /// The name of the element.
        /// </value>
        [ConfigurationProperty(nameProperty, IsKey = true, DefaultValue = "Name", IsRequired = true)]
        [StringValidator(MinLength = 1)]
        public string Name
        {
            get { return (string)this[nameProperty]; }
            set { this[nameProperty] = value; }
        }
    }
}