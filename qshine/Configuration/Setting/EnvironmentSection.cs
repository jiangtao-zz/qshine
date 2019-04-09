using System.Configuration;
using qshine.Configuration.Setting;

namespace qshine.Configuration
{
    /// <summary>
    /// configuration environment section
    /// </summary>
    public class EnvironmentSection : ConfigurationSection
	{
        /// <summary>
        /// environment attribute name
        /// </summary>
		internal const string EnvironmentsAttributeName = "environments";
        /// <summary>
        /// component attribute name
        /// </summary>
		internal const string ComponentsAttributeName = "components";
        /// <summary>
        /// module attribute name
        /// </summary>
		internal const string ModulesAttributeName = "modules";
        /// <summary>
        /// appsetting attribute name
        /// </summary>
		internal const string AppSettingsAttributeName = "appSettings";
        /// <summary>
        /// map attribute name
        /// </summary>
        internal const string MapsAttributeName = "maps";

        /// <summary>
        /// Get the environment collection
        /// </summary>
        [ConfigurationProperty(EnvironmentsAttributeName)]
		[ConfigurationCollection(typeof(EnvironmentElementCollection))]
		public EnvironmentElementCollection Environments
		{
			get { return ((EnvironmentElementCollection)(base[EnvironmentsAttributeName])); }
		}

		/// <summary>
		/// Gets the component collection
		/// </summary>
		/// <value>The components.</value>
		[ConfigurationProperty(ComponentsAttributeName)]
		[ConfigurationCollection(typeof(ComponentElementCollection))]
		public ComponentElementCollection Components
		{
			get { return ((ComponentElementCollection)(base[ComponentsAttributeName])); }
		}

        /// <summary>
        /// Get the module collection
        /// </summary>
		[ConfigurationProperty(ModulesAttributeName)]
		[ConfigurationCollection(typeof(ModuleElementCollection))]
		public ModuleElementCollection Modules
		{
			get { return ((ModuleElementCollection)(base[ModulesAttributeName])); }
		}

        /// <summary>
        /// Get the appsetting collection
        /// </summary>
		[ConfigurationProperty(AppSettingsAttributeName)]
		[ConfigurationCollection(typeof(KeyValueElement), AddItemName = "add",  RemoveItemName = "remove", ClearItemsName = "clear")]
		public KeyValueElementCollection<KeyValueElement> AppSettings 
		{
			get { return (KeyValueElementCollection<KeyValueElement>)base[AppSettingsAttributeName]; } 
		}

        /// <summary>
        /// Get the map collection
        /// </summary>
        [ConfigurationProperty(MapsAttributeName)]
        [ConfigurationCollection(typeof(NamedKeyValueElementCollection), AddItemName = "add", RemoveItemName = "remove", ClearItemsName = "clear")]
        public NamedKeyValueElementCollection Maps
        {
            get { return (NamedKeyValueElementCollection)base[MapsAttributeName]; }
        }
    }

}
