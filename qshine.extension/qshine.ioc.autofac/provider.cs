using System;
using qshine.IoC;
namespace qshine.ioc.autofac
{
	public class Provider:IIoCProvider
	{
		public IIoCContainer CreateContainer()
		{
			return new Container();
		}
	}
}
