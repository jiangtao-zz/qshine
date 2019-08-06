using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Configuration;
using qshine.Globalization;

namespace qshine.Tests
{
    [TestClass]
    public class LocalStringTests
    {
        [TestMethod]
        public void LocalStringByResourceTest()
        {
            var provider = new ResourceStringProvider(typeof(ResourceStringProvider).Assembly);

            var local = provider.Create("qshine.Globalization.Resources");
            var resourceString = local.GetString("My Test {0}");
            Assert.AreEqual("My Test {0}", resourceString);

        }

        [TestMethod]
        public void LocalStringByResourceTest2()
        {
            var app = ApplicationEnvironment.Default;

            app.EnvironmentConfigure.Components.AddTransien<ILocalStringProvider, ResourceStringProvider>();

            ILocalStringProvider provider = (ILocalStringProvider)app.Services.GetProvider(typeof(ILocalStringProvider));

            var local = provider.Create("qshine.Globalization.Resources");
            var resourceString = local.GetString("My Test {0}");
            Assert.AreEqual("My Test {0}", resourceString);

        }
    }
}