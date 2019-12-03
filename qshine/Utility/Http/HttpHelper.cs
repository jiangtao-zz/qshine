using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

using System.Net;
using System.Collections.Specialized;

namespace qshine.Utility.Http
{
    /// <summary>
    /// Uri
    /// 
    /// 1. uri.AbsoluteUri/uri.AbsolutePath - Encode a uri human readable text to http valid URI.
    /// 
    /// 2. HtmlEncode - Encode a http valid URI to HTML qualified URI which can be added in HTML href tag.
    /// (Note: In web environment Html encoded string will be converted to a valid Uri by HTML render or browser. No code required to decode Html encoded Uri)
    /// 3. UrlDecode - Decode a valid URI to human readable Uri
    /// 3. ParseQueryString  - Parse a decoded uri query string to a list of name/value pairs
    /// 
    /// 
    /// </summary>
    public static class HttpExtension
    {
        /// <summary>
        /// Create a new Uri with a relative path
        /// </summary>
        /// <param name="uri">base Uri</param>
        /// <param name="path">relative path</param>
        /// <returns></returns>
        public static Uri AppendPath(this Uri uri, string path)
        {
            return new Uri(uri, path);
        }

        /// <summary>
        /// Add query
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <param name="name">query argument name</param>
        /// <param name="value">query argument value</param>
        /// <returns></returns>
        public static Uri AddQuery(this Uri uri, string name, string value)
        {
            NameValueCollection queries = uri.ParseQueryString();

            var isFirstQuery = uri.Query.Length == 0;

            //Append query parameters
            var queryStringBuilder = new StringBuilder(uri.OriginalString);

            if (isFirstQuery)
                queryStringBuilder.Append("?");
            else
                queryStringBuilder.Append("&");
            queryStringBuilder.AppendFormat("{0}={1}", name, WebUtility.UrlEncode(value));

            return new UriBuilder(queryStringBuilder.ToString()).Uri;
                
        }

        /// <summary>
        /// Convert a human readable URL to a valid URL string
        /// </summary>
        /// <param name="uri">Uri object</param>
        /// <param name="originalUrl">original Url string.
        /// If the original Url is null or empty, it will encode full Uri
        /// </param>
        /// <example>
        /// <![CDATA[
        /// Example:
        /// http://my.domain.com/path1/path2/file1 name.html?p1=v1 v11&p2=v2 v22 &p3=v3+v33#abc=xyz
        /// ]]>
        /// </example>
        /// <returns>returns a valid URI string</returns>
        public static string GetQualifiedUri(this Uri uri, string originalUrl=null)
        {
            if (!string.IsNullOrEmpty(originalUrl))
            {
                Uri temp = uri;
                if (originalUrl.StartsWith("/"))
                {
                    temp = new Uri(uri, originalUrl);
                    var fullPath = temp.AbsoluteUri;
                    return fullPath.Substring(uri.GetLeftPart(UriPartial.Authority).Length);
                }
                else if (originalUrl.IndexOf("//") > 0)
                {
                    temp = new Uri(uri, originalUrl);
                    return temp.AbsoluteUri;
                }
                else
                {
                    temp = new Uri(uri, "/"+originalUrl);
                    var fullPath = temp.AbsoluteUri;
                    return fullPath.Substring(uri.GetLeftPart(UriPartial.Authority).Length+1);
                }
            }
            return uri.AbsoluteUri;
        }



        /// <summary>
        /// Parse Uri query string to a list of arguments
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns>Return a key/value pair collection.</returns>
        public static NameValueCollection ParseQueryString(this Uri uri)
        {
            var query = uri.Query;// WebUtility.HtmlDecode(uri.Query);
            return System.Web.HttpUtility.ParseQueryString(query);
        }

    }

    ///// <summary>
    ///// Html requester
    /////     var request = new HttpHelper("http://qshine.com")
    /////     request
    /////         .AddPath("token")
    /////         .AddQuery("name","value")
    /////         .AddFragment("f1","v1")
    /////         .
    ///// 
    ///// </summary>
    //public class HttpHelper
    //{
    //    string _baseUri;
    //    string _path;
    //    HttpMethod _requestMethod;
    //    HttpStatusCode _statusCode;
    //    int _subStatusCode;

    //    Dictionary<string,string> _queries = new Dictionary<string, string>();
    //    Dictionary<string, string> _fragments = new Dictionary<string, string>();
    //    Dictionary<string, string> _cacheControls = new Dictionary<string, string>();
    //    Dictionary<string, object> _data = new Dictionary<string, object>();

    //    enum CommonFormat
    //    {
    //        Other,
    //        Json,
    //        Text,
    //        Html,
    //        XHtml,
    //        Xml
    //    };

    //    CommonFormat _commonFormat = CommonFormat.Other;

    //    bool _isRequest;
    //    HttpRequestMessage _requestMessage;
    //    HttpResponseMessage _responseMessage;

    //    /// <summary>
    //    /// Create a new HttpHelper instance for http request.
    //    /// </summary>
    //    /// <param name="baseUri"></param>
    //    /// <returns></returns>
    //    public static HttpHelper NewRequest(string baseUri)
    //    {
    //        return new HttpHelper(baseUri);
    //    }

    //    /// <summary>
    //    /// Create a http response instance
    //    /// </summary>
    //    /// <returns></returns>
    //    public static HttpHelper NewResponse()
    //    {
    //        return new HttpHelper("", false);
    //    }

    //    /// <summary>
    //    /// Default ctor
    //    /// </summary>
    //    public HttpHelper()
    //        :this("", true)
    //    {
    //        _isRequest = true;
    //    }

    //    /// <summary>
    //    /// Ctor:: with http Uri
    //    /// </summary>
    //    /// <param name="baseUri"></param>
    //    public HttpHelper(string baseUri, bool isRequest=true)
    //    {
    //        _isRequest = isRequest;

    //        if (!string.IsNullOrEmpty(baseUri))
    //        {
    //            _baseUri = baseUri;
    //            if (!_baseUri.EndsWith("/"))
    //            {
    //                _baseUri = _baseUri + "/";
    //            }
    //        }

    //        if (_isRequest) _requestMessage = new HttpRequestMessage();

    //        //BaseAddress = new Uri(baseUri);
    //    }

    //    /// <summary>
    //    /// Get http request URL
    //    /// </summary>
    //    public string URL
    //    {
    //        get
    //        {
    //            StringBuilder uri = new StringBuilder();

    //            //append base
    //            if (!string.IsNullOrEmpty(_baseUri))
    //            {
    //                uri.Append(_baseUri);
    //            }

    //            //append path
    //            if (!string.IsNullOrEmpty(_path))
    //            {
    //                if (!_path.StartsWith("/"))
    //                    uri.Append("/");
    //                uri.Append(_path);
    //            }

    //            uri.Replace("//", "/");


    //            //append query
    //            if (_queries.Count > 0)
    //            {
    //                var query = string.Join("&", _queries.Select(x => x.Key + "=" + Uri.EscapeDataString(x.Value)).ToArray());
    //                if (!(uri.ToString().Contains("?")))
    //                {
    //                    uri.Append("?");
    //                }
    //                else
    //                {
    //                    uri.Append("&");
    //                }
    //                uri.Append(query);
    //            }

    //            //append fragment
    //            if (_fragments.Count > 0)
    //            {
    //                var fragment = string.Join("&", _fragments.Select(x => x.Key + "=" + Uri.EscapeDataString(x.Value)).ToArray());
    //                if (!(uri.ToString().Contains("#")))
    //                {
    //                    uri.Append("#");
    //                }
    //                else
    //                {
    //                    uri.Append("&");
    //                }
    //                uri.Append(fragment);
    //            }
    //            return uri.ToString();
    //        }
    //    }
    //    /// <summary>
    //    /// Append Uri relative path
    //    /// </summary>
    //    /// <param name="path"></param>
    //    /// <returns></returns>
    //    public HttpHelper AppendPath(string path)
    //    {
    //        if (string.IsNullOrEmpty(_path))
    //        {
    //            _path = path;
    //        }
    //        else
    //        {
    //            if (!_path.EndsWith("/"))
    //            {
    //                _path += "/";
    //            }
    //            _path += path;
    //            _path = _path.Replace("//", "/");
    //        }
    //        return this;
    //    }

    //    /// <summary>
    //    /// Add query parameter name=value. The query parameter start with question mark symbol "?" and separated by <![CDATA["&"]]> symbol.
    //    /// Example:
    //    /// <![CDATA[https://base.com/path/path?name=value]]>
    //    /// </summary>
    //    /// <param name="name">query parameter name</param>
    //    /// <param name="value">query parameter value</param>
    //    /// <returns></returns>
    //    public HttpHelper AddQuery(string name, string value)
    //    {
    //        if (!_queries.ContainsKey(name))
    //            _queries.Add(name, value);

    //        return this;
    //    }

    //    /// <summary>
    //    /// Add fragment in query string.
    //    /// A fragment is the last part of query string starting with "#" symbol
    //    /// See rfc <a href="https://tools.ietf.org/html/rfc3986">rfc3986</a>.
    //    /// </summary>
    //    /// <param name="name">fragment name</param>
    //    /// <param name="value">fragment value. if the val</param>
    //    /// <returns></returns>
    //    public HttpHelper AddFragment(string name, string value)
    //    {
    //        if(!_fragments.ContainsKey(name))
    //            _fragments.Add(name, value);

    //        return this;
    //    }

    //    /// <summary>
    //    /// Append http header message.
    //    /// 
    //    /// The http header message defined in <a href="https://tools.ietf.org/html/rfc4229"> RFC4229</a>.
    //    /// 
    //    /// There are four types of header:
    //    /// General headers apply to both requests and responses, but with no relation to the data transmitted in the body.
    //    /// Request headers contain more information about the resource to be fetched, or about the client requesting the resource.
    //    /// Response headers hold additional information about the response, like its location or about the server providing it.
    //    /// Entity headers contain information about the body of the resource, like its content length or MIME type.
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper Header(string name, string value)
    //    {
    //        _requestMessage.Headers.Add(name, value);
    //        return this;
    //    }

    //    #region Authorization Header
    //    /// <summary>
    //    /// Set <a href="https://tools.ietf.org/html/rfc2617#section-2">Basic Authentication Scheme.</a>
    //    /// </summary>
    //    /// <param name="userId">user name or id</param>
    //    /// <param name="password">password</param>
    //    /// <returns></returns>
    //    public HttpHelper AuthorizationBasic(string userId, string password)
    //    {
    //        return Header("Authorization", "Basic " + (userId + ":" + password).ToBase64());
    //    }

    //    /// <summary>
    //    /// Set Bearer Authorization token. See <a href="https://tools.ietf.org/html/rfc6750">Detail</a>.
    //    /// </summary>
    //    /// <param name="bearerToken">a base64 token for OAuth2</param>
    //    /// <returns></returns>
    //    public HttpHelper AuthorizationBearer(string bearerToken)
    //    {
    //        return Header("Authorization", "Bearer  " + bearerToken);
    //    }
    //    #endregion

    //    #region Content Type
    //    /// <summary>
    //    /// Set http content type header. The completed list of context type could be found in <a href="https://www.iana.org/assignments/media-types/media-types.xhtml">IANA</a>
    //    /// </summary>
    //    /// <param name="mimeType">MIME type of http context type.</param>
    //    /// <returns></returns>
    //    public HttpHelper ContentType(string mimeType)
    //    {
    //        return Header("Content-Type", mimeType);
    //    }

    //    /// <summary>
    //    /// The Json format content
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper JSON()
    //    {
    //        _commonFormat = CommonFormat.Json;
    //        return ContentType("application/json");
    //    }

    //    /// <summary>
    //    /// The HTML format
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper HTML()
    //    {
    //        _commonFormat = CommonFormat.Html;
    //        return ContentType("text/html");
    //    }

    //    /// <summary>
    //    /// The XML format
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper XML()
    //    {
    //        _commonFormat = CommonFormat.Xml;
    //        return ContentType("text/xml");
    //    }

    //    /// <summary>
    //    /// The XHTML format
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper XHTML()
    //    {
    //        _commonFormat = CommonFormat.XHtml;
    //        return ContentType("application/xhtml+xml");
    //    }

    //    /// <summary>
    //    /// The JPEG image
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper JPEG()
    //    {
    //        return ContentType("image/jpeg");
    //    }

    //    /// <summary>
    //    /// The PNG image
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper PNG()
    //    {
    //        return ContentType("image/png");
    //    }

    //    /// <summary>
    //    /// The TIFF image
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper TIFF()
    //    {
    //        return ContentType("image/tiff");
    //    }


    //    /// <summary>
    //    /// Default application form and url encoded context
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper ApplicationFormUrl()
    //    {
    //        return ContentType("application/x-www-form-urlencoded");
    //    }

    //    #endregion

    //    #region CacheControl 5.2.1.  Request Cache-Control Directives https://tools.ietf.org/html/rfc7234#section-5.2

    //    /// <summary>
    //    /// Turn off request/response cache
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper TurnOffCache()
    //    {
    //        return Header("Pragma", "no-cache")
    //            .NoCache()
    //            .NoStore();
    //    }


    //    /// <summary>
    //    /// Add Cache Control header
    //    /// </summary>
    //    /// <param name="name">The cache control name</param>
    //    /// <param name="value">The cache control value</param>
    //    /// <returns></returns>
    //    public HttpHelper CacheControl(string name, string value)
    //    {
    //        _cacheControls.Add(name, value);
    //        return this;
    //    }

    //    /// <summary>
    //    /// Set Cache control to no-cache for both request/response.
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper NoCache()
    //    {
    //        return CacheControl("no-cache", null);
    //    }

    //    /// <summary>
    //    /// Set Cache control with no store for both request/response.
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper NoStore()
    //    {
    //        return CacheControl("no-store", null);
    //    }

    //    /// <summary>
    //    /// Set cache control with no transform for both request/response.
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper NoTransform()
    //    {
    //        return CacheControl("no-transform", null);
    //    }


    //    /// <summary>
    //    /// Set Cache Control max-age value for both request and response.
    //    /// </summary>
    //    /// <param name="deltSeconds">maximum age in seconds</param>
    //    /// <returns></returns>
    //    public HttpHelper MaxAge(int deltSeconds)
    //    {
    //        return CacheControl("max-age", deltSeconds.ToString());
    //    }

    //    /// <summary>
    //    /// Set Cache Control max-stale value for request.
    //    /// </summary>
    //    /// <param name="deltSeconds">maximum stale in seonds</param>
    //    /// <returns></returns>
    //    public HttpHelper MaxStale(int deltSeconds)
    //    {
    //        return CacheControl("max-stale", deltSeconds.ToString());
    //    }

    //    /// <summary>
    //    /// Set Cache Control minimal fresh interval in seconds for request.
    //    /// </summary>
    //    /// <param name="deltSeconds">minimal fresh interval in seconds</param>
    //    /// <returns></returns>
    //    public HttpHelper MinFresh(int deltSeconds)
    //    {
    //        return CacheControl("min-fresh", deltSeconds.ToString());
    //    }

    //    /// <summary>
    //    /// Set cache control with only-if-cached setting for request.
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper OnlyIfCached()
    //    {
    //        return CacheControl("only-if-cached", null);
    //    }

    //    /// <summary>
    //    /// Set cache control with must-revalidate for response.
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper MustRevalidate()
    //    {
    //        return CacheControl("must-revalidate", null);
    //    }

    //    /// <summary>
    //    /// Set cache control to public for response.
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper PublicCache()
    //    {
    //        return CacheControl("public", null);
    //    }

    //    /// <summary>
    //    /// Set cache control to private for response.
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper PrivateCache()
    //    {
    //        return CacheControl("private", null);
    //    }

    //    /// <summary>
    //    /// Set cache control proxy-revalidate for response.
    //    /// </summary>
    //    /// <returns></returns>
    //    public HttpHelper ProxyRevalidate()
    //    {
    //        return CacheControl("proxy-revalidate", null);
    //    }



    //    #endregion

    //    #region Set Data

    //    /// <summary>
    //    /// Set form data
    //    /// </summary>
    //    /// <param name="key">form item key</param>
    //    /// <param name="value">form item value</param>
    //    /// <returns></returns>
    //    public HttpHelper WithData(string key, object value)
    //    {
    //        _data.Add(key, value);
    //        return this;
    //    }

    //    /// <summary>
    //    /// Only set form data if the value is not empty.
    //    /// </summary>
    //    /// <param name="key">form item key</param>
    //    /// <param name="value">form item value</param>
    //    /// <returns></returns>
    //    public HttpHelper WithDataIf(string key, string value)
    //    {
    //        if (!string.IsNullOrEmpty(value))
    //        {
    //            _data.Add(key, value);
    //        }
    //        return this;
    //    }
    //    #endregion


    //    #region Response Status Code

    //    /// <summary>
    //    /// Http response status code
    //    /// </summary>
    //    /// <param name="code">An integer value that represents the http response status code.</param>
    //    /// <returns></returns>
    //    public HttpHelper Status(HttpStatusCode code)
    //    {
    //        _statusCode = code;
    //        return this;
    //    }

    //    /// <summary>
    //    /// Gets or sets http sub status code of the response.
    //    /// </summary>
    //    /// <param name="code">An integer value that represents the sub status code.</param>
    //    /// <returns></returns>
    //    public HttpHelper SubStatus(int code)
    //    {
    //        _subStatusCode = code;
    //        return this;
    //    }

    //    /// <summary>
    //    /// Get Response status code.
    //    /// </summary>
    //    public string StatusCode
    //    {
    //        get
    //        {
    //            if (_subStatusCode == 0) return _statusCode.ToString();

    //            return string.Format("{0}.{1}", _statusCode, _subStatusCode);
    //        }
    //    }
    //    #endregion

    //}


}
