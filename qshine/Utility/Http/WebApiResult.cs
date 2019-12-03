using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace qshine.Utility.Http
{
    /// <summary>
    /// Web api result
    /// </summary>
    public class WebApiResult
    {
        /// <summary>
        /// Indicates a successful web api request
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// OAuth2 HTTP response code.
        /// A successful request always return 200.
        /// A failed request returns 400 status code (unless specified otherwise)
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; set; }

        #region Unsuccessful request information

        /// <summary>
        /// Error type
        /// </summary>
        public string ErrorType { get; set; }

        /// <summary>
        /// Error code if the OAuth2 token is invalid
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Detail error description
        /// </summary>
        public string ErrorDescription { get; set; }

        /// <summary>
        /// A URL for invalid OAuth2 token
        /// </summary>
        public string ErrorUrl { get; set; }

        /// <summary>
        /// Raw data from request
        /// </summary>
        public Dictionary<string, object> Data { get; set; }

        #endregion

    }
}
