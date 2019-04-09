using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Caching;
using System.Collections.Generic;
using System.Linq;

namespace qshine.Tests
{
    [TestClass]
    public class CacheTests
    {

        [TestMethod()]
        public void CacheObjectBestTest()
        {
            var monitor = MyCacheDataChangeMonitor.Current;
            Cache.RegisterDataChangeMonitor(monitor);

            var cacheObject = new CacheableTestObject1();
            Assert.AreEqual(3, cacheObject.Value.Count);
            Assert.AreEqual(1, cacheObject.Value[0].IntV);
            //change source data directly doesn't affect cache object
            TestObject1.SourceDataStore[0].IntV = 12333;
            Assert.AreEqual(1, cacheObject.Value[0].IntV);

            //change source data through dependency
            TestObject1.ChangeData(1, "A",12333);

            Assert.AreEqual(12333, cacheObject.Value[0].IntV);
        }

    }

    public class CacheableTestObject1 : CacheObject<List<TestObject1>>
    {
        public CacheableTestObject1()
        {
            LoadSourceData = () =>
            {
                return TestObject1.DataStore.FindAll(x=>x.StringV=="A");
            };
            DependencyTags = new string[] {typeof(TestObject1).FullName};
        }
    }

    public class CacheableTestObject2 : CacheObject<List<TestObject1>>
    {
        public CacheableTestObject2()
        {
            LoadSourceData = () =>
            {
                return TestObject1.DataStore.FindAll(x => x.StringV == "B");
            };
            DependencyTags = new string[] { typeof(TestObject1).FullName };
        }
    }


}
