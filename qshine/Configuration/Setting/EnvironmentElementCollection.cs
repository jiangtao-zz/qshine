namespace qshine.Configuration.Setting
{
    /// <summary>
    /// application environment element collection.
    /// </summary>
    public class EnvironmentElementCollection : ConfigurationElementCollection<EnvironmentElement>
	{
        /// <summary>
        /// Ctro.
        /// </summary>
		public EnvironmentElementCollection()
			: base("environment")
		{
		}
	}
}
