using qshine.Utility.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace qshine.oauth2
{
    /// <summary>
    /// IOAuth2 Provider
    /// </summary>
    public interface IOAuth2Provider:IProvider
    {
        /// <summary>
        /// Get/Set callback URL
        /// </summary>
        string CallbackUrl { get; set; }

        #region OAuth2 Authorization Code Grant

        /// <summary>
        /// Get OAuth2 Authorization Code Grant request URL .
        /// After the request sent to authorization server, the authorization server will ask user to enter user credentials
        /// for user authentication. 
        /// When the user got the authorization, the callbackUri will receive a request from authorization server.
        /// A authorization code will be embedded in callback url 
        /// </summary>
        /// <param name="callbackUri">A callback uri to receive authorization code from OAuth2 server.</param>
        /// <param name="state">Any value used to maintain state between this request and callback request from authorization server. 
        /// The state can be used to aganst CSRF and also be used to persist data between original request and authorization code callback process
        /// </param>
        /// <param name="scope">A space-delimited list of scopes that identify the resources the OAuth2 token applied.</param>
        /// <returns>Returns a request Url which will be redirect to authorization server..</returns>
        /// <remarks>
        /// See explain in https://www.oauth.com/oauth2-servers/server-side-apps/authorization-code/.
        /// </remarks>
        string AuthorizationCodeGrantUrl(string state, string scope);

        /// <summary>
        /// Get OAuth2 token using authorization code.
        /// </summary>
        /// <param name="callbackUri">A uri previously sent to authorization server for user authorization code.
        /// This callback uri must be register in authorization server.
        /// </param>
        /// <param name="code">An authorization code received from callbackUri in previous request.</param>
        /// <param name="supportBodyParameter">Indicates the authorization server accept Client Authentication by body parameters.
        /// The default Client Authentication need be pass from Basic Auth header
        /// </param>
        /// <returns>Returns a token contains authorization token and refresh token and expires in seconds.</returns>
        Task<OAuth2Token> GetTokenByAuthorizationCode(string code, bool supportBodyParameter = false);
        #endregion

        #region OAuth2 Authorization Code Grant With PKCE

        /// <summary>
        /// Send OAuth2 Authorization Code Grant request to authorization server with PKCE for Native App user login.
        /// See RFC7636:: https://tools.ietf.org/html/rfc7636
        /// This is the extension of OAuth2 Authorization Code Grant.
        /// </summary>
        /// <param name="callbackUri">A callback uri to receive authorization code from OAuth2 server.</param>
        /// <param name="state">Any value used to maintain state between this request and callback request from authorization server. 
        /// The state can be used to aganst CSRF and also be used to persist data between original request and authorization code callback process
        /// </param>
        /// <param name="scope">A space-delimited list of scopes that identify the resources the OAuth2 token applied.</param>
        /// <param name="isPlainPKCE">A method of transformation on verification code.
        /// Teh validate value is S256 or plain</param>
        /// <param name="responseMode">The response mode is an optional. The default value is "query" for CODE and "fragment" for token.
        /// For javascript app, the mode usually is "fragment". </param>
        /// <returns>Indicates the OAuth2 Authorization Code Grant request sucessfully sent to authorization server.</returns>
        /// <remarks>
        /// See explain in https://www.oauth.com/oauth2-servers/server-side-apps/authorization-code/.
        /// </remarks>
        Task<WebApiResponse> AuthorizationCodeGrantPKCE(string codeVerifier, string state, string scope,
            bool isPlainPKCE = false, string responseMode = "");

        /// <summary>
        /// Get a secure and random string
        /// </summary>
        /// <returns></returns>
        string GenerateUniqueId();

        /// <summary>
        /// Get OAuth2 token using authorization code and verifier.
        /// </summary>
        /// <param name="callbackUri">A uri previously sent to authorization server for user authorization code.
        /// This callback uri must be register in authorization server.
        /// </param>
        /// <param name="code">An authorization code received from callbackUri in previous request.</param>
        /// <param name="code_verifier">A code verifier originally sent to retrieve the code</param>
        /// <returns>Returns a token contains authorization token and refresh token and expires in seconds.</returns>
        Task<OAuth2Token> GetTokenByAuthorizationCodePKCE(string code, string codeVerifier);
        #endregion

        #region OAuth2 Password Grant - Legacy Limited Use.
        /// <summary>
        /// Send OAuth2 Password Grant request to authorization server and returns OAuth2 token.
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="password">password</param>
        /// <param name="scope">A space-delimited list of scopes that identify the resources the OAuth2 token applied.</param>
        /// <returns></returns>
        /// <remarks>
        /// The Password grant will store password in client application. It should not be used in 3rd-party application.
        /// Detail see: https://oauth.net/2/grant-types/password/
        /// </remarks>
        Task<OAuth2Token> PasswordGrant(string userName, string password, string scope = null);

        #endregion

        #region OAuth2 Client Credentials Grant

        /// <summary>
        /// Make OAuth 2.0 Client Credentials Grant
        /// </summary>
        /// <param name="scope">A space-delimited list of scopes that identify the resources the OAuth2 token applied.</param>
        /// <returns></returns>
        Task<OAuth2Token> ClientCredentialsGrant(string scope = null);

        #endregion

        #region OAuth2 Implict Grant - Legacy Security Risk

        /// <summary>
        /// Make OAuth 2.0 Implicit Grant.
        /// The Implicit Grant is only used by Single Page Web Application which cannot keep client credentials in the app.
        /// </summary>
        /// <param name="scope">A space-delimited list of scopes that identify the resources the OAuth2 token applied.</param>
        /// <returns></returns>
        /// <remarks>
        /// This is a legacy OAuth2 authentication grant. It has big security risk in moden system.
        /// Detail see: https://oauth.net/2/grant-types/implicit/
        /// </remarks>
        Task<WebApiResponse> ImplicitGrant(string state, string scope);

        #endregion

        #region OAuth2 Refresh Token
        /// <summary>
        /// Get OAuth2 token by original issued refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token previously issued to the client.</param>
        /// <param name="scope">It is the original scope which should not include aditional scopes.
        /// Usually, it is not required.</param>
        /// <returns></returns>
        Task<OAuth2Token> RefreshOAuth2Token(string refreshToken, string scope = null);

        #endregion

        #region Revocate an OAuth2 token

        /// <summary>
        /// Revocates an OAuth2 access token or refresh token.
        /// </summary>
        /// <param name="token">Access token or refresh token</param>
        /// <param name="isRefreshToken">Indicates a refresh token or access token.</param>
        /// <returns></returns>
        Task<bool> RevocateOAuth2Token(string token, bool isRefreshToken = false);

        #endregion

        #region Get User Resource by Scoped token

        /// <summary>
        /// Get user information from by given token
        /// </summary>
        /// <param name="token">OAuth2 token</param>
        /// <param name="scope">resource scope</param>
        /// <param name="userPropertyMap">a delegate map json key/value to UserInfo object</param>
        /// <param name="resourceUrl">User information resource endpoint url.
        /// Use provider resource url if the url is empty.
        /// </param>
        /// <returns></returns>
        Task<UserInfo> GetUserInfo(string token, string scope,
            Action<UserInfo, Dictionary<string, object>> userPropertyMap, string resourceUrl = "");
        #endregion

        /// <summary>
        /// Get Claim Principal from open id it token.
        /// </summary>
        /// <param name="idToken">Id token</param>
        /// <returns></returns>
        Task<ClaimsPrincipal> GetOpenConnectionClaims(string idToken);
    }
}
