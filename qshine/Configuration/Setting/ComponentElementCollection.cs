using System;
namespace qshine.Configuration
{
	public class ComponentElementCollection : ConfigurationElementCollection<ComponentElement>
	{
		public ComponentElementCollection()
			: base("component")
		{
		}
	}
}
