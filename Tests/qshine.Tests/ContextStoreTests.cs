using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace qshine.Tests
{
    [TestClass()]
    public class ContextStoreTests
    {
        [TestMethod()]
        public void StaticContextStore_Tests()
        {
            var store = new StaticContextStore();
            //set data by top level caller
            store.SetData("test1", "value1");
            Assert.AreEqual("value1", store.GetData("test1").ToString());

            //pass top value to tasks, but value will be overwritten by later caller
            var ts1 = Tests_Static_ContextStore_MultiTasks("test1", "value1", "value2");

            //overwrite top level value
            store.SetData("test1", "value2");
            Assert.AreEqual("value2", store.GetData("test1").ToString());

            var ts2 = Tests_Static_ContextStore_MultiTasks("test1", "value2", "value2");

            var x = new List<Task>(ts1);
            x.AddRange(ts2);
            Task.WaitAll(x.ToArray());
        }

        Task[] Tests_Static_ContextStore_MultiTasks(string key, string expectedValue, string overwriteValue)
        {
            var store = new StaticContextStore();
            Assert.AreEqual(expectedValue, store.GetData(key).ToString());

            //each thread has the same value from caller
            var t1 = Task.Run(() => Static_GetExpectedValue(key, overwriteValue));
            var t2 = Task.Run(() => Static_GetExpectedValue(key, overwriteValue));
            return new Task[] { t1, t2 };
        }
        void Static_GetExpectedValue(string key, string expectedValue)
        {
            var store = new StaticContextStore();
            Assert.AreEqual(expectedValue, store.GetData(key).ToString());
        }


        [TestMethod()]
        public void LogicContextStore_Tests()
        {
            var store = new CallContextLogicStore();
            //set data by top level caller
            store.SetData("test1", "value1");
            Assert.AreEqual("value1", store.GetData("test1").ToString());

            //pass top value to tasks, value will pass into all async process threads
            var ts1 = Tests_Logic_ContextStore_MultiTasks("test1", "value1", "value1");

            //overwrite top level value only and the top value only affect to the threads created after store data changed
            store.SetData("test1", "value2");
            Assert.AreEqual("value2", store.GetData("test1").ToString());

            var ts2 = Tests_Logic_ContextStore_MultiTasks("test1", "value2", "value2");

            var x = new List<Task>(ts1);
            x.AddRange(ts2);
            Task.WaitAll(x.ToArray());
        }

        Task[] Tests_Logic_ContextStore_MultiTasks(string key, string expectedValue, string overwriteValue)
        {
            var store = new CallContextLogicStore();
            Assert.AreEqual(expectedValue, store.GetData(key).ToString());

            //each thread has the same value from caller
            var t1 = Task.Run(() => Logic_GetExpectedValue(key, overwriteValue));
            var t2 = Task.Run(() => Logic_GetExpectedValue(key, overwriteValue));
            return new Task[] { t1, t2 };
        }
        void Logic_GetExpectedValue(string key, string expectedValue)
        {
            var store = new CallContextLogicStore();
            Assert.AreEqual(expectedValue, store.GetData(key).ToString());
        }

        [TestMethod()]
        public void LocalContextStore_Tests()
        {
            var store = new CallContextLocalStore();
            //set data by top level caller
            store.SetData("test1", "value1");
            Assert.AreEqual("value1", store.GetData("test1").ToString());

            //Same as CallContextLogicStore in single appdomain
            var ts1 = Tests_Local_ContextStore_MultiTasks("test1", "value1", "value1");

            //overwrite top level value only and the value only affect to the threads created after store data changed
            store.SetData("test1", "value2");
            Assert.AreEqual("value2", store.GetData("test1").ToString());

            var ts2 = Tests_Local_ContextStore_MultiTasks("test1", "value2", "value2");

            var x = new List<Task>(ts1);
            x.AddRange(ts2);
            Task.WaitAll(x.ToArray());
        }

        Task[] Tests_Local_ContextStore_MultiTasks(string key, string expectedValue, string overwriteValue)
        {
            var store = new CallContextLocalStore();
            Assert.AreEqual(expectedValue, store.GetData(key).ToString());

            //each thread has the same value from caller
            var t1 = Task.Run(() => Local_GetExpectedValue(key, overwriteValue));
            var t2 = Task.Run(() => Local_GetExpectedValue(key, overwriteValue));
            return new Task[] { t1, t2 };
        }
        void Local_GetExpectedValue(string key, string expectedValue)
        {
            var store = new CallContextLocalStore();

            if(expectedValue!="")
                Assert.AreEqual(expectedValue, store.GetData(key).ToString());
            else
                Assert.IsNull(store.GetData(key));

            //Let each task running in single thread
            Thread.Sleep(100);
        }

        [TestMethod()]
        public void ContextStoreManager_Tests()
        {
            var store1 = ContextManager.GetContextStore(ContextStoreType.CallLocal);
            store1.SetData("test11", "value1");
            Assert.AreEqual("value1", store1.GetData("test11").ToString());

            //CallLocal and CallLogic has same store in same appdomain
            var store2 = ContextManager.GetContextStore(ContextStoreType.CallLogic);
            store2.SetData("test11", "value2");
            Assert.AreEqual("value2", store1.GetData("test11").ToString());
            Assert.AreEqual("value2", store2.GetData("test11").ToString());

            var store3 = ContextManager.GetContextStore(ContextStoreType.Static);
            store3.SetData("test11", "value3");
            Assert.AreEqual("value2", store1.GetData("test11").ToString());
            Assert.AreEqual("value2", store2.GetData("test11").ToString());
            Assert.AreEqual("value3", store3.GetData("test11").ToString());

            //Current context
            ContextManager.SetData("test11", "value4");
            Assert.AreEqual("value4", store1.GetData("test11").ToString());
            Assert.AreEqual("value4", store2.GetData("test11").ToString());
            Assert.AreEqual("value3", store3.GetData("test11").ToString());
        }


    }
}
