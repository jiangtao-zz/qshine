using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Configuration;

namespace qshine.newton.Tests
{
    [TestClass]
    public class mainTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            ApplicationEnvironment.Build("app.config");
        }
    }
}
