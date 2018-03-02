using NUnit.Framework;
using System;
namespace qshine.ioc.autofac.UnitTests
{
	[TestFixture()]
	public class Test
	{
		[Test()]
		public void provider_created()
		{
			var provider = new qshine.ioc.autofac.Provider();
			var container = provider.CreateContainer();
			Assert.IsNotNull(provider);
			Assert.IsNotNull(container);
		}
	}
}
