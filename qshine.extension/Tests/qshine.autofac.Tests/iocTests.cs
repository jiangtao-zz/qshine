using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Autofac;
namespace qshine.autofac.Tests
{
    [TestClass]
    public class iocTests
    {
        [TestMethod]
        public void provider_created()
        {
            var provider = new qshine.ioc.autofac.Provider();
            var container = provider.CreateContainer();
            Assert.IsNotNull(provider);
            Assert.IsNotNull(container);
        }

        [TestMethod]
        public void ConstructorInjection_Test()
        {
            var provider = new qshine.ioc.autofac.Provider();
            var container = provider.CreateContainer();
            container.RegisterType<IIoCTest, TestIoC>();
            container.RegisterType<ConsumeTestComponent>();

            var instance = container.Resolve<ConsumeTestComponent>();
            var result = instance.ConsumeMethod("123");
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void PropertyInjection_Test()
        {
            var provider = new qshine.ioc.autofac.Provider();
            var container = provider.CreateContainer();
            container.RegisterType<IIoCTest, TestIoC>();
            container.RegisterType<ConsumeTestComponent>();

            var instance = container.Resolve<ConsumeTestComponent>();
            var result = instance.ConsumeMethod("123");
            Assert.AreEqual(3, result);
        }

    }

    public interface IIoCTest
    {
        int SaveData(string data);
    }

    public class TestIoC : IIoCTest
    {
        public int SaveData(string data)
        {
            return data.Length;
        }
    }

    public class ConsumeTestComponent
    {
        IIoCTest _Instance;
        public ConsumeTestComponent(IIoCTest constructorInjection)
        {
            _Instance = constructorInjection;
        }

        public int ConsumeMethod(string data)
        {
            return _Instance.SaveData(data);
        }
    }

    public class ConsumeTestComponent2
    {
        IIoCTest Instance { get; set; }

        public int ConsumeMethod(string data)
        {
            return Instance.SaveData(data);
        }
    }
}
