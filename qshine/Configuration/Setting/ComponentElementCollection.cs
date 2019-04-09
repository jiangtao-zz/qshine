using System;
namespace qshine.Configuration.Setting
{
    /// <summary>
    /// Component element collection
    /// </summary>
    public class ComponentElementCollection : ConfigurationElementCollection<ComponentElement>
	{
        /// <summary>
        /// Ctro.
        /// </summary>
		public ComponentElementCollection()
			: base("component")
		{
		}
	}
}
