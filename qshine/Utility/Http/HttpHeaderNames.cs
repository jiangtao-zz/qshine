using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Utility.Http
{
    /// <summary>
    /// Common name of http header
    /// </summary>
    public static class HttpHeaderNames
    {
        /// <summary>
        /// ContextType header name
        /// </summary>
        public static string ContentType = "Content-Type";

        /// <summary>
        /// Authorization header Basic sub-name
        /// </summary>
        public static string AuthorizationBasic = "Basic";

        /// <summary>
        /// Authorization header Bear sub-name
        /// </summary>
        public static string AuthorizationBearer = "Bearer";
    }
}
