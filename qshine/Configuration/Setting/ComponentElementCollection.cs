using System;
namespace qshine.Configuration.Setting
{
    public class ComponentElementCollection : ConfigurationElementCollection<ComponentElement>
	{
		public ComponentElementCollection()
			: base("component")
		{
		}
	}
}
