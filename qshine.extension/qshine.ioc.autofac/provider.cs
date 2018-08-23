using System;
using qshine;
namespace qshine.ioc.autofac
{
	public class Provider:IIocProvider
	{
		public IIocContainer CreateContainer()
		{
			return new Container();
		}
	}
}
