using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Configuration;
using qshine;
using System;
using System.Collections.Generic;

namespace qshine.UnitTests
{
	[TestClass()]
	public class UtilityTests
	{
		[TestMethod()]
		public void JsonSerialize_Deserialize()
		{
			var v = new SampleObject
			{
				V1 = "abc\n123\r\nXYZ",
				V2 = 0.1234M,
				V3 = new DateTime(2010, 10, 12),
				V4 = new Dictionary<string, int>()
				{
					{"a",1},
					{"b",2}
				}
			};
			var json = v.Serialize();
			var v2 = json.Deserialize<SampleObject>();

			Assert.AreEqual(v.V1, v2.V1);
			Assert.AreEqual(v.V2, v2.V2);
			Assert.AreEqual(v.V3, v2.V3);
			Assert.AreEqual(v.V4["a"], v2.V4["a"]);
			Assert.AreEqual(v.V4["b"], v2.V4["b"]);
		}

		[TestMethod()]
		public void Serialzie_Deserialize_Json_MicrosoftDateFormat_Utc()
		{
			var v = new SampleObject();
			var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Utc);
			v.V3 = d;
			var json = v.Serialize(JsonFormat.MicrosoftDateFormat);
			var v2 = json.Deserialize<SampleObject>(JsonFormat.MicrosoftDateFormat);
			Assert.AreEqual(v.V3.Ticks, v2.V3.Ticks);
			Assert.AreEqual(DateTimeKind.Utc, v2.V3.Kind);
		}

		[TestMethod()]
		public void Serialzie_Deserialize_Json_MicrosoftDateFormat_Unspecified()
		{
			var v = new SampleObject();
			var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Unspecified);
			v.V3 = d;
			var json = v.Serialize(JsonFormat.MicrosoftDateFormat);
			var v2 = json.Deserialize<SampleObject>(JsonFormat.MicrosoftDateFormat);
			Assert.AreEqual(v.V3.Ticks, v2.V3.Ticks);
			//Unspecified and Local both convert to local
			Assert.AreEqual(DateTimeKind.Local, v2.V3.Kind);
		}

		[TestMethod()]
		public void Serialzie_Deserialize_Json_MicrosoftDateFormat_Local()
		{
			var v = new SampleObject();
			var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Local);
			v.V3 = d;
			var json = v.Serialize(JsonFormat.MicrosoftDateFormat);
			var v2 = json.Deserialize<SampleObject>(JsonFormat.MicrosoftDateFormat);
			Assert.AreEqual(v.V3.Ticks, v2.V3.Ticks);
			Assert.AreEqual(DateTimeKind.Local, v2.V3.Kind);
		}


		[TestMethod()]
		public void Serialzie_Deserialize_Json_ISO8601DateFormat_Utc()
		{
			var v = new SampleObject();
			var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12,10,20,30,123), DateTimeKind.Utc);
			v.V3 = d;
			var json = v.Serialize(JsonFormat.ISO8601DateFormat);
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601DateFormat);
			Assert.AreEqual(v.V3.Ticks-1230000, v2.V3.Ticks);
			Assert.AreEqual(DateTimeKind.Utc, v2.V3.Kind);
		}

		[TestMethod()]
		public void Serialzie_Deserialize_Json_ISO8601DateFormat_Unspecified()
		{
			var v = new SampleObject();
			var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Unspecified);
			v.V3 = d;
			var json = v.Serialize(JsonFormat.ISO8601DateFormat);
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601DateFormat);
			Assert.AreEqual(v.V3.Ticks - 1230000, v2.V3.Ticks);
			Assert.AreEqual(DateTimeKind.Unspecified, v2.V3.Kind);
		}

		[TestMethod()]
		public void Serialzie_Deserialize_Json_ISO8601DateFormat_Local()
		{
			var v = new SampleObject();
			var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Local);
			v.V3 = d;
			var json = v.Serialize(JsonFormat.ISO8601DateFormat);
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601DateFormat);
			Assert.AreEqual(v.V3.Ticks - 1230000, v2.V3.Ticks);
			Assert.AreEqual(DateTimeKind.Local, v2.V3.Kind);
		}

		[TestMethod()]
		public void Serialzie_Deserialize_Json_ISO8601RoundtripDateFormat_Utc()
		{
			var v = new SampleObject();
			var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Utc);
			v.V3 = d;
			var json = v.Serialize(JsonFormat.ISO8601RoundtripDateFormat);
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601RoundtripDateFormat);
			Assert.AreEqual(v.V3.Ticks, v2.V3.Ticks);
			Assert.AreEqual(DateTimeKind.Utc, v2.V3.Kind);
		}

		[TestMethod()]
		public void Serialzie_Deserialize_Json_ISO8601RoundtripDateFormat_Unspecified()
		{
			var v = new SampleObject();
			var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Unspecified);
			v.V3 = d;
			var json = v.Serialize(JsonFormat.ISO8601RoundtripDateFormat);
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601RoundtripDateFormat);
			Assert.AreEqual(v.V3.Ticks, v2.V3.Ticks);
			Assert.AreEqual(DateTimeKind.Unspecified, v2.V3.Kind);
		}

		[TestMethod()]
		public void Serialzie_Deserialize_Json_ISO8601RoundtripDateFormat_Local()
		{
			var v = new SampleObject();
			var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Local);
			v.V3 = d;
			var json = v.Serialize(JsonFormat.ISO8601RoundtripDateFormat);
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601RoundtripDateFormat);
			Assert.AreEqual(v.V3.Ticks, v2.V3.Ticks);
			Assert.AreEqual(DateTimeKind.Local, v2.V3.Kind);
		}

		[TestMethod()]
		public void Serialzie_Deserialize_Json_ISO8601JavascriptDateFormatDateFormat_Utc()
		{
			var v = new SampleObject();
			var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Utc);
			v.V3 = d;
			var json = v.Serialize(JsonFormat.ISO8601JavascriptDateFormat);
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601JavascriptDateFormat);
			Assert.AreEqual(v.V3.Ticks, v2.V3.Ticks);
			Assert.AreEqual(DateTimeKind.Utc, v2.V3.Kind);
		}

		[TestMethod()]
		public void Serialzie_Deserialize_Json_ISO8601JavascriptDateFormat_Unspecified()
		{
			var v = new SampleObject();
			var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Unspecified);
			v.V3 = d;
			var json = v.Serialize(JsonFormat.ISO8601JavascriptDateFormat);
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601JavascriptDateFormat);
			Assert.AreEqual(v.V3.Ticks, v2.V3.Ticks);
			Assert.AreEqual(DateTimeKind.Unspecified, v2.V3.Kind);
		}

		[TestMethod()]
		public void Serialzie_Deserialize_Json_ISO8601JavascriptDateFormat_Local()
		{
			var v = new SampleObject();
			var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Local);
			v.V3 = d;
			var json = v.Serialize(JsonFormat.ISO8601JavascriptDateFormat);
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601JavascriptDateFormat);
			Assert.AreEqual(v.V3.Ticks, v2.V3.Ticks);
			Assert.AreEqual(DateTimeKind.Local, v2.V3.Kind);
		}


		[TestMethod()]
		public void Deserialize_Json()
		{
			var json = "{\"V1\":\"abc\n123\r\nXYZ\",\"V2\":0.1234,\"V3\":\"\\/Date(1286856000000-0400)\\/\",\"V4\":{\"a\":1,\"b\":2}}";
			var v2 = json.Deserialize<SampleObject>();

			Assert.AreEqual("abc\n123\r\nXYZ", v2.V1);
			Assert.AreEqual( 0.1234M, v2.V2);
			Assert.AreEqual(new DateTime(2010, 10, 12), v2.V3);
			Assert.AreEqual(1, v2.V4["a"]);
			Assert.AreEqual(2, v2.V4["b"]);
		}

		[TestMethod()]
		public void Deserialize_Json_Partial()
		{
			var json = "{\"V1\":\"abc\n123\r\nXYZ\",\"V2\":0.1234,\"V3\":\"\\/Date(1286856000000-0400)\\/\"}";
			var v2 = json.Deserialize<SampleObject>();

			Assert.AreEqual("abc\n123\r\nXYZ", v2.V1);
			Assert.AreEqual(0.1234M, v2.V2);
			Assert.AreEqual(new DateTime(2010, 10, 12), v2.V3);
			Assert.IsNull(v2.V4);
		}

		[TestMethod()]
		public void Deserialize_Json_ISO8601DateFormat_Unspecified()
		{
			var json = "{\"V3\":\"2009-09-28T10:00:00\"}"; //unspecified
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601DateFormat);
			Assert.AreEqual(new DateTime(2009, 9, 28,10,0,0), v2.V3);
			Assert.AreEqual(DateTimeKind.Unspecified, v2.V3.Kind);
			Assert.IsNull(v2.V4);
		}

		[TestMethod()]
		public void Deserialize_Json_ISO8601DateFormat_Utc()
		{
			var json = "{\"V3\":\"2009-09-28T10:00:00Z\"}";//Utc
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601DateFormat);
			Assert.AreEqual(new DateTime(2009, 9, 28, 10, 0, 0), v2.V3);
			Assert.AreEqual(DateTimeKind.Utc, v2.V3.Kind);
			Assert.IsNull(v2.V4);
		}

		[TestMethod()]
		public void Deserialize_Json_ISO8601DateFormat_Local()
		{
			var json = "{\"V3\":\"2009-09-28T10:00:00-05:00\"}";//Utc
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601DateFormat);
			Assert.AreEqual(new DateTime(2009, 9, 28, 11, 0, 0), v2.V3);
			Assert.AreEqual(DateTimeKind.Local, v2.V3.Kind);
			Assert.IsNull(v2.V4);
		}

		[TestMethod()]
		public void Deserialize_Json_ISO8601NetDateFormat_Unspecified()
		{
			var json = "{\"V3\":\"2009-09-28T10:00:00.1234567\"}"; //Unspecified
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601RoundtripDateFormat);
			var ticks = (new DateTime(2009, 9, 28, 10, 0, 0)).Ticks + 1234567;
			Assert.AreEqual(ticks, v2.V3.Ticks);
			Assert.AreEqual(DateTimeKind.Unspecified, v2.V3.Kind);
			Assert.IsNull(v2.V4);
		}

		[TestMethod()]
		public void Deserialize_Json_ISO8601NetDateFormat_Utc()
		{
			var json = "{\"V3\":\"2009-09-28T10:00:00.1234567Z\"}";//UTC zero
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601RoundtripDateFormat);
			var ticks = (new DateTime(2009, 9, 28, 10, 0, 0)).Ticks + 1234567;
			Assert.AreEqual(ticks, v2.V3.Ticks);
			Assert.AreEqual(DateTimeKind.Utc, v2.V3.Kind);
			Assert.IsNull(v2.V4);
		}

		[TestMethod()]
		public void Deserialize_Json_ISO8601NetDateFormat_Local()
		{
			var json = "{\"V3\":\"2009-09-28T10:00:00.1234567-05:00\"}"; //local
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601RoundtripDateFormat);
			var ticks = (new DateTime(2009, 9, 28, 11, 0, 0)).Ticks + 1234567;
			Assert.AreEqual(ticks, v2.V3.Ticks);
			Assert.AreEqual(DateTimeKind.Local, v2.V3.Kind);
			Assert.IsNull(v2.V4);
		}


		[TestMethod()]
		public void Deserialize_Json_ISO8601JavascriptDateFormat_Utc()
		{
			var json = "{\"V3\":\"2009-09-28T10:00:00.019Z\"}"; //UTC from javascript JSON.stringfy
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601JavascriptDateFormat);
			Assert.AreEqual(new DateTime(2009, 9, 28, 10, 0, 0,019), v2.V3);
			Assert.AreEqual(DateTimeKind.Utc, v2.V3.Kind);
			Assert.IsNull(v2.V4);
		}

		[TestMethod()]
		public void Deserialize_Json_ISO8601JavascriptDateFormat_Local()
		{
			var json = "{\"V3\":\"2009-09-28T10:00:00.019-05:00\"}"; //Local from javascript JSON.stringfy
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601JavascriptDateFormat);
			Assert.AreEqual(new DateTime(2009, 9, 28, 11, 0, 0, 019), v2.V3);
			Assert.AreEqual(DateTimeKind.Local, v2.V3.Kind);
			Assert.IsNull(v2.V4);
		}

		[TestMethod()]
		public void Deserialize_Json_ISO8601JavascriptDateFormat_Unspecified()
		{
			var json = "{\"V3\":\"2009-09-28T10:00:00.019\"}"; //javascript JSON.stringfy (unspecified)
			var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601JavascriptDateFormat);
			Assert.AreEqual(new DateTime(2009, 9, 28, 10, 0, 0, 019), v2.V3);
			Assert.AreEqual(DateTimeKind.Unspecified, v2.V3.Kind);
			Assert.IsNull(v2.V4);
		}

		[TestMethod()]
		public void Deserialize_Json_CustomFormat()
		{
			var json = "{\"V1\":\"12\",\"V3\":new Date(1286856000000)}"; //Javascript Date object
			var interceptor = Interceptor.Get<IJsonSerializer>();
			interceptor.OnEnter += CustomJsonInterceptor;
			var v2 = json.Deserialize<SampleObject>(JsonFormat.CustomDateFormat);
			interceptor.OnEnter -= CustomJsonInterceptor;
			Assert.AreEqual(2010, v2.V3.Year);
			Assert.AreEqual(10, v2.V3.Month);
			Assert.AreEqual(12, v2.V3.Day);
			Assert.AreEqual(DateTimeKind.Utc, v2.V3.Kind);
			Assert.AreEqual("12", v2.V1);
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


            var xmlHelper = new XmlHelper(xml);
            var result = xmlHelper.XmlSection;
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


            void CustomJsonInterceptor(object sender, InterceptorEventArgs e)
		{
			if (e.MethodName == "Deserialize" && (ulong)e.Args[2] == (ulong)JsonFormat.CustomDateFormat)
			{
				var jsonString = (string)e.Args[0];
				var objectType = (Type)e.Args[1];
				var serializer = (IJsonSerializer)sender;

            	var regex = new System.Text.RegularExpressions.Regex(@"new Date\(([0-9]*)\)");
				var matches = regex.Matches(jsonString);
				if (matches.Count > 0)
				{
					string dateNumber = matches[0].Groups[1].Value;
					var result = regex.Replace(jsonString, "\"/Date(" + dateNumber + ")/\"");
					//use current result instead of original Deserialize() result
					e.StopExecution = true;
					e.Result = (serializer).Deserialize(result, objectType, JsonFormat.Default);
				}
			}
		}

	
	}

	public class SampleObject
	{
		public string V1 { get; set; }
		public decimal V2 { get; set; }
		public DateTime V3 { get; set; }
		public Dictionary<string, int> V4 { get; set; }
	}
}
