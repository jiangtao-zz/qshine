using qshine.Utility.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace qshine.oauth2
{
    /// <summary>
    /// Request and Response OAuth2 access/refresh token from Client to Authorization Server.
    /// 
    /// See <a href="https://tools.ietf.org/html/rfc6749#section-4.2.2">Section 4.2.2</a>.
    /// 
    /// </summary>
    public class OAuth2Token:WebApiResult
    {
        /// <summary>
        /// Request. REQUIRED,
        /// Access token request grant_type.
        /// The valid value is:
        ///     authorization_code - <a href="https://tools.ietf.org/html/rfc6749#section-4.1">Authorization Code Grant</a>.
        ///     password - <a href="https://tools.ietf.org/html/rfc6749#section-4.3">Password Credentials Grant</a>.
        ///     client_credentials - <a href="https://tools.ietf.org/html/rfc6749#section-4.4">Client Credentials Grant</a>.
        ///     refresh_token - <a href="https://tools.ietf.org/html/rfc6749#section-6">Refreshing an Access Token</a>.
        /// </summary>
        public string GrantType { get; set; }

        /// <summary>
        /// Request. REQUIRED if grant_type is "authorization_code".
        /// See <a href="https://tools.ietf.org/html/rfc6749#section-4.1.3">Section 4.1.3</a>.
        /// The authorization code received from the authorization server.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Request. REQUIRED if grant_type is "authorization_code".
        /// if the "redirect_uri" parameter was included in the authorization request as described 
        /// in <a href="https://tools.ietf.org/html/rfc6749#section-4.1.1">Section 4.1.1</a>, and their values MUST be identical.
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Request. REQUIRED if grant_type is "authorization_code".
        /// if the client is not authenticating with the authorization server as described in <a href="https://tools.ietf.org/html/rfc6749#section-3.2.1">Section 3.2.1</a>.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Request. REQUIRED  if grant_type is "password". See <a href="https://tools.ietf.org/html/rfc6749#section-4.3">Section 4.3.</a>.
        /// The resource owner username.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Request. REQUIRED if grant_type is "password".
        /// The resource owner password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Response. REQUIRED. 
        /// The access token issued by the authorization server.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Response. REQUIRED. 
        /// The type of the token issued as described in <a href="https://tools.ietf.org/html/rfc6749#section-7.1">Section 7.1.</a>.
        /// Value is case insensitive.
        /// A common token type is "Bearer" defined in  <a href="https://tools.ietf.org/html/rfc6750">The OAuth 2.0 Authorization Framework: Bearer Token Usage</a>.
        /// </summary>
        public string TokenType { get; set; }

        /// <summary>
        /// Response. RECOMMENDED. The lifetime in seconds of the access token.
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        ///  Request. REQUIRED if grant_type is "refresh_token".   
        ///  The refresh token issued to the client.
        ///  
        ///  Response. OPTIONAL if grant_type is "authorization_code", "password".
        ///  The refresh token, which can be used to obtain new
        ///  access tokens using the same authorization grant as described
        ///  in <a href="https://tools.ietf.org/html/rfc6749#section-6">Section 6</a>.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// The lifetime in seconds of the refresh token.
        /// </summary>
        public int RefreshTokenExpiresIn { get; set; }

        /// <summary>
        /// Request. Response. OPTIONAL, 
        /// if identical to the scope requested by the client;
        /// otherwise, REQUIRED.The scope of the access token as
        /// described by <a href="https://tools.ietf.org/html/rfc6749#section-3.3">Section 3.3</a>.
        /// </summary>
        public string[] Scopes { get; set; }

        /// <summary>
        /// Response, Required, if the "state" parameter was present in the client
        /// authorization request.The exact value received from the client.
        /// Also see <seealso cref="AuthorizationGrant.State"/>.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Token issue time
        /// </summary>
        public long IssuedOn { get; set; }


        /// <summary>
        /// OpenID token
        /// </summary>
        public string IdToken
        {
            get;set;
        }


    }
}
