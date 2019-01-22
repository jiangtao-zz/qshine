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
        public void CallContextStore_Tests()
        {
            var store = new CallContextStore();
            //set data by top level caller
            //store.SetData("test1", "value1");
            //Assert.AreEqual("value1", store.GetData("test1").ToString());

            //pass top value to tasks, value will pass into all async process threads
            var ts1 = Tests_CallContextStore_MultiTasks("test2", null, "value3");

            //overwrite top level value only and the top value only affect to the threads created after store data changed
            store.SetData("test2", "value2");
            Assert.AreEqual("value2", store.GetData("test2").ToString());

            var ts2 = Tests_CallContextStore_MultiTasks("test2", "value2", "value4");

            var x = new List<Task>(ts1);
            x.AddRange(ts2);
            Task.WaitAll(x.ToArray());

            Assert.AreEqual("value2", store.GetData("test2").ToString());

            Assert.AreEqual(ContextStoreType.CallContext, store.ContextType);
            store.FreeData("test2");
            Assert.IsTrue(store.GetData("test2") == null);
        }

        Task[] Tests_CallContextStore_MultiTasks(string key, string expectedValue, string overwriteValue)
        {
            var store = new CallContextStore();
            if (expectedValue == null)
            {
                Assert.IsNull(store.GetData(key));
            }
            else
            {
                Assert.AreEqual(expectedValue, store.GetData(key).ToString());
            }

            //each thread has the same value from caller
            var t1 = Task.Run(() => Logic_GetExpectedValue(key, expectedValue, overwriteValue));
            var t2 = Task.Run(() => Logic_GetExpectedValue(key, expectedValue, overwriteValue));
            return new Task[] { t1, t2 };
        }
        void Logic_GetExpectedValue(string key, string expectedValue, string newValue)
        {
            var store = new CallContextStore();
            if (expectedValue == null)
            {
                Assert.IsNull(store.GetData(key));
            }
            else
            {
                Assert.AreEqual(expectedValue, store.GetData(key).ToString());
            }
            store.SetData(key, newValue);
        }

        [TestMethod()]
        public void LocalContextStore_Tests()
        {
            var store = new LocalContextStore();
            //set data by top level caller
            store.SetData("test3", "value1");
            Assert.AreEqual("value1", store.GetData("test3").ToString());

            //Same as CallContextLogicStore in single appdomain
            var ts1 = Tests_Local_ContextStore_MultiTasks("test3", "value1", "value4");

            //overwrite top level value only and the value only affect to the threads created after store data changed
            store.SetData("test3", "value2");
            Assert.AreEqual("value2", store.GetData("test3").ToString());

            var ts2 = Tests_Local_ContextStore_MultiTasks("test3", "value2", "value6");

            var x = new List<Task>(ts1);
            x.AddRange(ts2);
            Task.WaitAll(x.ToArray());

            Assert.AreEqual("value2", store.GetData("test3").ToString());

            Assert.AreEqual(ContextStoreType.ThreadLocal, store.ContextType);
            store.FreeData("test3");
            Assert.IsTrue(store.GetData("test3") == null);

        }

        Task[] Tests_Local_ContextStore_MultiTasks(string key, string expectedValue, string overwriteValue)
        {
            var store = new LocalContextStore();
            Assert.AreEqual(expectedValue, store.GetData(key).ToString());

            //each thread has the same value from caller
            var t1 = Task.Run(() => Local_GetExpectedValue(key, null, overwriteValue));
            var t2 = Task.Run(() => Local_GetExpectedValue(key, null, overwriteValue));
            return new Task[] { t1, t2 };
        }
        void Local_GetExpectedValue(string key, string expectedValue, string newValue)
        {
            var store = new LocalContextStore();

            if(!string.IsNullOrEmpty(expectedValue))
                Assert.AreEqual(expectedValue, store.GetData(key).ToString());
            else
                Assert.IsNull(store.GetData(key));

            store.SetData(key, newValue);

            //Let each task running in single thread
            Thread.Sleep(100);
            Assert.AreEqual(newValue, store.GetData(key).ToString());
            store.FreeData(key);
        }

        [TestMethod()]
        public void ContextStoreManager_Tests()
        {
            var store1 = ContextManager.GetContextStore(ContextStoreType.ThreadLocal);
            store1.SetData("test11", "value1");
            Assert.AreEqual("value1", store1.GetData("test11").ToString());
            store1.SetData("test11", "value2");

            //CallLocal and CallLogic has same store in same appdomain
            var store2 = ContextManager.GetContextStore(ContextStoreType.CallContext);
            store2.SetData("test11", "value2");
            Assert.AreEqual("value2", store1.GetData("test11").ToString());
            Assert.AreEqual("value2", store2.GetData("test11").ToString());
            store2.SetData("test11", "value21");

            var store3 = ContextManager.GetContextStore(ContextStoreType.Static);
            store3.SetData("test11", "value3");
            Assert.AreEqual("value2", store1.GetData("test11").ToString());
            Assert.AreEqual("value21", store2.GetData("test11").ToString());
            Assert.AreEqual("value3", store3.GetData("test11").ToString());
            store3.SetData("test11", "value31");

            //Current context
            ContextManager.Current = new LocalContextStore();
            ContextManager.SetData("test11", "value4");
            Assert.AreEqual("value4", store1.GetData("test11").ToString());
            Assert.AreEqual("value21", store2.GetData("test11").ToString());
            Assert.AreEqual("value31", store3.GetData("test11").ToString());

            Assert.AreEqual(ContextStoreType.ThreadLocal, ContextManager.Current.ContextType);

            ContextManager.FreeData("test11");
            Assert.IsTrue(ContextManager.GetData("test11") == null);

            ContextManager.Current = new StaticContextStore();
            Assert.AreEqual(ContextStoreType.CallContext, ContextManager.CallContext.ContextType);
            Assert.AreEqual(ContextStoreType.Static, ContextManager.StaticContext.ContextType);
            Assert.AreEqual(ContextStoreType.ThreadLocal, ContextManager.ThreadContext.ContextType);
        }


    }
}
