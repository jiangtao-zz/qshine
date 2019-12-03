using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Utility;
using System;
using System.Collections.Generic;
using qshine.Utility.Http;

namespace qshine.Tests
{
    [TestClass()]
    public class UriExtensionTests
    {
        [TestMethod()]
        public void Uri_Encode_Best()
        {
            //Convert a human readable uri to fully qualified uri string
            var uri = new Uri("http://my.domain.com/path1/path2/file1 name.html?p1=v1 v11&p2=v2 v22 &p3=v3+v33&p4=%26#abc=xyz");
            var validUrl = uri.GetQualifiedUri();
            Assert.AreEqual("http://my.domain.com/path1/path2/file1%20name.html?p1=v1%20v11&p2=v2%20v22%20&p3=v3+v33&p4=%26#abc=xyz", validUrl);

            //A qualified uri will not be convert again
            Assert.AreEqual("http://my.domain.com/path1/path2/file1%20name.html?p1=v1%20v11&p2=v2%20v22%20&p3=v3+v33&p4=%26#abc=xyz", 
                uri.GetQualifiedUri(validUrl));

            //Convert a relative path
            validUrl = uri.GetQualifiedUri("/path1/path2/file1 name.html?p1=v1 v11&p2=v2 v22 &p3=v3+v33#abc=xyz");
            Assert.AreEqual("/path1/path2/file1%20name.html?p1=v1%20v11&p2=v2%20v22%20&p3=v3+v33#abc=xyz", validUrl);
            Assert.AreEqual("/path1/path2/file1%20name.html?p1=v1%20v11&p2=v2%20v22%20&p3=v3+v33#abc=xyz", uri.GetQualifiedUri(validUrl));

            //Convert a partial uri parameter
            validUrl = uri.GetQualifiedUri("file1 name.html?p1=v1 v11&p2=v2 v22 &p3=v3+v33#abc=xyz");
            Assert.AreEqual("file1%20name.html?p1=v1%20v11&p2=v2%20v22%20&p3=v3+v33#abc=xyz", validUrl);
            Assert.AreEqual("file1%20name.html?p1=v1%20v11&p2=v2%20v22%20&p3=v3+v33#abc=xyz", uri.GetQualifiedUri(validUrl));

        }



        [TestMethod()]
        public void Uri_ParseQueryString()
        {
            var uri = new Uri("https://my.domain.com/path1/path2?p1=v1&p2=v2&p3=v3");

            var queries = uri.ParseQueryString();
            Assert.AreEqual("v1", queries["p1"]);
            Assert.AreEqual("v2", queries["p2"]);
            Assert.AreEqual("v3", queries["p3"]);
            Assert.IsNull(queries["p4"]);
        }

        [TestMethod()]
        public void Uri_ParseQueryString_without_argument()
        {
            var uri = new Uri("https://my.domain.com/path1/path2");

            var queries = uri.ParseQueryString();
            Assert.IsNull(queries["p1"]);

            uri = new Uri("https://my.domain.com/path1/path2?");

            queries = uri.ParseQueryString();
            Assert.IsNull(queries["p1"]);

            uri = new Uri("https://my.domain.com/path1/path2#abc");

            queries = uri.ParseQueryString();
            Assert.IsNull(queries["p1"]);

            uri = new Uri("https://my.domain.com/path1/path2?===");

            queries = uri.ParseQueryString();
            Assert.IsNull(queries["p1"]);
            Assert.IsNull(queries["p2"]);
            Assert.IsNull(queries["p3"]);
        }

        [TestMethod()]
        public void Uri_ParseQueryString_with_encodedParameter()
        {
            var uri = new Uri("https://my.domain.com/path1/path2");
            var newUri=uri.AddQuery("p1", "v1")
                .AddQuery("P2", "V2&")
                .AddQuery("p3", "https://newSite/path1/contain space.html?pp1=V1 v11&pp2=v2+v22#s=xyz");

            Assert.AreEqual("https://my.domain.com/path1/path2?p1=v1&P2=V2%26&p3=https%3A%2F%2FnewSite%2Fpath1%2Fcontain+space.html%3Fpp1%3DV1+v11%26pp2%3Dv2%2Bv22%23s%3Dxyz",
                newUri.GetQualifiedUri());

            var queries = newUri.ParseQueryString();
            Assert.AreEqual("v1", queries["p1"]);
            Assert.AreEqual("V2&", queries["P2"]);
            Assert.AreEqual("https://newSite/path1/contain space.html?pp1=V1 v11&pp2=v2+v22#s=xyz", queries["p3"]);
            Assert.AreEqual("https://newsite/path1/contain%20space.html?pp1=V1%20v11&pp2=v2+v22#s=xyz", uri.GetQualifiedUri(queries["p3"]));
            Assert.IsNull(queries["p4"]);
        }

        [TestMethod()]
        public void Uri_ParseQueryString_encoded_space()
        {
            var uri = new Uri("https://my.domain.com/path1/path2?p1=v1+v11&p2=v2%20v22");

            var queries = uri.ParseQueryString();
            Assert.AreEqual("v1 v11", queries["p1"]);
            Assert.AreEqual("v2 v22", queries["p2"]);
            Assert.IsNull(queries["p4"]);
        }
    }
}
