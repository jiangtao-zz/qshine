using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Configuration;
using qshine.log.nlog;

namespace qshine.log.nlog.Tests
{
    [TestClass]
    public class NLogTests
    {

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            ApplicationEnvironment.Build("app.cofig");
        }

        [TestMethod]
        public void TestMethod_default_config()
        {
            var provider = new Provider();
            var logger = provider.GetLogger("category_not_set_and_no_wildcard_in_log");

            Assert.IsFalse(logger.IsFatalEnabled);
            Assert.IsFalse(logger.IsErrorEnabled);
            Assert.IsFalse(logger.IsWarnEnabled);
            Assert.IsFalse(logger.IsInfoEnabled);
            Assert.IsFalse(logger.IsDebugEnabled);
            Assert.IsFalse(logger.IsTraceEnabled);
        }

        [TestMethod]
        public void TestMethod_file_config()
        {
            var provider = new Provider("./config/nlog_config/NLog.config");
            var logger = provider.GetLogger("unittest");

            Assert.IsTrue(logger.IsFatalEnabled);
            Assert.IsTrue(logger.IsErrorEnabled);
            Assert.IsTrue(logger.IsWarnEnabled);
            Assert.IsTrue(logger.IsInfoEnabled);
            Assert.IsTrue(logger.IsDebugEnabled);
            Assert.IsFalse(logger.IsTraceEnabled);
            logger.Fatal("Test1 {0}", "fatal");
            logger.Error("Test2 {0}", "error");
            logger.Warn("Test3 {0}", "warning");
            logger.Info("Test4 {0}", "information");
            logger.Debug("Test4 {0}", "debug");
            logger.Trace("Test4 {0}", "ignore");
        }

        [TestMethod]
        public void TestMethod_file_other_config()
        {
            var provider = new Provider("./config/nlog_config/NLog.config");
            var logger = provider.GetLogger("general");

            Assert.IsTrue(logger.IsFatalEnabled);
            Assert.IsTrue(logger.IsErrorEnabled);
            Assert.IsTrue(logger.IsWarnEnabled);
            Assert.IsTrue(logger.IsInfoEnabled);
            Assert.IsFalse(logger.IsDebugEnabled);
            Assert.IsFalse(logger.IsTraceEnabled);
        }

    }
}
