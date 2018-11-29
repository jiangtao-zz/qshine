using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace qshine.Tests
{
    [TestClass()]

    public class jsonTests
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

            var v3 = json.Deserialize(typeof(SampleObject)) as SampleObject;

            Assert.AreEqual(v.V1, v3.V1);
            Assert.AreEqual(v.V2, v3.V2);
            Assert.AreEqual(v.V3, v3.V3);
            Assert.AreEqual(v.V4["a"], v3.V4["a"]);
            Assert.AreEqual(v.V4["b"], v3.V4["b"]);

        }

        [TestMethod()]
        public void Json_Deserialize_Dictionary()
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
                },
                V5 = new SampleObject
                {
                    V1 = "/Date(133520559241X)",
                    V2 = 5678,
                    V4 = new Dictionary<string, int>()
                    {
                        {"x",11 },
                        {"y",22 }
                    },
                    V6 = new int[] { 9, 8, 7 },
                    V7 = null
                },
                V6 = new int[] { 1, 2, 3, 4, 5 },
                V7 = new decimal[] { 1.1m, 2, 2m },
                V8 = new List<string> { "aa", "bb" },
                V9 = true
            };
            var json = v.Serialize();
            var v2 = json.DeserializeDictionary();

            Assert.AreEqual(v.V1, v2["V1"]);
            Assert.AreEqual(v.V2, Convert.ToDecimal(v2["V2"]));
            Assert.AreEqual(v.V3, v2["V3"]);
            var v2_4 = v2["V4"] as Dictionary<string, object>;
            Assert.AreEqual(v.V4["a"], Convert.ToInt32(v2_4["a"]));
            Assert.AreEqual(v.V4["b"], Convert.ToInt32(v2_4["b"]));

            var v2_5 = v2["V5"] as Dictionary<string, object>;
            Assert.AreEqual(v.V5.V2, Convert.ToDecimal(v2_5["V2"]));
            Assert.AreEqual(v.V5.V1, v2_5["V1"]);

            var v2_5_4 = v2_5["V4"] as Dictionary<string, object>;
            Assert.AreEqual(v.V5.V4["x"], Convert.ToDecimal(v2_5_4["x"]));
            Assert.AreEqual(v.V5.V4["y"], Convert.ToDecimal(v2_5_4["y"]));

            Assert.IsTrue((bool)v2["V9"]);

        }

        [TestMethod()]
        public void Json_Deserialize_Dictionary_JsonFormat()
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
                },
                V5 = new SampleObject
                {
                    V1 = "/Date(133520559241X)",
                    V2 = 5678,
                    V4 = new Dictionary<string, int>()
                    {
                        {"x",11 },
                        {"y",22 }
                    },
                    V6 = new int[] { 9, 8, 7 },
                    V7 = null
                },
                V6 = new int[] { 1, 2, 3, 4, 5 },
                V7 = new decimal[] { 1.1m, 2, 2m },
                V8 = new List<string> { "aa", "bb" },
                V9 = true
            };
            var json = v.Serialize(JsonFormat.ISO8601JavascriptDateFormat);
            var v2 = json.DeserializeDictionary(JsonFormat.ISO8601JavascriptDateFormat);

            Assert.AreEqual(v.V1, v2["V1"]);
            Assert.AreEqual(v.V2, Convert.ToDecimal(v2["V2"]));
            Assert.AreEqual(v.V3, v2["V3"]);
            var v2_4 = v2["V4"] as Dictionary<string, object>;
            Assert.AreEqual(v.V4["a"], Convert.ToInt32(v2_4["a"]));
            Assert.AreEqual(v.V4["b"], Convert.ToInt32(v2_4["b"]));

            var v2_5 = v2["V5"] as Dictionary<string, object>;
            Assert.AreEqual(v.V5.V2, Convert.ToDecimal(v2_5["V2"]));
            Assert.AreEqual(v.V5.V1, v2_5["V1"]);

            var v2_5_4 = v2_5["V4"] as Dictionary<string, object>;
            Assert.AreEqual(v.V5.V4["x"], Convert.ToDecimal(v2_5_4["x"]));
            Assert.AreEqual(v.V5.V4["y"], Convert.ToDecimal(v2_5_4["y"]));

            Assert.IsTrue((bool)v2["V9"]);

        }

        [TestMethod]
        public void SetJsonProvider_Test()
        {
            var previousProvider = Json.Provider;
            Json.Provider = new DotNetJsonSerializerProvider();
            var json = Json.Serialize(new SampleObject
            {
                V1 = "111"
            });
            var v2 = Json.Deserialize<SampleObject>(json);
            Assert.AreEqual("111", v2.V1);
            Json.Provider = previousProvider;
        }


        [TestMethod()]
        public void Serialzie_Deserialize_Json_MicrosoftDateFormat_Utc()
        {
            var v = new SampleObject();
            var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Utc);
            v.V3 = d;
            var json = v.Serialize(JsonFormat.MicrosoftDateFormat);
            Assert.IsTrue(json.Contains("\"V3\":\"\\/Date(1286878830123)"));
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
            Assert.IsTrue(json.Contains(string.Format("\"V3\":\"\\/Date(1286893230123{0})", d.ToString("zzz").Replace(":", ""))));
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
            Assert.IsTrue(json.Contains(string.Format("\"V3\":\"\\/Date(1286893230123{0})", d.ToString("zzz").Replace(":",""))));
            var v2 = json.Deserialize<SampleObject>(JsonFormat.MicrosoftDateFormat);
            Assert.AreEqual(v.V3.Ticks, v2.V3.Ticks);
            Assert.AreEqual(DateTimeKind.Local, v2.V3.Kind);
        }


        [TestMethod()]
        public void Serialzie_Deserialize_Json_ISO8601DateFormat_Utc()
        {
            var v = new SampleObject();
            var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Utc);
            v.V3 = d;
            var json = v.Serialize(JsonFormat.ISO8601DateFormat);
            Assert.IsTrue(json.Contains("\"V3\":\"2010-10-12T10:20:30Z"));
            var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601DateFormat);
            Assert.AreEqual(v.V3.Ticks - 1230000, v2.V3.Ticks);
            Assert.AreEqual(DateTimeKind.Utc, v2.V3.Kind);
        }

        [TestMethod()]
        public void Serialzie_Deserialize_Json_ISO8601DateFormat_Unspecified()
        {
            var v = new SampleObject();
            var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Unspecified);
            v.V3 = d;
            var json = v.Serialize(JsonFormat.ISO8601DateFormat);
            Assert.IsTrue(json.Contains("\"V3\":\"2010-10-12T10:20:30"));
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
            Assert.IsTrue(json.Contains(string.Format("\"V3\":\"2010-10-12T10:20:30{0}",d.ToString("zzz"))));
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
            Assert.IsTrue(json.Contains("\"V3\":\"2010-10-12T10:20:30.1230000Z"));
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
            Assert.IsTrue(json.Contains("\"V3\":\"2010-10-12T10:20:30.1230000"));
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
            Assert.IsTrue(json.Contains(string.Format("\"V3\":\"2010-10-12T10:20:30.1230000{0}", d.ToString("zzz"))));
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
            Assert.IsTrue(json.Contains("\"V3\":\"2010-10-12T10:20:30.123Z"));
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
            Assert.IsTrue(json.Contains("\"V3\":\"2010-10-12T10:20:30.123"));
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
            Assert.IsTrue(json.Contains(string.Format("\"V3\":\"2010-10-12T10:20:30.123{0}", d.ToString("zzz"))));
            var v2 = json.Deserialize<SampleObject>(JsonFormat.ISO8601JavascriptDateFormat);
            Assert.AreEqual(v.V3.Ticks, v2.V3.Ticks);
            Assert.AreEqual(DateTimeKind.Local, v2.V3.Kind);
        }

        [TestMethod()]
        public void Serialzie_Deserialize_Json_JavascriptDateFormat_No_Kind()
        {
            var v = new SampleObject();
            var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Utc);
            v.V3 = d;
            var json = v.Serialize(JsonFormat.JavascriptDateFormat);
            Assert.IsTrue(
                json.Contains("\"V3\":\"new Date(2010,10,12,10,20,30,123)")||
                json.Contains("\"V3\":new Date(1286878830123)"));
            var v2 = json.Deserialize<SampleObject>(JsonFormat.JavascriptDateFormat);
            Assert.AreEqual(v.V3.Ticks, v2.V3.Ticks);

            d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Local);
            v.V3 = d;
            json = v.Serialize(JsonFormat.JavascriptDateFormat);
            Assert.IsTrue(
                json.Contains("\"V3\":\"new Date(2010,10,12,10,20,30,123)") ||
                json.Contains("\"V3\":new Date(1286893230123)"));

            d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30, 123), DateTimeKind.Unspecified);
            v.V3 = d;
            json = v.Serialize(JsonFormat.JavascriptDateFormat);
            Assert.IsTrue(
                json.Contains("\"V3\":\"new Date(2010,10,12,10,20,30,123)") ||
                json.Contains("\"V3\":new Date(1286893230123)"));
        }

        [TestMethod()]
        public void Serialzie_Deserialize_Json_Custom_Date()
        {
            var v = new SampleObject();
            var d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30), DateTimeKind.Utc);
            v.V3 = d;
            var json = v.Serialize(JsonFormat.CustomDateFormat, new JsonFormatSetting { DateTimeFormat="r"});
            Assert.IsTrue(
                json.Contains("\"V3\":\"Tue, 12 Oct 2010 10:20:30 GMT\""));
            var v2 = json.Deserialize<SampleObject>(JsonFormat.CustomDateFormat, new JsonFormatSetting { DateTimeFormat = "r" });
            Assert.AreEqual(v.V3.Ticks, v2.V3.Ticks);

            d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30), DateTimeKind.Local);
            v.V3 = d;
            json = v.Serialize(JsonFormat.CustomDateFormat, new JsonFormatSetting { DateTimeFormat = "r" });
            Assert.IsTrue(
                json.Contains("\"V3\":\"Tue, 12 Oct 2010 10:20:30 GMT\""));

            d = DateTime.SpecifyKind(new DateTime(2010, 10, 12, 10, 20, 30), DateTimeKind.Unspecified);
            v.V3 = d;
            json = v.Serialize(JsonFormat.CustomDateFormat, new JsonFormatSetting { DateTimeFormat = "r" });
            Assert.IsTrue(
                json.Contains("\"V3\":\"Tue, 12 Oct 2010 10:20:30 GMT\""));
        }

        [TestMethod()]
        public void Deserialize_Json()
        {
            var json = "{\"V1\":\"abc\n123\r\nXYZ\",\"V2\":0.1234,\"V3\":\"\\/Date(1286856000000-0400)\\/\",\"V4\":{\"a\":1,\"b\":2}}";
            var v2 = json.Deserialize<SampleObject>();

            Assert.AreEqual("abc\n123\r\nXYZ", v2.V1);
            Assert.AreEqual(0.1234M, v2.V2);
            Assert.AreEqual(new DateTime(2010, 10, 12), v2.V3);
            Assert.AreEqual(1, v2.V4["a"]);
            Assert.AreEqual(2, v2.V4["b"]);

            var v3 = json.Deserialize(typeof(SampleObject)) as SampleObject;

            Assert.AreEqual("abc\n123\r\nXYZ", v3.V1);
            Assert.AreEqual(0.1234M, v3.V2);
            Assert.AreEqual(new DateTime(2010, 10, 12), v3.V3);
            Assert.AreEqual(1, v3.V4["a"]);
            Assert.AreEqual(2, v3.V4["b"]);

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
            Assert.AreEqual(new DateTime(2009, 9, 28, 10, 0, 0), v2.V3);
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
            Assert.AreEqual(new DateTime(2009, 9, 28, 10, 0, 0, 019), v2.V3);
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
        public void Serialize_Deserialize_Json_Dictionary()
        {
            var v1 = new SampleObject
            {
                V4 = new Dictionary<string, int>()
            };
            v1.V4.Add("k1", 1);
            v1.V4.Add("k2", 2);
            v1.V4.Add("k3", 3);
            var json = v1.Serialize();
            Assert.IsTrue(json.Contains("\"V4\":{\"k1\":1,\"k2\":2,\"k3\":3},"));
            json = "{\"V4\":{\"k1\":1,\"k2\":2,\"k3\":3}}";

            var v2 = json.Deserialize<SampleObject>();
            Assert.AreEqual(1, v2.V4["k1"]);
            Assert.AreEqual(2, v2.V4["k2"]);
            Assert.AreEqual(3, v2.V4["k3"]);
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
                    e.Result = (serializer).Deserialize(result, objectType, JsonFormat.Default,null);
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
        public SampleObject V5 { get; set; }
        public int[] V6;
        public decimal[] V7;
        public List<string> V8;
        public bool V9;
    }
}
