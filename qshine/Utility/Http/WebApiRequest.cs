using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Net;

namespace qshine.Utility.Http
{
    /// <summary>
    /// Web Api request message
    /// </summary>
    public class WebApiRequest
    {
        enum RawBodyDataType
        {
            UnDefined,
            Json,
            Xml,
            FormItem,
            RawText,
            Stream,
            ByteArray
        };
        //RawDataType _rawDataType = RawDataType.NotDefined;

        #region Fields

        WebApiHelper _apiHelper;
        HttpMethod _method;
        Uri _uri;
        HttpRequestMessage _requestMessage;

        CommonContentType _innerContentType = CommonContentType.UnDefined;

        Dictionary<string, string> _pathParameters = new Dictionary<string, string>();
        List<Tuple<string,string>> _queryParameters = new List<Tuple<string, string>>();
        Dictionary<string, object> _cookies = new Dictionary<string, object>();
        List<Tuple<string, Tuple<CommonContentType, object>>> _dataProperties = new List<Tuple<string, Tuple<CommonContentType, object>>>();
        //Dictionary<string, CommonContentType> _multipartDataPropertyTypes = new Dictionary<string, CommonContentType>();

        string _boundary;
        object _rawBody = null;
        RawBodyDataType _rawBodyDataType;
        //Stream _streamBody;
        //byte[] _byteArray;
        bool _isMultipart;

        #endregion

        /// <summary>
        /// Create a web api request
        /// </summary>
        /// <param name="apiHelper">Api helper</param>
        /// <param name="method">http request method/action</param>
        /// <param name="uri">http request Uri</param>
        public WebApiRequest(WebApiHelper apiHelper, HttpMethod method, Uri uri)
        {
            _apiHelper = apiHelper;

            _method = method;
            _uri = uri;
            _requestMessage = new HttpRequestMessage();
            _requestMessage.Method = _method;

        }

        #region AppendPath
        /// <summary>
        /// Append a relative path in request url
        /// </summary>
        /// <param name="path">Uri relative path</param>
        /// <example>
        ///     request.AppendPath("/student/{studient_id}");
        /// </example>
        /// <returns></returns>
        public WebApiRequest AppendPath(string path)
        {
            _uri = _uri.AppendPath(path);
            return this;
        }
        #endregion

        #region Path Params

        /// <summary>
        /// Add a path parameter.
        /// The path parameter is a placeholder enclosed in braces ({}).
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <example>
        /// URI: http://localhost/api/student/{stduient_id}/
        ///     request.AddPathParam("studient_id", 12345);
        ///     
        /// The request URI will be: http://localhost/api/student/12345/
        /// </example>
        /// <returns></returns>
        public WebApiRequest AddPathParam(string name, string value)
        {
            _pathParameters.AddOrUpdate(name, value);
            return this;
        }

        #endregion

        #region Query Params

        /// <summary>
        /// Add a query parameter.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <remarks>
        /// The request can have more than one parameters with the same same. In this case, the named parameter actually is an array.
        /// Example,
        /// .AddQueryParam("names","Apple")
        /// .AddQueryParam("names","Orange")
        /// </remarks>
        /// <returns></returns>
        public WebApiRequest AddQueryParam(string name, string value)
        {
            _queryParameters.Add(Tuple.Create(name, value));

            return this;
        }

        /// <summary>
        /// Add a query parameter if value is not blank.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <remarks>
        /// The request can have more than one parameters with the same same. In this case, the named parameter actually is an array.
        /// Example,
        /// .AddQueryParam("names","Apple")
        /// .AddQueryParam("names","Orange")
        /// </remarks>
        /// <returns></returns>
        public WebApiRequest AddQueryParamIf(string name, string value)
        {
            if(!string.IsNullOrEmpty(value))
                _queryParameters.Add(Tuple.Create(name, value));

            return this;
        }

        #endregion

        #region Authorization

        /// <summary>
        /// Set Basic Authorization header
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="password">Password</param>
        /// <returns></returns>
        public WebApiRequest BasicAuthorization(string username, string password)
        {
            _requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue(
                    HttpHeaderNames.AuthorizationBasic,
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", username, password))));
            
            return this;
        }


        /// <summary>
        /// Set Bearer Token Authorization header.
        /// </summary>
        /// <param name="token">Authorization token. OAuth2 uses this Bearer token for authorization.</param>
        /// <returns></returns>
        public WebApiRequest BearerAuthorization(string token)
        {
            _requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue(
                     HttpHeaderNames.AuthorizationBearer, token);

            return this;
        }

        #endregion

        #region Headers

        /// <summary>
        /// Add a request header
        /// </summary>
        /// <param name="key">Http header key</param>
        /// <param name="value">Http header value</param>
        /// <returns></returns>
        public WebApiRequest AddHeader(string key, string value)
        {
            _requestMessage.Headers.Add(key, value);

            return this;
        }


        #endregion

        #region Set Content Type

        /// <summary>
        /// Set http content type header. 
        /// The completed list of context type could be found in <a href="https://www.iana.org/assignments/media-types/media-types.xhtml">IANA</a>
        /// </summary>
        /// <param name="mimeType">MIME type of http context type.</param>
        /// <returns></returns>
        public WebApiRequest ContentType(string mimeType)
        {
            _innerContentType = mimeType.GetEnumValue(CommonContentType.Unknown,  EnumValueType.StringValue);

            if(_innerContentType == CommonContentType.MultipartFormData ||
                _innerContentType == CommonContentType.MultipartMixed)
                _isMultipart = true;

            _requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

            return this;// AddHeader(HttpHeaderNames.ContentType, mimeType);
        }

        #endregion

        #region Cookie

        /// <summary>
        /// Add/update a cookie key value
        /// </summary>
        /// <param name="key">Cookie parameter key name</param>
        /// <param name="value">Cookie parameter value associated to a key.</param>
        /// <returns></returns>
        public WebApiRequest AddCookie(string key, string value)
        {
            if (!_cookies.ContainsKey(key))
                _cookies.Add(key, value);
            else
                _cookies[key] = value;

            return this;
        }

        #endregion

        #region Body Data Type

        /// <summary>
        /// Explicitly set request body data content type.
        /// If BodyDataType is not set explictly, the system will set data conent type based on the data type.
        /// </summary>
        /// <param name="type">data content type.</param>
        /// <remarks>
        /// The body data could be parameterized added by AddBodyProperty() or directly set by BodyRaw().
        /// If choose BodyRaw, you can set any type of content type, but make sure the raw data format match to content type.
        /// If the raw data type is object, it will be serialized to json or xml if content type if json or xml.
        /// If choose AddBodyProeprty(), it only support below common content type:
        ///     1. Json
        ///     2. Xml
        ///     3. Xml8utf
        ///     4. FormUrlencoded
        ///     5. MultipartFormData or MultipartMixed
        ///     Other type may ignore and a system selected type will be applied.
        /// </remarks>
        /// <returns></returns>
        public WebApiRequest ContentType(CommonContentType type)
        {
            _innerContentType = type;
            ContentType(type.GetStringValue());
            return this;
        }

        /// <summary>
        /// Accept a specific response content type.
        /// </summary>
        /// <param name="acceptType">Http request accept type.
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept
        /// </param>
        /// <returns></returns>
        public WebApiRequest AcceptContentType(CommonContentType acceptType)
        {
            if (acceptType == CommonContentType.UnDefined || acceptType == CommonContentType.Unknown) return this;

            return AcceptContentType(acceptType.GetStringValue());
        }

        /// <summary>
        /// Accept a specific response content type
        /// </summary>
        /// <param name="acceptType"></param>
        /// <returns></returns>
        public WebApiRequest AcceptContentType(string acceptType)
        {
            _requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptType));
            return this;
        }


        /// <summary>
        /// Overwrite multipart boundary string.
        /// A default boundary is generated automatically from a guid.
        /// </summary>
        /// <param name="boundary">boundary text</param>
        /// <returns></returns>
        public WebApiRequest Boundary(string boundary)
        {
            Check.Assert<InvalidOperationException>(_innerContentType == CommonContentType.UnDefined ||
                _innerContentType == CommonContentType.MultipartFormData ||
                _innerContentType == CommonContentType.MultipartMixed,
                "The boundary overload is only available for multipart content.");

            _boundary = boundary;
            _isMultipart = true;
            return this;
        }

        #endregion

        #region Body Data

        /// <summary>
        /// Add a property name and value into the body.
        /// The body could contains property and value list in ApplicationFormUrlencoded, MultipartFormData or JSON style.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="value">The property value.
        /// The property could be different type: int, string, float/double, ...
        /// </param>
        /// <returns></returns>
        public WebApiRequest AddBodyProperty(string name, object value)
        {
            Check.Assert<InvalidOperationException>(
                _rawBody == null,
                "The AddBodyProperty() cannot mix with BodyRaw() operation");

            _dataProperties.Add(Tuple.Create(name, Tuple.Create(CommonContentType.UnDefined, value)));

            if(value is Stream || value is byte[])
                _isMultipart = true;


            return this;
        }

        /// <summary>
        /// Add a property name and value into the body with a given data type.
        /// This only for multipart mixed body data type.
        /// Each proeprty is a separated part.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="value">The property value.</param>
        /// <param name="type">Type of object data
        /// The property could be different types: int, string, float/double, binary...
        /// </param>
        /// <returns></returns>
        public WebApiRequest AddBodyProperty(string name, object value, CommonContentType type)
        {
            Check.Assert<InvalidOperationException>(
                _rawBody == null,
                "The AddBodyProperty() cannot mix with BodyRaw() operation");

            _dataProperties.Add(Tuple.Create(name, Tuple.Create(type, value)));

            _isMultipart = true;

            return this;
        }

        /// <summary>
        /// Add a property name and value into the body if the value is not blank.
        /// The body could contains property and value list in ApplicationFormUrlencoded, MultipartFormData or JSON style.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="value">The property value.
        /// The property could be different type: int, string, float/double, ...
        /// </param>
        /// <returns></returns>
        public WebApiRequest AddBodyPropertyIf(string name, object value)
        {
            if (value == null) return this;

            if (value is string && value.ToString() == "") return this;

            return AddBodyProperty(name, value);

        }

        /// <summary>
        /// Upload file
        /// </summary>
        /// <param name="filePath">file full path</param>
        /// <returns></returns>
        public WebApiRequest AddFile(string filePath)
        {
            Check.Assert<InvalidOperationException>(
                _rawBody == null,
                "The AddFile() cannot mix with BodyRaw() operation");

            FileStream fs = File.OpenRead(filePath);

            AddBodyProperty("file", fs);

            _isMultipart = true;

            return this;
        }

    /// <summary>
    /// Add raw data in body.
    /// </summary>
    /// <param name="bodyData">Raw data.
    /// The type of body Data could be:
    /// String, for text, json or xml
    /// Stream, for stream
    /// FileStream, for file
    /// byte[], for array bytes
    /// any typed objectfor json/xml
    /// </param>
    /// <returns></returns>
    public WebApiRequest BodyRaw(object bodyData)
        {
            Check.Assert<InvalidOperationException>(_dataProperties.Count == 0,
                "The BodyRaw() cannot mix with AddBodyProperty() operation");

            _rawBody = bodyData;
            _rawBodyDataType = RawBodyDataType.UnDefined;
            return this;
        }

        /// <summary>
        /// Add Json data in http body
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="bodyData">The object to be json format</param>
        /// <param name="jsonFormat">Json specific format</param>
        /// <param name="setting">Json formatting setting for object serialization</param>
        /// <returns></returns>
        public WebApiRequest JsonBody<T>(T bodyData, JsonFormat jsonFormat= JsonFormat.Default, JsonFormatSetting setting = null)
            where T:class
        {
            _rawBody = bodyData.Serialize(jsonFormat, setting);

            _rawBodyDataType = RawBodyDataType.Json;

            return this;
        }

        /// <summary>
        /// Add Json data in http body
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="bodyData">The object to be json format</param>
        /// <param name="encoding">XML encodeing. The default is UTF8</param>
        /// <returns></returns>
        public WebApiRequest XmlBody<T>(T bodyData, Encoding encoding = null)
            where T : class
        {

            if (encoding == null) encoding = Encoding.Default;

            XmlSerializer ser = new XmlSerializer(typeof(T));
            using(var writer = new MemoryStream()){
                ser.Serialize(writer, bodyData);
                using (var reader = new StreamReader(writer, encoding))
                {
                    _rawBody = reader.ReadToEnd();
                    _rawBodyDataType = RawBodyDataType.Xml;
                }
            }

            return this;
        }

        bool HasRawData
        {
            get
            {
                return _rawBody != null;
            }
        }


        #endregion

        /// <summary>
        /// Get request path
        /// </summary>
        /// <returns></returns>
        public Uri GetRequestUri()
        {
            var originalUrl = _uri.OriginalString;
            
            //Replace path parameters
            foreach(var pathParameter in _pathParameters)
            {
                originalUrl = originalUrl.Replace("{" + pathParameter.Key + "}", pathParameter.Value);
            }

            //Append query parameters
            var queryStringBuilder = new StringBuilder(originalUrl);

            for (int i = 0; i < _queryParameters.Count; i++)
            {
                if (i==0 && _uri.Query.Length == 0)
                    queryStringBuilder.Append("?");
                else
                    queryStringBuilder.Append("&");
                queryStringBuilder.AppendFormat("{0}={1}", _queryParameters[i].Item1, WebUtility.UrlEncode(_queryParameters[i].Item2));
            }

            return new Uri(queryStringBuilder.ToString());
        }

        /// <summary>
        /// Form a HttpRequestMessage instance from given arguments
        /// </summary>
        public HttpRequestMessage RequestMessage
        {
            get
            {
                //prepare a request message
                _requestMessage.RequestUri = GetRequestUri();

                SetCookies();

                PrepareData();

                return _requestMessage;
            }
        }

        #region Send

        /// <summary>
        /// Send a web api request as an asyn
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<WebApiResponse> SendAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _apiHelper.HttpClient.SendAsync(RequestMessage,
                cancellationToken)
                .ConfigureAwait(false);//do not restore context.

            return new WebApiResponse(response);
        }

        /// <summary>
        /// Send a web api request and wait it responses
        /// </summary>
        /// <returns></returns>
        public WebApiResponse Send()
        {
            return AsyncHelper.RunSync(()=>SendAsync());
        }

        #endregion

        #region Privates

        bool IsJson(string text)
        {
            if (text == null) return false;
            var jsonString = text.Trim();
            return jsonString.StartsWith("{") && jsonString.EndsWith("}");
        }

        bool IsXml(string text)
        {
            if (text == null) return false;

            var xml = text.Trim();

            if (!(xml.StartsWith("<") && xml.EndsWith(">"))) return false;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(text);
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
        }

         private void SetCookies()
        {
            if (_cookies.Count > 0)
            {
                StringBuilder cookieBuilder = new StringBuilder();
                //Apend cookies if provided
                foreach (var cookie in _cookies)
                {
                    cookieBuilder.AppendFormat(" {0}={1};", cookie.Key, cookie.Value);
                }
                AddHeader("cookie", cookieBuilder.ToString());
                _cookies.Clear();
            }
        }

        private void SetBodyFormatByRawData()
        {
            if(_rawBody !=null)
            {
                string text = _rawBody as string;

                if (text!=null)
                {
                    if (_rawBodyDataType== RawBodyDataType.Json || IsJson(text))
                    {
                        ContentType(CommonContentType.Json);
                    }
                    else if(_rawBodyDataType == RawBodyDataType.Xml || IsXml(text))
                    {
                        ContentType(CommonContentType.XmlUtf8);
                    }
                    else
                    {
                        //Set default
                        ContentType(CommonContentType.PlainText);
                    }
                }
                else
                {
                    if (_rawBody is byte[])
                    {
                        ContentType(CommonContentType.Binary);
                    }
                    else if (_rawBody is FileStream)
                    {
                        var fileStream = _rawBody as FileStream;

                        var mimeType = MimeTypeMap.GetMimeMapping(fileStream.Name);
                        if (string.IsNullOrEmpty(mimeType))
                            mimeType = CommonContentType.PlainText.GetStringValue();

                        ContentType(mimeType);
                    }
                    else if (_rawBody is Stream)
                    {
                        ContentType(CommonContentType.Binary);
                    }
                    else
                    {
                        //Any other object will be serialized as json format
                        ContentType(CommonContentType.Json);
                    }
                }
            }
        }

        void SetBodyRawData()
        {
            if (_innerContentType == CommonContentType.UnDefined)
            {
                SetBodyFormatByRawData();
            }

            _requestMessage.Content = GetContent(_rawBody, _innerContentType);
        }

        HttpContent GetContent(object itemData, CommonContentType itemType)
        {
            HttpContent content;
            if (itemData is string)
            {
                var encoding = itemType == CommonContentType.XmlUtf8 ? Encoding.UTF8 : Encoding.Default;
                content = new StringContent(itemData as string, encoding);
            }
            else if (itemData is byte[])
            {
                content = new ByteArrayContent(itemData as byte[]);
            }
            else if (itemData is FileStream)
            {
                content = new StreamContent(itemData as FileStream);
            }
            else if (itemData is Stream)
            {
                content = new StreamContent(itemData as Stream);
            }
            else //value is object which will be serialized to json format
            {
                string text;
                if (itemData == null)
                    text = "{}";
                else
                    text = itemData.Serialize();
                content = new StringContent(text, Encoding.UTF8);
            }
            return content;
        }

        private void PrepareData()
        {
            if(HasRawData)
            {
                SetBodyRawData();
                return;
            }

            if (_dataProperties.Count == 0) return;


            //Generate a default boundary key for multipart/* content
            if (_isMultipart && string.IsNullOrEmpty(_boundary))
            {
                _boundary = "qid:" + Guid.NewGuid().ToString("d");
            }

            if (_isMultipart)
            {
                if (_innerContentType == CommonContentType.UnDefined)
                    ContentType(CommonContentType.MultipartFormData);
                SetMultipartDataRequestMessage();
            }
            else
            {
                //if (_innerContentType == CommonContentType.UnDefined)
                //    ContentType(CommonContentType.FormUrlencoded);

                SetUrlFormDataRequestMessage();
            }
        }

        /// <summary>
        /// Simple form url encoded content
        /// </summary>
        void SetUrlFormDataRequestMessage()
        {
            _requestMessage.Content = new FormUrlEncodedContent(
                _dataProperties.ToDictionary(k => k.Item1, k => k.Item2.Item2 == null ? "" : k.Item2.Item2.ToString())
                .ToList());
        }

        /// <summary>
        /// set multi-part
        /// </summary>
        void SetMultipartDataRequestMessage()
        {
            //_requestMessage.Content =
            var content = new MultipartFormDataContent(_boundary);
            foreach (var item in _dataProperties)
            {
                var fileStream = item.Item2.Item2 as FileStream;
                if (fileStream == null)
                {
                    content.Add(GetContent(item.Item2.Item2, item.Item2.Item1), item.Item1);
                }
                else
                {
                    content.Add(GetContent(item.Item2.Item2, item.Item2.Item1), item.Item1, fileStream.Name);
                }
            }
            _requestMessage.Content = content;
        }

        #endregion
    }
}
