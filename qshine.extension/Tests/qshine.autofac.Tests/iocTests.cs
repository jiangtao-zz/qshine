using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Configuration;

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
        public void Named_ConstructorInjection_Test()
        {
            var provider = new qshine.ioc.autofac.Provider();
            var container = provider.CreateContainer();
            //container.RegisterType<IIoCTest, TestIoC>();
            container.RegisterType<IIoCTest, TestIoC2>();

           // var r = container.Resolve<IIoCTest>("n1");
            //Assert.AreEqual(4, r.SaveData("123"));


            container.RegisterType<ConsumeTestComponent, ConsumeTestComponent>("n1");

            var instance = container.Resolve<ConsumeTestComponent>("n1");
            var result = instance.ConsumeMethod("123");
            Assert.AreEqual(4, result);
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

    public class TestIoC2 : IIoCTest
    {
        public int SaveData(string data)
        {
            return data.Length+1;
        }
    }

    public interface IDummy
    {

    }
    public class ConsumeTestComponent: IDummy
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

    public class ConsumeTestComponent2: IDummy
    {
        int _x;
        public ConsumeTestComponent2(int x)
        {
            _x = x;
        }

        public int ConsumeMethod(string data)
        {
            return _x;
        }
    }
}
