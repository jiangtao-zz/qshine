using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace qshine.Utility.Http
{
    /// <summary>
    /// Web api response
    /// </summary>
    public class WebApiResponse
    {
        HttpResponseMessage _response;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="response"></param>
        public WebApiResponse(HttpResponseMessage response)
        {
            _response = response;
        }

        /// <summary>
        /// Get original Http response message instance
        /// </summary>
        public HttpResponseMessage ResponseMessage
        {
            get
            {
                return _response;
            }
        }

        /// <summary>
        /// Determine response error or success.
        /// </summary>
        public bool IsSuccessStatusCode
        {
            get
            {
                return _response.IsSuccessStatusCode;
            }
        }

        /// <summary>
        /// Get byte array data
        /// </summary>
        /// <returns></returns>
        public byte[] GetByteArray()
        {
            return AsyncHelper.RunSync(() => _response.Content.ReadAsByteArrayAsync());
        }

        /// <summary>
        /// Get file name from http response header Content-Disposition.
        /// </summary>
        /// <returns></returns>
        public string GetFileName()
        {
            if (_response.Content.Headers.ContentDisposition == null)
            {
                //fix issue from server which doesn't fully follow RFC2616 and RFC6266
                //https://stackoverflow.com/questions/21008499/httpresponsemessage-content-headers-contentdisposition-is-null
                IEnumerable<string> contentDisposition;
                if (_response.Content.Headers.TryGetValues("Content-Disposition", out contentDisposition))
                {
                    _response.Content.Headers.ContentDisposition =
                        ContentDispositionHeaderValue.Parse(contentDisposition.ToArray()[0].TrimEnd(';').Replace("\"", ""));
                }
            }

            if (_response.Content.Headers.ContentDisposition != null)
                return _response.Content.Headers.ContentDisposition.FileName;
            else
                return "";
        }

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="fileName">File name to be saved. 
        /// Default name is from response.</param>
        /// <returns></returns>
        public async Task DownloadAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = GetFileName();
            }
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("File name cannot be null.");

            using (Stream contentStream = await _response.Content.ReadAsStreamAsync())
            {
                using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write,
                    FileShare.None, 0, true))
                {
                    await contentStream.CopyToAsync(stream);
                }
            }
        }


        /// <summary>
        /// Get text result
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            return AsyncHelper.RunSync(() => GetTextAsync());
        }

        /// <summary>
        /// Get text result in async
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetTextAsync()
        {
            return await _response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Parse json formatted response content to typed object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> GetJsonData<T>()
        {
            return (await GetTextAsync()).Deserialize<T>();
        }

        /// <summary>
        /// Retrieve json formatted response content to WebApiResult typed object by mapping
        /// </summary>
        /// <typeparam name="T">Typeof result</typeparam>
        /// <param name="map">mapping json dictionary to typed object property</param>
        /// <returns></returns>
        public async Task<T> GetJsonResult<T>(Action<T, Dictionary<string,object>> map)
            where T: WebApiResult, new()
        {
            var content = await _response.Content.ReadAsStringAsync();

            Dictionary<string, object> data;
            if (CommonContentType == CommonContentType.Json)
            {
                data = content.DeserializeDictionary();
            }
            else
            {
                data = new Dictionary<string, object>
                {
                    {"rawBody",content}
                };
            }


            T result = new T();
            result.Data = data;
            result.Data.AddOrUpdate("rawBody", content);
            result.IsSuccess = _response.IsSuccessStatusCode;
            result.HttpStatusCode = _response.StatusCode;

            //Map properties
            map(result, data);

            return result;
        }

        CommonContentType _commonContentType = CommonContentType.UnDefined;
        /// <summary>
        /// Get common content type
        /// </summary>
        public CommonContentType CommonContentType
        {
            get
            {
                if (_commonContentType == CommonContentType.UnDefined)
                {
                    if (_response.Content != null && _response.Content.Headers.ContentType != null)
                    {
                        _commonContentType =
                            _response.Content.Headers.ContentType.MediaType.GetEnumValue(CommonContentType.Unknown, EnumValueType.StringValue);
                    }
                }
                return _commonContentType;
            }
        }

        ///// <summary>
        ///// Get text result
        ///// </summary>
        ///// <returns></returns>
        //public string GetText()
        //{
        //    return AsyncHelper.RunSync(() => _response.Content.ReadAsStringAsync());
        //}





    }
}
