using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.oauth2
{
    /// <summary>
    /// 1. Client re-direct the request to Authorization Server through user agent.
    /// 2. User authenticates (authorization grant to client) from Authorization Server.
    /// 3. Client get access token or code from a call back URL.
    /// </summary>
    public class AuthorizationGrant
    {
        /// <summary>
        /// Required. Authorization Grant Type. 
        /// This OAuth2 request parameter name is "request_type"
        /// The valid value is:
        ///     token    - Implicit Grant
        /// </summary>
        public string ResponseType { get; set; }

        /// <summary>
        /// Required. The client identifier as described in <a href="https://tools.ietf.org/html/rfc6749#section-4.2.2">Section 2.2</a>.
        /// This OAuth2 request parameter name is "client_id"
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Optional, re-direct URI as described in <a href="https://tools.ietf.org/html/rfc6749#section-3.1.2">Section 3.1.2</a>.
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Optional, The scope of the access request as described by <a href="https://tools.ietf.org/html/rfc6749#section-3.3">Section 3.3</a>.
        /// </summary>
        public string[] Scope { get; set; }

        /// <summary>
        /// Recommended, An opaque value used by the client to maintain state between the request and callback.The authorization
        /// server includes this value when redirecting the user-agent back to the client.The parameter SHOULD be used for preventing
        /// cross-site request forgery as described in <a href="https://tools.ietf.org/html/rfc6749#section-10.12">Section 10.12.</a>
        /// </summary>
        public string State { get; set; }

    }
}
