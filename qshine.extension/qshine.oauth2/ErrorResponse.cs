using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.oauth2
{
    /// <summary>
    /// OAuth2 error response value defined under <a href="https://tools.ietf.org/html/rfc6749#section-5.2">spec.</a>
    /// </summary>
    public enum ErrorResponse
    {
        access_denied,
        invalid_request,
        invalid_client,
        invalid_grant,
        invalid_token,
        unauthorized_client,
        unsupported_grant_type,
        invalid_scope,

    }
}
