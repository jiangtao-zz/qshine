using System;
namespace qshine
{
	public class DefaultCommandBusFactory:ICommandBusFactory
	{

		public ICommandBus Create()
		{
			return new NetCommandBus();
		}
	}
}
