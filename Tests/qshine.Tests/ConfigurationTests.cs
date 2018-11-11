using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Configuration;
using System;
using System.Reflection;

namespace qshine.UnitTests
{
    [TestClass]
    public class SetupAssemblyInitializer
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            
            Log.SysLoggerProvider = new TraceLoggerProvider();
            Log.SysLogger.EnableLogging(System.Diagnostics.TraceEventType.Verbose);
            
            //This is only running once. Ignore subsequently call ApplicationEnvironment.Boot().
            ApplicationEnvironment.Build("app.config");
        }
    }

    [TestClass()]
	public class EnvironmentManagerTests
	{
        [TestMethod()]
		public void LoadConfig_Default_File()
		{
			var configure = ApplicationEnvironment.Configure;

			Assert.AreEqual("config", configure.Environments["root"].Path);
			Assert.AreEqual("", configure.Environments["root"].Host);
			Assert.AreEqual("TestServerFolder/QA_config", configure.Environments["qa"].Path);
			Assert.AreEqual("202.22.22.22", configure.Environments["qa"].Host);
			Assert.AreEqual("TestServerFolder/UA_config", configure.Environments["ua"].Path);
			Assert.AreEqual("key1 value", configure.AppSettings["key1"]);
			Assert.AreEqual("level0 key0 value", configure.AppSettings["key0"]);
            Assert.AreEqual("moduleTest", configure.Modules["moduleTest"].Name);
            Assert.AreEqual("qshine.NHibernateRepository.BootStraper, QShine.NHibernateRepository", configure.Modules["moduleTest"].Type);
            Assert.IsFalse(configure.Modules.ContainsKey("xyz"));


            Assert.IsTrue(configure.ConfigureFolders.Count > 0);
			Assert.IsTrue(configure.AssemblyFolders.Count > 0);

            Assert.IsTrue(ApplicationEnvironment.AssemblyMaps["qshine.ioc.autofac"].Path.Contains("qshine.ioc.autofac.dll"));
            Assert.IsTrue(ApplicationEnvironment.AssemblyMaps["Autofac"].Path.Contains("Autofac.dll"));
            Assert.IsTrue(ApplicationEnvironment.AssemblyMaps["qshine.log.nlog"].Path.Contains("qshine.log.nlog.dll"));
            Assert.IsTrue(ApplicationEnvironment.AssemblyMaps["NLog"].Path.Contains("NLog.dll"));

            Log.SysLogger.Info("LoadConfig_Default_File End");

		}
		[TestMethod()]
		public void App_init()
		{
			var iocProvider = ApplicationEnvironment.GetProvider<IIocProvider>();
			Assert.IsNotNull(iocProvider);
		}

		[TestMethod()]
		public void ConnectionStrings()
		{
            var db1 = ApplicationEnvironment.Configure.ConnectionStrings["db1"];
            Assert.AreEqual("testProvider",db1.ProviderName);
            Assert.AreEqual("abc,002",db1.ConnectionString);

            var db2 = ApplicationEnvironment.Configure.ConnectionStrings["db2"];
            Assert.AreEqual("testProvider2",db2.ProviderName);
            Assert.AreEqual("abc2,aaa",db2.ConnectionString);

            var db3 = ApplicationEnvironment.Configure.ConnectionStrings["db3"];
            Assert.IsNull(db3);
            
        }

        [TestMethod()]
        public void TargetFramework_Test()
        {
            var targetFramework = EnvironmentEx.TargetFramework;
            Assert.IsTrue(!string.IsNullOrEmpty(targetFramework));
        }

        [TestMethod()]
        public void EnvironmentInitializationOption_overwrite_Test()
        {
            var option = new EnvironmentInitializationOption
            {
                RootConfigFile = "config/unitTest/test_app1.config",
                OverwriteConnectionString = false
            };

            var appEnv = new ApplicationEnvironment("c1", option);

            //Test connectionStrings
            Assert.IsTrue(appEnv.EnvironmentConfigure.ConnectionStrings.Count>=3);
            Assert.AreEqual("dbt1,001", appEnv.EnvironmentConfigure.ConnectionStrings["dbTest1"].ConnectionString);
            Assert.AreEqual("dbt2,002", appEnv.EnvironmentConfigure.ConnectionStrings["dbTest2"].ConnectionString);

            //Test AppSettings
            Assert.AreEqual("key0 value", appEnv["key0"]);
            Assert.AreEqual("key1 value", appEnv["key1"]);
            Assert.AreEqual("key2 value", appEnv["key2"]);

            //Test modules
            Assert.AreEqual("M1.testClass, M1", appEnv.EnvironmentConfigure.Modules["moduleTest1"].Type);
            Assert.AreEqual("M1.testClass, M1", appEnv.EnvironmentConfigure.Modules["moduleTest2"].Type);

            //Test components
            Assert.AreEqual("c1.testInterface", appEnv.EnvironmentConfigure.Components["c1"].InterfaceTypeName);
            Assert.AreEqual("c1.testClass, c1.test", appEnv.EnvironmentConfigure.Components["c1"].ClassTypeName);
            Assert.AreEqual("Singleton", appEnv.EnvironmentConfigure.Components["c1"].Scope);
            Assert.AreEqual("c2.testInterface", appEnv.EnvironmentConfigure.Components["c2"].InterfaceTypeName);
            Assert.AreEqual("c2.testClass, c2.test", appEnv.EnvironmentConfigure.Components["c2"].ClassTypeName);
            Assert.AreEqual("Transient", appEnv.EnvironmentConfigure.Components["c2"].Scope);


            option.OverwriteConnectionString = true;
            option.OverwriteAppSetting = true;
            option.OverwriteModule = true;
            option.OverwriteComponent = true;

            var appEnv1 = new ApplicationEnvironment("c2", option);

            //Test connectionStrings overwrite
            Assert.AreEqual(appEnv.EnvironmentConfigure.ConnectionStrings.Count, 
                appEnv1.EnvironmentConfigure.ConnectionStrings.Count);
            Assert.AreEqual("dbt1,001_overwrite", appEnv1.EnvironmentConfigure.ConnectionStrings["dbTest1"].ConnectionString);
            Assert.AreEqual("dbt2,002", appEnv1.EnvironmentConfigure.ConnectionStrings["dbTest2"].ConnectionString);
            Assert.AreEqual("dbt3,003", appEnv1.EnvironmentConfigure.ConnectionStrings["dbTest3"].ConnectionString);

            //Test AppSettings overwrite
            Assert.AreEqual("key0 value_overwrite", appEnv1["key0"]);
            Assert.AreEqual("key1 value", appEnv1["key1"]);
            Assert.AreEqual("key2 value", appEnv1["key2"]);

            //Test modules
            Assert.AreEqual("M1.testClass, M1_overwrite", appEnv1.EnvironmentConfigure.Modules["moduleTest1"].Type);
            Assert.AreEqual("M1.testClass, M1", appEnv1.EnvironmentConfigure.Modules["moduleTest2"].Type);

            //Test components
            Assert.AreEqual("c1.testInterface_overwrite", appEnv1.EnvironmentConfigure.Components["c1"].InterfaceTypeName);
            Assert.AreEqual("c1.testClass_1, c1.test", appEnv1.EnvironmentConfigure.Components["c1"].ClassTypeName);
            Assert.AreEqual("Transient", appEnv1.EnvironmentConfigure.Components["c1"].Scope);
            Assert.AreEqual("c2.testInterface", appEnv1.EnvironmentConfigure.Components["c2"].InterfaceTypeName);
            Assert.AreEqual("c2.testClass, c2.test", appEnv1.EnvironmentConfigure.Components["c2"].ClassTypeName);
            Assert.AreEqual("Transient", appEnv1.EnvironmentConfigure.Components["c2"].Scope);
        }

        [TestMethod()]
        public void EnvironmentInitializationOption_Configuration_Test()
        {
            // Get the current configuration file.
            System.Configuration.Configuration config = 
                System.Configuration.ConfigurationManager.OpenMachineConfiguration();

            var option = new EnvironmentInitializationOption
            {
                RootConfigFile = "config/unitTest/test_app1.config",//this line will be ignored
                RootConfiguration = config
            };

            var appEnv = new ApplicationEnvironment("c3", option);

            Assert.IsTrue(appEnv.EnvironmentConfigure.ConnectionStrings["dbTest1"]==null);

            Assert.AreEqual(0, appEnv.EnvironmentConfigure.Modules.Count);
            Assert.AreEqual(0, appEnv.EnvironmentConfigure.Components.Count);
            Assert.AreEqual("",appEnv["key0"]);
        }

        [TestMethod()]
        public void EnvironmentInitializationOption_IsCandidateAssembly_Test()
        {
            //No dll can be loaded
            Func<Assembly,bool> isCandidateAssembly = (Assembly a) =>
             {
                 return false;
             };

            var option = new EnvironmentInitializationOption
            {
                RootConfigFile = "config/unitTest/test_app1.config",//this line will be ignored
                IsCandidateAssembly = isCandidateAssembly
            };

            var appEnv = new ApplicationEnvironment("c4", option);

            //Assert.IsTrue(appEnv.EnvironmentConfigure.ConnectionStrings["dbTest1"] != null);
        }

        [TestMethod()]
        public void EnvironmentInitializationOption_RuntimeComponents_Test()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var option = new EnvironmentInitializationOption
            {
                RootConfigFile = "config/unitTest/test_app1.config",//this line will be ignored
                RuntimeComponents = assemblies
            };

            var appEnv = new ApplicationEnvironment("c5", option);

            //Assert.IsTrue(appEnv.EnvironmentConfigure.ConnectionStrings["dbTest1"] != null);
        }

    }
}
