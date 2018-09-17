using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Configuration;
using qshine;

namespace qshine.UnitTests
{
	[TestClass()]
	public class EnvironmentManagerTests
	{
		[TestMethod()]
		public void LoadConfig_Default_File()
		{
			Log.SysLogger.Info("LoadConfig_Default_File Start..");
			EnvironmentManager.Boot();
			var configure = EnvironmentManager.Configure;

			Assert.AreEqual("config", configure.Environments["root"].Path);
			Assert.AreEqual("", configure.Environments["root"].Host);
			Assert.AreEqual("TestServerFolder/QA_config", configure.Environments["qa"].Path);
			Assert.AreEqual("202.22.22.22", configure.Environments["qa"].Host);
			Assert.AreEqual("TestServerFolder/UA_config", configure.Environments["ua"].Path);
			Assert.AreEqual("key1 value", configure.AppSettings["key1"]);
			Assert.AreEqual("level0 key0 value", configure.AppSettings["key0"]);

			Assert.IsTrue(configure.ConfigureFolders.Count > 0);
			Assert.IsTrue(configure.AssemblyFolders.Count > 0);

            Assert.IsTrue(EnvironmentManager.AssemblyMaps["qshine.ioc.autofac"].Path.Contains("qshine.ioc.autofac.dll"));
            Assert.IsTrue(EnvironmentManager.AssemblyMaps["Autofac"].Path.Contains("Autofac.dll"));
            Assert.IsTrue(EnvironmentManager.AssemblyMaps["qshine.log.nlog"].Path.Contains("qshine.log.nlog.dll"));
            Assert.IsTrue(EnvironmentManager.AssemblyMaps["NLog"].Path.Contains("NLog.dll"));

            Log.SysLogger.Info("LoadConfig_Default_File End");

		}
		[TestMethod()]
		public void App_init()
		{
			EnvironmentManager.Boot();
			var iocProvider = EnvironmentManager.GetProvider<IIocProvider>();
			Assert.IsNotNull(iocProvider);
		}

		[TestMethod()]
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
