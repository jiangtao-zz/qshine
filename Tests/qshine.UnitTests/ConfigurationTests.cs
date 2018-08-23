using NUnit.Framework;
using qshine.Configuration;
using qshine;

namespace qshine.UnitTests
{
	[TestFixture()]
	public class EnvironmentManagerTests
	{
		[Test()]
		public void LoadConfig_Default_File()
		{
			Log.SysLogger.Info("LoadConfig_Default_File Start..");
			EnvironmentManager.Boot();
			var configure = EnvironmentManager.Configure;

			Assert.AreEqual("..\\..\\mySystemFolder\\config", configure.Environments["root"].Path);
			Assert.AreEqual("", configure.Environments["root"].Host);
			Assert.AreEqual("mySystemFolder/QA_config", configure.Environments["qa"].Path);
			Assert.AreEqual("202.22.22.22", configure.Environments["qa"].Host);
			Assert.AreEqual("mySystemFolder/UA_config", configure.Environments["ua"].Path);
			Assert.AreEqual("key1 value", configure.AppSettings["key1"]);
			Assert.AreEqual("level0 key0 value", configure.AppSettings["key0"]);

			Assert.IsTrue(configure.ConfigureFolders.Count > 0);
			Assert.IsTrue(configure.AssemblyFolders.Count > 0);

			Assert.IsTrue(EnvironmentManager.AssemblyMaps["log4net"].Path.Contains("log4net.dll"));
			Assert.IsTrue(EnvironmentManager.AssemblyMaps["Autofac"].Path.Contains("Autofac.dll"));

			Log.SysLogger.Info("LoadConfig_Default_File End");

		}
		[Test()]
		public void App_init()
		{
			EnvironmentManager.Boot();
			var iocProvider = EnvironmentManager.GetProvider<IIocProvider>();
			Assert.IsNotNull(iocProvider);
		}

		[Test()]
		public void ConnectionStrings()
		{
			EnvironmentManager.Boot();
			var db1 = EnvironmentManager.Configure.ConnectionStrings["db1"];
			Assert.AreEqual("testProvider",db1.ProviderName);
			Assert.AreEqual("abc,001",db1.ConnectionString);

			var db2 = EnvironmentManager.Configure.ConnectionStrings["db2"];
			Assert.AreEqual("testProvider2",db2.ProviderName);
			Assert.AreEqual("abc2,aaa",db2.ConnectionString);

			var db3 = EnvironmentManager.Configure.ConnectionStrings["db3"];
			Assert.IsNull(db3);
		}

	}
}
