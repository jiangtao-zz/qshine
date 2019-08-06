using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Configuration;
using qshine;
using System;
namespace qshine.Tests
{
    [TestClass()]
    public class IStartupInitializerTests
    {
        public static int TestValue = 1;
        public static int TestValue2 = 1;

        [TestMethod()]
        public void Initialized_By_ApplicationEnvironment_Build()
        {
            ApplicationEnvironment.Build().Startup<IBoot>();
            Assert.AreEqual(6, TestValue);
        }

        [TestMethod()]
        public void Initialized_By_ApplicationEnvironment_Module_Configure()
        {
            ApplicationEnvironment.Build();
            Assert.AreEqual(4, TestValue2);
        }
    }

    /// <summary>
    /// Class load automatically based on interface
    /// </summary>
    public class SampleStaticInitializerClass1: IBoot
    {

        public SampleStaticInitializerClass1()
        {
            //Initialize before build environment
            IStartupInitializerTests.TestValue += 2;
        }

    }

    public class SampleStaticInitializerClass2: IBoot
    {

        public SampleStaticInitializerClass2()
        {
            //Initialize before build environment
            IStartupInitializerTests.TestValue += 3;
        }

        public SampleStaticInitializerClass2(ApplicationEnvironment env)
            : this()
        {

        }
    }

    public interface IBoot { }


    /// <summary>
    /// Sample module class loaded through environment configure setting
    /// </summary>
    public class SampleModuleClass
    {
        static SampleModuleClass()
        {
            IStartupInitializerTests.TestValue2 = 4;
        }
    }

}
