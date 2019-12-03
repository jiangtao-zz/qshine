using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace qshine.Utility.Http
{
    /// <summary>
    /// Help to make RESTful web api call from the desktop/web application.
    /// 
    /// var apiHelper = new WebApiHelper("https://streetviewpublish.googleapis.com/v1");
    /// 
    /// apiHelper.NewGetRequest("photo/{photoId}")
    ///     .
    /// </summary>
    public class WebApiHelper : IDisposable
    {
        private Uri _baseUri;
        private HttpClient _httpClient;
        private bool _isExternalHttpClient;

        /// <summary>
        /// Defaul Ctro.
        /// </summary>
        public WebApiHelper()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Create an instance with base uri for api call.
        /// </summary>
        /// <param name="baseUri">base uri</param>
        public WebApiHelper(string baseUri)
        {
            _baseUri = new Uri(baseUri);
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Create an instance with base uri for api call.
        /// </summary>
        /// <param name="baseUri">base uri</param>
        public WebApiHelper(Uri baseUri)
        {
            _baseUri = baseUri;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Create an instance wth base uri and HttpClient instance pass from outside
        /// </summary>
        /// <param name="baseUri"></param>
        /// <param name="httpClient"></param>
        public WebApiHelper(Uri baseUri, HttpClient httpClient)
        {
            _baseUri = baseUri;
            _httpClient = httpClient;
            _isExternalHttpClient = true;
        }

        /// <summary>
        /// Convert to a request Uri.
        /// </summary>
        /// <param name="uri">It could be a relative uri or absolute uri</param>
        /// <returns>returns an absolute Uri </returns>
        public Uri RequestUri(string uri)
        {
            Uri requestUri;
            if (_baseUri != null)
            {
                requestUri = new Uri(_baseUri, uri);
            }
            else
            {
                requestUri = new Uri(uri);
            }
            return requestUri;
        }

        /// <summary>
        /// Create a GET request
        /// </summary>
        /// <param name="uri">request uri. It could be a relative uri or absolute uri</param>
        /// <returns>Return a new GET api request.</returns>
        public WebApiRequest NewGetRequest(string uri)
        {
            return new WebApiRequest(this, System.Net.Http.HttpMethod.Get, RequestUri(uri));
        }

        /// <summary>
        /// Create a POST request
        /// </summary>
        /// <param name="uri">request uri. It could be a relative uri or absolute uri</param>
        /// <returns>Return a new POST api request.</returns>
        public WebApiRequest NewPostRequest(string uri)
        {
            return new WebApiRequest(this, System.Net.Http.HttpMethod.Post, RequestUri(uri));
        }

        /// <summary>
        /// Create a PUT request
        /// </summary>
        /// <param name="uri">request uri. It could be a relative uri or absolute uri</param>
        /// <returns>Return a new POST api request.</returns>
        public WebApiRequest NewPutRequest(string uri)
        {
            return new WebApiRequest(this, System.Net.Http.HttpMethod.Put, RequestUri(uri));
        }

        /// <summary>
        /// Create a PATCH request
        /// </summary>
        /// <param name="uri">request uri. It could be a relative uri or absolute uri</param>
        /// <returns>Return a new PATCH api request.</returns>
        public WebApiRequest NewPatchRequest(string uri)
        {
            return new WebApiRequest(this, new System.Net.Http.HttpMethod("PATCH"), RequestUri(uri));
        }

        /// <summary>
        /// Create a DELETE request
        /// </summary>
        /// <param name="uri">request uri. It could be a relative uri or absolute uri</param>
        /// <returns>Return a new DELETE api request.</returns>
        public WebApiRequest NewDeleteRequest(string uri)
        {
            return new WebApiRequest(this, System.Net.Http.HttpMethod.Delete, RequestUri(uri));
        }

        /// <summary>
        /// Create a HEAD request
        /// </summary>
        /// <param name="uri">request uri. It could be a relative uri or absolute uri</param>
        /// <returns>Return a new HEAD api request.
        /// Teh HEAD request similar as GET request but without the response body
        /// </returns>
        public WebApiRequest NewHeadRequest(string uri)
        {
            return new WebApiRequest(this, System.Net.Http.HttpMethod.Head, RequestUri(uri));
        }

        /// <summary>
        /// Get HttpClient instance
        /// </summary>
        public HttpClient HttpClient
        {
            get
            {
                return _httpClient;
            }
         }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (!_isExternalHttpClient)
                    {
                        _httpClient.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~WebApiHelper() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        ///  This code added to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
