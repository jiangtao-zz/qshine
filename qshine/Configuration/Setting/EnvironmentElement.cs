using System.Configuration;

namespace qshine.Configuration
{
	public class EnvironmentElement : NamedConfigurationElement
	{
		const string PathAttributeName = "path";
		const string BinAttributeName = "bin";
		const string HostAttributeName = "host";

		/// <summary>
		/// Gets or sets the config.
		/// </summary>
		/// <value>The config.</value>
		[ConfigurationProperty(PathAttributeName)]
		public string Path
		{
			get { return (string)this[PathAttributeName]; }
			set { this[PathAttributeName] = value; }
		}

		/// <summary>
		/// Gets or sets the bin.
		/// </summary>
		/// <value>The bin.</value>
		[ConfigurationProperty(BinAttributeName)]
		public string Bin
		{
			get { return (string)this[BinAttributeName]; }
			set { this[BinAttributeName] = value; }
		}

		/// <summary>
		/// Gets or sets the host.
		/// </summary>
		/// <value>The host.</value>
		[ConfigurationProperty(HostAttributeName)]
		public string Host
		{
			get { return (string)this[HostAttributeName]; }
			set { this[HostAttributeName] = value; }
		}

}
}
