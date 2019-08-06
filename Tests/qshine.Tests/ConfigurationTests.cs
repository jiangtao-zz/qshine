using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Logger;
using qshine.Configuration;
using System;
using System.Reflection;
using qshine.Configuration.ConfigurationStore;

namespace qshine.Tests
{
    [TestClass]
    public class SetupAssemblyInitializer
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            //This is only running once. Ignore subsequently call ApplicationEnvironment.Boot().
            ApplicationEnvironment.Build("app.config");

            //try use internal providerfactory 
            Log.LogProviderFactory = new InternalProviderFactory();
            Log.SysLogger.EnableLogging(System.Diagnostics.TraceEventType.Verbose);

        }
    }

    [TestClass()]
    [DeploymentItem("app.config", "./testhost.dll.config")]
    public class ApplicationEnvironmentTests
    {
        ApplicationEnvironment app;

        [TestInitialize]
        public void Init()
        {
            var builder = new ApplicationEnvironmentBuilder("test1");

            app = builder.Configure(
                (context, configure) =>
                {
                    configure.LoadConfigFile("app.config",null);
                }).Build();
        }


        [TestMethod()]
        public void LoadConfig_Default_File()
        {
            var configure = app.EnvironmentConfigure;

            //Assert.AreEqual("config", configure.Environments["root"].Path);
            //Assert.AreEqual("", configure.Environments["root"].Host);
            //Assert.AreEqual("TestServerFolder/QA_config", configure.Environments["qa"].Path);
            //Assert.AreEqual("202.22.22.22", configure.Environments["qa"].Host);
            //Assert.AreEqual("TestServerFolder/UA_config", configure.Environments["ua"].Path);
            Assert.AreEqual("key1 value", configure.AppSettings["key1"]);
            Assert.AreEqual("level0 key0 value", configure.AppSettings["key0"]);
            Assert.AreEqual("moduleTest", configure.Modules["moduleTest"].Name);
            Assert.AreEqual("qshine.NHibernateRepository.BootStraper, QShine.NHibernateRepository", configure.Modules["moduleTest"].ClassTypeName);
            Assert.IsFalse(configure.Modules.Contains("xyz"));


            Assert.IsTrue(configure.ConfigureFolders.Count > 0);
            Assert.IsTrue(configure.AssemblyFolders.Count > 0);

            Assert.IsTrue(app.PlugableAssemblies["qshine.ioc.autofac"].Path.Contains("qshine.ioc.autofac.dll"));
            Assert.IsTrue(app.PlugableAssemblies["Autofac"].Path.Contains("Autofac.dll"));
            Assert.IsTrue(app.PlugableAssemblies["qshine.log.nlog"].Path.Contains("qshine.log.nlog.dll"));
            //            Assert.IsTrue(ApplicationEnvironment.AssemblyMaps["NLog"].Path.Contains("NLog.dll"));

            Assert.IsTrue(configure.Maps.ContainsKey("qshine.Tests.ITest1Provider"));
            Assert.AreEqual("c1", configure.Maps["qshine.Tests.ITest1Provider"]["c1Key"]);
            Assert.AreEqual("c1", configure.Maps["qshine.Tests.ITest1Provider"]["c11Key"]);
            Assert.AreEqual("c2", configure.Maps["qshine.Tests.ITest1Provider"]["c2Key"]);
            Assert.AreEqual("c2", configure.Maps["qshine.Tests.ITest1Provider"].Default);

            Assert.IsFalse(configure.Maps["qshine.Tests.ITest1Provider"].ContainsKey("X"));
            Assert.IsFalse(configure.Maps.ContainsKey("busx"));


            Log.SysLogger.Info("LoadConfig_Default_File End");

        }
        [TestMethod()]
        public void App_init()
        {
            //var app = ApplicationEnvironment.Default;

            var iocProvider = app.Services.GetProvider<IIocProvider>();
            Assert.IsNotNull(iocProvider);
        }

        [TestMethod()]
        public void CreateProvider_NoProvider()
        {
            var provider = app.Services.GetProvider<IProvider>();
            Assert.IsNull(provider);
        }

        [TestMethod()]
        public void CreateNamedProvider_NoName()
        {
            var provider = app.Services.GetProviderByName<IIocProvider>("NoName");
            Assert.IsNull(provider);
        }

        [TestMethod()]
        public void CreateNamedProvider_GoodName()
        {
            var provider = app.Services.GetProvider<ITest1Provider>("c1");
            Assert.IsNotNull(provider as TestC1Provider);

            provider = app.Services.GetProvider<ITest1Provider>("c2");
            Assert.IsNotNull(provider as TestC2Provider);
        }

        [TestMethod()]
        public void CreateMappedProvider_NoMapAtAll()
        {
            var provider = app.Services.GetProvider<IIocProvider>("NoMap");
            Assert.IsNotNull(provider,"Should be default provider");

        }

        [TestMethod()]
        public void CreateMappedProvider_GoodMap()
        {
            var provider = app.Services.GetProvider<ITest1Provider>("c1Key");
            Assert.IsNotNull(provider as TestC1Provider, "Should be TestC1Provider");

            provider = app.Services.GetProvider<ITest1Provider>("c11Key");
            Assert.IsNotNull(provider as TestC1Provider, "Should be TestC1Provider");

            provider = app.Services.GetProvider<ITest1Provider>("c11Keyxxxxx");
            Assert.IsNotNull(provider as TestC2Provider, "Should be Default provider");

            provider = app.Services.GetProvider<ITest1Provider>("c2");
            Assert.IsNotNull(provider as TestC2Provider, "Should be c2 provider");

        }

        [TestMethod()]
        public void CreateComponent_Good()
        {
            var provider = app.Services.GetComponent<ITest1Provider>("c1");
            Assert.IsTrue(provider is TestC1Provider, "Should be TestC1Provider");
        }

        [TestMethod()]
        public void CreateMappedComponent_Good()
        {
            var provider = app.Services.GetProvider<ITest1Provider >("c2Key");
            Assert.IsTrue(provider is TestC2Provider, "Should be TestC1Provider");
        }


        [TestMethod()]
        public void ConnectionStrings()
        {
            var db1 = app.EnvironmentConfigure.ConnectionStrings["db1"];
            Assert.AreEqual("testProvider", db1.ProviderName);
            Assert.AreEqual("abc,002", db1.ConnectionString);

            var db2 = app.EnvironmentConfigure.ConnectionStrings["db2"];
            Assert.AreEqual("testProvider2", db2.ProviderName);
            Assert.AreEqual("abc2,aaa", db2.ConnectionString);

            var db3 = app.EnvironmentConfigure.ConnectionStrings["db3"];
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
                OverwriteConnectionString = false
            };
            var configureFile = "unitTest/test_app1.config";

            var appEnv = ApplicationEnvironment.Build(configureFile, option);

            //Test connectionStrings
            Assert.IsTrue(appEnv.EnvironmentConfigure.ConnectionStrings.Count >= 3);
            Assert.AreEqual("dbt1,001", appEnv.EnvironmentConfigure.ConnectionStrings["dbTest1"].ConnectionString);
            Assert.AreEqual("dbt2,002", appEnv.EnvironmentConfigure.ConnectionStrings["dbTest2"].ConnectionString);

            //Test AppSettings
            Assert.AreEqual("key0 value", appEnv["key0"]);
            Assert.AreEqual("key1 value", appEnv["key1"]);
            Assert.AreEqual("key2 value", appEnv["key2"]);

            //Test modules
            Assert.AreEqual("M1.testClass, M1", appEnv.EnvironmentConfigure.Modules["moduleTest1"].ClassTypeName);
            Assert.AreEqual("M1.testClass, M1", appEnv.EnvironmentConfigure.Modules["moduleTest2"].ClassTypeName);

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

            var appEnv1 = ApplicationEnvironment.Build(configureFile, option);

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
            Assert.AreEqual("M1.testClass, M1_overwrite", appEnv1.EnvironmentConfigure.Modules["moduleTest1"].ClassTypeName);
            Assert.AreEqual("M1.testClass, M1", appEnv1.EnvironmentConfigure.Modules["moduleTest2"].ClassTypeName);

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

            var appEnv = ApplicationEnvironment.Build("config.FilePath", null);

            Assert.IsTrue(appEnv.EnvironmentConfigure.ConnectionStrings["dbTest1"] == null);

            Assert.AreEqual(0, appEnv.EnvironmentConfigure.Modules.Count);
            Assert.AreEqual(0, appEnv.EnvironmentConfigure.Components.Count);
            Assert.AreEqual("", appEnv["key0"]);
        }

        [TestMethod()]
        public void EnvironmentInitializationOption_IsCandidateAssembly_Test()
        {
            //No dll can be loaded
            Func<Assembly, bool> isCandidateAssembly = (Assembly a) =>
              {
                  return false;
              };

            EnvironmentInitializationOption.IsCandidateAssembly = isCandidateAssembly;


            var appEnv = ApplicationEnvironment.Build("unitTest/test_app1.config", null);

            //Assert.IsTrue(appEnv.EnvironmentConfigure.ConnectionStrings["dbTest1"] != null);
        }

        [TestMethod()]
        public void EnvironmentInitializationOption_RuntimeComponents_Test()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            EnvironmentInitializationOption.RuntimeComponents = assemblies;

            var appEnv = ApplicationEnvironment.Build("unitTest/test_app1.config", null);

            //Assert.IsTrue(appEnv.EnvironmentConfigure.ConnectionStrings["dbTest1"] != null);
        }

        [TestMethod()]
        public void ApplicationEnvironment_GetNamedType_From_Plugin_Test()
        {
            var theType = app.PlugableAssemblies.GetTypeByName("qshine.Tests.SampleModuleClass");
            Assert.AreEqual(typeof(SampleModuleClass), theType);

            var theType1 = app.PlugableAssemblies.GetTypeByName("qshine.Tests.SampleModuleClass, qshine.Tests");
            Assert.AreEqual(typeof(SampleModuleClass), theType1);

            var theType2 = app.PlugableAssemblies.GetTypeByName("qshine.Tests.SampleModuleClass, qshine.Tests, Version=1.0.0.0");
            Assert.AreEqual(typeof(SampleModuleClass), theType2);

            var theType3 = app.PlugableAssemblies.GetTypeByName("qshine.ioc.autofac.Provider");
            Assert.AreEqual("qshine.ioc.autofac.Provider", theType3.FullName);

            var theType4 = app.PlugableAssemblies.GetTypeByName("qshine.ioc.autofac.Provider, qshine.ioc.autofac");
            Assert.AreEqual("qshine.ioc.autofac.Provider", theType4.FullName);

            var theType5 = app.PlugableAssemblies.GetTypeByName("qshine.ioc.autofac.Provider, qshine.ioc.autofac, Version=1.0.0.0");
            Assert.AreEqual("qshine.ioc.autofac.Provider", theType5.FullName);

        }

        [TestMethod()]
        public void ApplicationEnvironment_Startup_Test()
        {
            Assert.AreEqual(1, Startup1.Result);
            Assert.AreEqual(1, Startup2.Result);

            var builder = new ApplicationEnvironmentBuilder("Tetst1");
            builder.Configure(
                (context, configure) =>
                {
                    configure.LoadConfigFile("app.config", null);
                }
                ).Build()
            .Startup<IStartupClass>();

            Assert.AreEqual(2, Startup1.Result);
            Assert.AreEqual("Tetst1", Startup1.EnvironmentName);
            Assert.AreEqual(3, Startup2.Result);

        }
    }

    public class InternalProviderFactory : ILoggerProviderFactory
    {
        public ILoggerProvider CreateProvider(string category)
        {
            return new TraceLoggerProvider();
        }

        public void RegisterProvider(ILoggerProvider provider, string category)
        {
            throw new NotImplementedException();
        }
    }

    public interface ITest1Provider:IProvider
    {

    }

    public class TestC1Provider :ITest1Provider
    {

    }

    public class TestC2Provider : ITest1Provider
    {

    }

    public interface IStartupClass
    {
    }

    public class Startup1:IStartupClass
    {
        public Startup1(ApplicationEnvironment env)
        {
            Startup1.Result++;

            EnvironmentName = env.Name;

            Assert.IsTrue(env.EnvironmentConfigure.Environments.Count > 0);
        }

        public static int Result = 1;
        public static string EnvironmentName { get; set; }
    }

    public class Startup2 : IStartupClass
    {
        public Startup2()
        {
            Startup2.Result+=2;
        }

        public static int Result = 1;
    }
}
