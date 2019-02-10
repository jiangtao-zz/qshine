﻿using System.Configuration;
using qshine.Configuration.Setting;

namespace qshine.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class EnvironmentSection : ConfigurationSection
	{
		public const string EnvironmentsAttributeName = "environments";
		public const string ComponentsAttributeName = "components";
		public const string ModulesAttributeName = "modules";
		public const string AppSettingsAttributeName = "appSettings";
        public const string MapsAttributeName = "maps";


        [ConfigurationProperty(EnvironmentsAttributeName)]
		[ConfigurationCollection(typeof(EnvironmentElementCollection))]
		public EnvironmentElementCollection Environments
		{
			get { return ((EnvironmentElementCollection)(base[EnvironmentsAttributeName])); }
		}
		/// <summary>
		/// Gets the components.
		/// </summary>
		/// <value>The components.</value>
		[ConfigurationProperty(ComponentsAttributeName)]
		[ConfigurationCollection(typeof(ComponentElementCollection))]
		public ComponentElementCollection Components
		{
			get { return ((ComponentElementCollection)(base[ComponentsAttributeName])); }
		}

		[ConfigurationProperty(ModulesAttributeName)]
		[ConfigurationCollection(typeof(ModuleElementCollection))]
		public ModuleElementCollection Modules
		{
			get { return ((ModuleElementCollection)(base[ModulesAttributeName])); }
		}

		[ConfigurationProperty(AppSettingsAttributeName)]
		[ConfigurationCollection(typeof(KeyValueElement), AddItemName = "add",  RemoveItemName = "remove", ClearItemsName = "clear")]
		public KeyValueElementCollection<KeyValueElement> AppSettings 
		{
			get { return (KeyValueElementCollection<KeyValueElement>)base[AppSettingsAttributeName]; } 
		}

        [ConfigurationProperty(MapsAttributeName)]
        [ConfigurationCollection(typeof(NamedKeyValueElementCollection), AddItemName = "add", RemoveItemName = "remove", ClearItemsName = "clear")]
        public NamedKeyValueElementCollection Maps
        {
            get { return (NamedKeyValueElementCollection)base[MapsAttributeName]; }
        }
    }

}
