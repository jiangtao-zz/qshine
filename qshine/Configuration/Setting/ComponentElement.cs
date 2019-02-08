using System.Configuration;

namespace qshine.Configuration.Setting
{
	public class ComponentElement : NamedConfigurationElement
	{
		const string InterfaceTypeName = "interface";
		const string TypeAttributeName = "type";
		const string ScopeAttributeName = "scope";
		const string ParametersAttributeName = "parameters";
        const string DefaultAttributeName = "default";

        /// <summary>
        /// Gets or sets the type of the interface.
        /// </summary>
        /// <value>The type of the interface.</value>
        [ConfigurationProperty(InterfaceTypeName)]
		public string InterfaceType
		{
			get { return (string)this[InterfaceTypeName]; }
			set { this[InterfaceTypeName] = value; }
		}

		/// <summary>
		/// Gets or sets the type.
		/// </summary>
		/// <value>The type.</value>
		[ConfigurationProperty(TypeAttributeName)]
		public string Type
		{
			get { return (string)this[TypeAttributeName]; }
			set { this[TypeAttributeName] = value; }
		}

		/// <summary>
		/// Gets or sets the scope.
		/// </summary>
		/// <value>The scope.</value>
		[ConfigurationProperty(ScopeAttributeName)]
		public string Scope
		{
			get { return (string)this[ScopeAttributeName]; }
			set { this[ScopeAttributeName] = value; }
		}

		/// <summary>
		/// Gets the parameters.
		/// </summary>
		/// <value>The parameters.</value>
		[ConfigurationProperty(ParametersAttributeName)]
		public ParameterElementCollection Parameters
		{
			get { return (ParameterElementCollection)base[ParametersAttributeName];}
		}

        [ConfigurationProperty(ParametersAttributeName)]
        public bool Default
        {
            get { return (bool)this[DefaultAttributeName]; }
            set { this[DefaultAttributeName] = value; }
        }
    }
}
