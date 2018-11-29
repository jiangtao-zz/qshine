using System.Configuration;

namespace qshine.Configuration.Setting
{
    /// <summary>
    /// name/value/type pair configuration element
	/// 
	/// <![CDATA[
	/// 	&lt;add name="abc" value="xyz" type="o.b.c" /&gt;
	/// ]]>
    /// </summary>
    public class NamedValueElement : NamedConfigurationElement
    {
        const string ValueAttributeName = "value";
		const string TypeAttributeName = "type";

        /// <summary>
        /// Define a value attribute for a named element <see cref="NamedValueElement"/>.
        /// </summary>
        [ConfigurationProperty(ValueAttributeName)]
        public string Value
        {
            get { return (string)this[ValueAttributeName]; }
            set { this[ValueAttributeName] = value; }
        }

		/// <summary>
		/// Define a type attribute for a named element <see cref="NamedValueElement"/>.
		/// </summary>
		[ConfigurationProperty(TypeAttributeName, IsRequired = false)]
		public string Type
		{
			get { return (string)this[TypeAttributeName]; }
			set { this[TypeAttributeName] = value; }
		}
    }
}
