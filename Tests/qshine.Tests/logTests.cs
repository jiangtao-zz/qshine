using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Logger;

namespace qshine.Tests
{
    [TestClass]
    public class logTests
    {
        [TestMethod]
        public void ConsoleLoggerTest()
        {
            var loggerProvider = new ConsoleLoggerProvider();
            var logger = loggerProvider.GetLogger("general");
            logger.EnableLogging(System.Diagnostics.TraceEventType.Verbose);
            logger.Debug("test1 {0}-{1}", "p1", 2);
            logger.Trace("test1 {0}-{1}", "p1", 2);
            logger.Warn("test1 {0}-{1}", "p1", 2);
            logger.Error("test1 {0}-{1}", "p1", 2);
        }
    }
}
