using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.oauth2
{
    /// <summary>
    /// OAuth2 client key and secret defined in RFC 6749
    /// <see cref="https://tools.ietf.org/html/rfc6749#section-11.2.2"/>
    /// </summary>
    public class OAuth2Client
    {
        /// <summary>
        /// OAuth2 Client Id.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// OAuth2 Client Secret.
        /// </summary>
        public string ClientSecret { get; set; }
    }
}
