using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace qshine.Tests
{
	[TestClass()]
	public class UtilityTests
	{
        [TestMethod()]
        public void Check_IsNotEmpty()
        {
            Check.HaveValue("abc");
            Check.HaveValue("abc","XYZ");
            try
            {
                Check.HaveValue("");
                Assert.Fail("Must throw ArgumentNullException exception");
            }
            catch(ArgumentNullException)
            {
            }

            try
            {
                Check.HaveValue("","dummyName");
                Assert.Fail("Must throw ArgumentNullException exception");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("dummyName", ex.ParamName);
            }

            try
            {
                Check.HaveValue(null, "dummyName");
                Assert.Fail("Must throw ArgumentNullException exception");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("dummyName", ex.ParamName);
            }
        }

        [TestMethod()]
        public void Check_IsNotNull()
        {
            Check.HaveValue(2.5);
            Check.HaveValue(new object(), "XYZ");
            try
            {
                Check.HaveValue((object)null);
                Assert.Fail("Must throw ArgumentNullException exception");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                Check.HaveValue((object)null, "dummyName");
                Assert.Fail("Must throw ArgumentNullException exception");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("dummyName", ex.ParamName);
            }
        }

        [TestMethod()]
        public void Check_IsTrue()
        {
            Check.Assert<Exception>(true);

            try
            {
                Check.Assert<Exception>(false, "Cannot be null.");
                Assert.Fail("Must throw Exception");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Cannot be null.", ex.Message);
            }

            try
            {
                Check.Assert<IndexOutOfRangeException>(false, "The day {0} is incorrect", 2);
                Assert.Fail("Must throw IndexOutOfRangeException exception");
            }
            catch (IndexOutOfRangeException ex)
            {
                Assert.AreEqual("The day 2 is incorrect", ex.Message);
            }
        }

        [TestMethod()]
        public void ObjectInspection_Test()
        {
            var array = new int[] { 1, 2, 3, 4, 5 };
            var msg = ObjectInspector.FormatObjectValues(array);
            Assert.AreEqual("Int32[] {\r\n Int32[0]= {\r\n  1\r\n  }\r\n Int32[1]= {\r\n  2\r\n  }\r\n Int32[2]= {\r\n  3\r\n  }\r\n Int32[3]= {\r\n  4\r\n  }\r\n Int32[4]= {\r\n  5\r\n  }\r\n }\r\n", 
                msg);

            var list = new List<int> { 1, 2, 3, 4, 5 };
            msg = ObjectInspector.FormatObjectValues(list);
            Assert.AreEqual("List`1 {\r\n Int32[0]= {\r\n  1\r\n  }\r\n Int32[1]= {\r\n  2\r\n  }\r\n Int32[2]= {\r\n  3\r\n  }\r\n Int32[3]= {\r\n  4\r\n  }\r\n Int32[4]= {\r\n  5\r\n  }\r\n }\r\n",
                msg);

            var obj = new SampleObject
            {
                V1 = "VXY",
                V2 = 2.3m,
                V3 = DateTime.Now
            };

            msg = ObjectInspector.FormatObjectValues(obj);
            Assert.IsTrue(msg.Contains("VXY"));

            try
            {
                throw new ArgumentException("test");
            }
            catch(Exception ex)
            {
                msg = ObjectInspector.GetExceptionCallStack(ex);
                Assert.IsTrue(msg.Contains("ArgumentException"));
            }

        }


        [TestMethod()]
        public void DynamicXml()
        {
            var xml =
                @"<system.data>
    <!-- !!!!!   -->
    <DbProviderFactories>
      <!--Sqlite Data provider-->
      <remove invariant=""System.Data.SQLite.EF6""/>
      <add name = ""SQLite Data Provider (Entity Framework 6)"" 
            invariant = ""System.Data.SQLite.EF6""
           description = "".NET Framework Data Provider for SQLite (Entity Framework 6)""
           type = ""System.Data.SQLite.EF6.SQLiteProviderFactory, System.Data.SQLite.EF6"" />
      <remove invariant = ""System.Data.SQLite"" />
 
       <add name = ""SQLite Data Provider"" invariant = ""System.Data.SQLite""
           type = ""System.Data.SQLite.SQLiteFactory, System.Data.SQLite"" />
      <!--MySQL Data provider-->Text Value</DbProviderFactories >
  </system.data > ";


            var result = new XmlSection(xml);
            Assert.AreEqual("system.data", result.Name);
            Assert.AreEqual("DbProviderFactories", result.Items[0].Name);
            Assert.AreEqual("Text Value", result.Items[0].Value);
            Assert.AreEqual("remove", result.Items[0].Items[0].Name);
            Assert.AreEqual("System.Data.SQLite.EF6", result.Items[0].Items[0]["invariant"]);
            Assert.AreEqual("add", result.Items[0].Items[1].Name);
            Assert.AreEqual("SQLite Data Provider (Entity Framework 6)", result.Items[0].Items[1]["name"]);
            Assert.AreEqual("System.Data.SQLite.EF6", result.Items[0].Items[1]["invariant"]);
            Assert.AreEqual(".NET Framework Data Provider for SQLite (Entity Framework 6)", result.Items[0].Items[1]["description"]);
            Assert.AreEqual("System.Data.SQLite.EF6.SQLiteProviderFactory, System.Data.SQLite.EF6", result.Items[0].Items[1]["type"]);
            Assert.AreEqual("remove", result.Items[0].Items[2].Name);
            Assert.AreEqual("System.Data.SQLite", result.Items[0].Items[2]["invariant"]);
            Assert.AreEqual("add", result.Items[0].Items[3].Name);
            Assert.AreEqual("SQLite Data Provider", result.Items[0].Items[3]["name"]);
            Assert.IsNull(result.Items[0].Items[3]["NotExists"]);
            var name = result.Name;

        }


        [TestMethod]
        public void Enum_ToString_Test()
        {
            var v1 = SampleEnum.Status2;
            Assert.AreEqual("Status2", v1.GetStringValue());
            Assert.AreEqual("101", v1.GetStringValue(EnumValueType.OriginalValue));
            Assert.AreEqual("Status2", v1.GetStringValue(EnumValueType.OrigunalString));
            Assert.AreEqual("Status 2", v1.GetStringValue(EnumValueType.StringValue));
        }

        [TestMethod]
        public void Enum_StringToEnum_Test()
        {
            Assert.AreEqual(SampleEnum.Status3, "Status3".GetEnumValue<SampleEnum>());
            Assert.AreEqual(SampleEnum.Unknown, "xyz".GetEnumValue(SampleEnum.Unknown));
            var x = "xyz".GetEnumValue(SampleEnum.Unknown);
            
        }
    }

    public enum SampleEnum
     {
        [StringValue("Status 1")]
        Status1=100,
        [StringValue("Status 2")]
        Status2,
        [StringValue("Status 3")]
        Status3,
        [StringValue("Status 4")]
        Status4,
        [StringValue("Unknown value")]
        Unknown = -1
    }
}
