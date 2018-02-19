using NUnit.Framework;
using qshine.Configuration;

namespace qshine.UnitTests
{
	[TestFixture()]
	public class EnvironmentManagerTests
	{
		[Test()]
		public void LoadConfig_Default_File()
		{
			var configure = EnvironmentManager.Current.EnvironmentConfigure;

			Assert.AreEqual("../../mySystemFolder/config",configure.Environments["top"].Path);
			Assert.AreEqual("",configure.Environments["top"].Host);
			Assert.AreEqual("mySystemFolder/QA_config",configure.Environments["qa"].Path);
			Assert.AreEqual("202.22.22.22",configure.Environments["qa"].Host);
			Assert.AreEqual("mySystemFolder/UA_config",configure.Environments["ua"].Path);
			Assert.AreEqual("key1 value",configure.AppSettings["key1"]);
			Assert.AreEqual("level0 key0 value",configure.AppSettings["key0"]);

			Assert.IsTrue(configure.ConfigureFolders.Count > 0);
			Assert.IsTrue(configure.AssemblyFolders.Count > 0);

			Assert.IsTrue(EnvironmentManager.Current.AssemblyMaps["log4net"].Path.Contains("log4net.dll"));

		}
	}
}
