using System.Configuration;
namespace qshine.Configuration.Setting
{
    /// <summary>
    /// Key valye element
    /// </summary>
	public class KeyValueElement: ConfigurationElement
	{
		const string KeyAttributeName = "key";
		const string ValueAttributeName = "value";

		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>The key.</value>
		[ConfigurationProperty(KeyAttributeName, IsRequired = true)]
        public string Key
		{
			get { return (string)this[KeyAttributeName]; }
			set { this[KeyAttributeName] = value; }
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		[ConfigurationProperty(ValueAttributeName, IsRequired = false)]
		public string Value
		{
			get { return (string)this[ValueAttributeName]; }
			set { this[ValueAttributeName] = value; }
		}

	}
}
