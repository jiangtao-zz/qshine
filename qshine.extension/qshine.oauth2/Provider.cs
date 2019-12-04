using qshine.Utility.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using qshine.Utility;
using System.Text;
using System.Security.Cryptography;

using System.Threading;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Logging;

namespace qshine.oauth2
{

    /// <summary>
    /// OAuth2 provider
    /// </summary>
    public class Provider:IOAuth2Provider, IDisposable
    {
        enum ClientCredentialAuthorizationMethod
        {
            None,
            Basic,
            Query
        };

        string _name;
        string _baseUri; //OAuth2 server base URI.
        string _authorizationUri; //OAuth2 Authorization server authorization endpoint URL
        string _tokenUri; //OAuth2 Authorization server token endpoint URL
        string _resourceUri; //OAuth2 Resource server endpoint URL
        string _revocationUri; //OAuth2 Authorization server revocation endpoint URL
        string _clientId; //Registered client id
        string _clientSecret; //Registered client credential
        string _callbackUrl; //Client callback URL registered in OAuth2 Authorization server
        WebApiHelper _webApiHelper;
        ClientCredentialAuthorizationMethod _clientCredentialAuthorizationMethod;

        /// <summary>
        /// Create an OAuth2 provider instance
        /// </summary>
        /// <param name="name">provider name</param>
        /// <param name="baseUri">A base uri refer to OAuth2 Authorization Server.</param>
        /// <param name="authorizationUri">A path points to OAuth2 autorization endpoint. This endpoint handle user authentication and tracking session.</param>
        /// <param name="tokenUri">A path points to OAuth2 token service endpoint. This endpoint is used to handler OAuth2 token.</param>
        /// <param name="resourceUri">A URL points to a resource server endpoint which provides basic user protected resources, 
        /// such as user name, email for the client. OAuth2 can have many resource server endpoints for different type of resource.
        /// To access the resource server it must provide corresponsing scoped access token or Access Token Type.
        /// Detail see https://tools.ietf.org/html/rfc6749#section-7.
        /// </param>
        /// <param name="clientId">OAuth2 client credential of client id for your application.</param>
        /// <param name="clientSecret">OAuth2 client credential of client secret for your application.
        /// The client secret is an option parameter for "confidential client" (not mobile and javascript).
        /// If it provided, it should not be expose to public.
        /// </param>
        /// <param name="clientAuthorizationMethod">Indicates how to pass OAuth2 client credential to authorization server.
        /// The validate values are:
        ///     Basic - pass cleint credential through Basic Authorization Header.
        ///     Query - pass cleint credential through query parameters.
        /// </param>
        /// <param name="callbackUrl">Callback url register in OAuth2 server. </param>
        public Provider(
            string name, 
            string baseUri, 
            string authorizationUri,
            string tokenUri, 
            string resourceUri,
            string revocationUri,
            string clientId, 
            string clientSecret,
            string clientAuthorizationMethod,
            string callbackUrl)
        {
            _name = name;
            _baseUri = baseUri;
            _authorizationUri = authorizationUri;
            _tokenUri = tokenUri;
            _resourceUri = resourceUri;
            _revocationUri = revocationUri;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _callbackUrl = callbackUrl;
            _webApiHelper = new WebApiHelper(baseUri);

            if (!string.IsNullOrEmpty(clientAuthorizationMethod))
            {
                _clientCredentialAuthorizationMethod =
                    clientAuthorizationMethod.GetEnumValue<ClientCredentialAuthorizationMethod>(ClientCredentialAuthorizationMethod.Basic,
                    EnumValueType.OriginalString);
            }
        }

        /// <summary>
        /// Get/Set callback url.
        /// </summary>
        public string CallbackUrl
        {
            get
            {
                return _callbackUrl;
            }
            set
            {
                _callbackUrl = value;
            }
        }

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
        public string AuthorizationCodeGrantUrl(string state, string scope)
        {
            return
                _webApiHelper
                .NewGetRequest(_authorizationUri)
                .AddQueryParam("response_type", "code")
                .AddQueryParamIf("scope", scope)
                .AddQueryParamIf("state", state)
                .AddQueryParam("redirect_uri", CallbackUrl)
                .AddQueryParam("client_id", _clientId)
                .GetRequestUri().AbsoluteUri;
        }


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
        public async Task<OAuth2Token> GetTokenByAuthorizationCode(string code, bool supportBodyParameter=false)
        {
            var request = _webApiHelper
                .NewPostRequest(_tokenUri)
                .AddBodyProperty("grant_type", "authorization_code")
                .AddBodyProperty("code", code)
                .AddBodyProperty("redirect_uri", CallbackUrl);

            if (supportBodyParameter)
            {
                request.AddBodyProperty("client_id", _clientId)
                .AddBodyProperty("client_secret", _clientSecret);
            }
            else
            {
                request.BasicAuthorization(_clientId, _clientSecret);
            }

            var response = await request.SendAsync();

            return await ReadTokenFromResponse(response);
        }

        #endregion

        #region OAuth2 Authorization Code Grant With PKCE

        /// <summary>
        /// Get OAuth2 Authorization Code Grant request to authorization server with PKCE for Native App user login.
        /// See RFC7636:: https://tools.ietf.org/html/rfc7636
        /// This is the extension of OAuth2 Authorization Code Grant.
        /// </summary>
        /// <param name="codeVerifier">A plain verifier code.</param>
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
        public string AuthorizationCodeGrantPkceUrl(string codeVerifier, string state, string scope, 
            bool isPlainPKCE=false, string responseMode="")
        {

            string codeChallenger = codeVerifier;
            if(!isPlainPKCE)
            {
                //transform the code verifier to code challenger
                using (var sha256 = SHA256.Create())
                {
                    var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                    codeChallenger = Convert.ToBase64String(challengeBytes)
                      .Replace('+', '-')
                      .Replace('/', '_')
                      .Replace("=", "");
                }
            }

            return 
                _webApiHelper
                .NewGetRequest(_authorizationUri)
                .AddQueryParam("response_type", "code")
                .AddQueryParamIf("response_mode", responseMode)
                .AddQueryParam("scope", scope)
                .AddQueryParam("state", state)
                .AddQueryParam("redirect_uri", CallbackUrl)
                .AddQueryParam("client_id", _clientId)
                .AddQueryParam("code_challenge_method", isPlainPKCE?"plain": "S256")
                .AddQueryParam("code_challenge", codeChallenger)
                .GetRequestUri().AbsoluteUri;
        }

        /// <summary>
        /// Get a secure and random string
        /// </summary>
        /// <returns></returns>
        public string GenerateUniqueId()
        {
            using (var provider = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[16];
                provider.GetBytes(bytes);

                return new Guid(bytes).ToString("N");
            }
        }


        /// <summary>
        /// Get OAuth2 token using authorization code and verifier.
        /// </summary>
        /// <param name="callbackUri">A uri previously sent to authorization server for user authorization code.
        /// This callback uri must be register in authorization server.
        /// </param>
        /// <param name="code">An authorization code received from callbackUri in previous request.</param>
        /// <param name="code_verifier">A code verifier originally sent to retrieve the code</param>
        /// <param name="needClientSecretKey">Indicates client secret key required. In some system the client secret key is still required</param>
        /// <returns>Returns a token contains authorization token and refresh token and expires in seconds.</returns>
        public async Task<OAuth2Token> GetTokenByAuthorizationCodePKCE(string code, string codeVerifier, bool needClientSecretKey)
        {
            var request = _webApiHelper
                .NewPostRequest(_tokenUri)
                .AddBodyProperty("grant_type", "authorization_code")
                .AddBodyProperty("code", code)
                .AddBodyProperty("redirect_uri", CallbackUrl)
                .AddBodyProperty("client_id", _clientId)
                .AddBodyPropertyIf("client_secret", needClientSecretKey?_clientSecret:"")
                .AddBodyProperty("code_verifier", codeVerifier);

            var response = await request.SendAsync();

            return await ReadTokenFromResponse(response);
        }

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
        public async Task<OAuth2Token> PasswordGrant(string userName, string password, string scope = null)
        {
            var request =
                 _webApiHelper
                .NewPostRequest(_authorizationUri)
                .AddBodyProperty("grant_type", "password")
                .AddBodyProperty("username", userName)
                .AddBodyProperty("password", password);
            if(_clientCredentialAuthorizationMethod == ClientCredentialAuthorizationMethod.None)
            {
                //do not need client secret.
                request.AddBodyProperty("client_id", _clientId);
            }
            else if(_clientCredentialAuthorizationMethod == ClientCredentialAuthorizationMethod.Query)
            {
                request.AddBodyProperty("client_id", _clientId)
                .AddBodyPropertyIf("client_secret", _clientSecret);
            }
            else
            {
                request.BasicAuthorization(_clientId, _clientSecret);
            }
            request.AddBodyPropertyIf("scope", scope);

            var response = await request.SendAsync();

            return await ReadTokenFromResponse(response);
        }

        #endregion

        #region OAuth2 Client Credentials Grant

        /// <summary>
        /// Make OAuth 2.0 Client Credentials Grant
        /// </summary>
        /// <param name="scope">A space-delimited list of scopes that identify the resources the OAuth2 token applied.</param>
        /// <returns></returns>
        public async Task<OAuth2Token> ClientCredentialsGrant(string scope = null)
        {
            var request =
                 _webApiHelper
                .NewPostRequest(_authorizationUri)
                .AddBodyProperty("grant_type", "client_credentials");

            if (_clientCredentialAuthorizationMethod == ClientCredentialAuthorizationMethod.None)
            {
                //do not need client secret.
                request.AddBodyProperty("client_id", _clientId);
            }
            else if (_clientCredentialAuthorizationMethod == ClientCredentialAuthorizationMethod.Query)
            {
                request.AddBodyProperty("client_id", _clientId)
                .AddBodyPropertyIf("client_secret", _clientSecret);
            }
            else
            {
                request.BasicAuthorization(_clientId, _clientSecret);
            }
            request.AddBodyPropertyIf("scope", scope);

            var response = await request.SendAsync();

            return await ReadTokenFromResponse(response);
        }

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
        public async Task<WebApiResponse> ImplicitGrant(string state, string scope)
        {
            return await
                _webApiHelper
                .NewGetRequest(_authorizationUri)
                .AddBodyProperty("response_type", "token")
                .AddBodyProperty("client_id", _clientId)
                .AddBodyProperty("redirect_uri", CallbackUrl)
                .AddBodyPropertyIf("scope", scope)
                .AddBodyProperty("state", state)
                .SendAsync();
        }

        #endregion

        #region OAuth2 Refresh Token
        /// <summary>
        /// Get OAuth2 token by original issued refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token previously issued to the client.</param>
        /// <param name="scope">It is the original scope which should not include aditional scopes.
        /// Usually, it is not required.</param>
        /// <returns></returns>
        public async Task<OAuth2Token> RefreshOAuth2Token(string refreshToken, string scope = null)
        {
            var request =
                 _webApiHelper
                .NewPostRequest(_authorizationUri)
                .AddBodyProperty("grant_type", "refresh_token")
                .AddBodyProperty("refresh_token",refreshToken)
                .AddBodyPropertyIf("scope", scope)
                ;

            if (_clientCredentialAuthorizationMethod == ClientCredentialAuthorizationMethod.Query)
            {
                request.AddBodyProperty("client_id", _clientId)
                .AddBodyPropertyIf("client_secret", _clientSecret);
            }
            else
            {
                request.BasicAuthorization(_clientId, _clientSecret);
            }

            var response = await request.SendAsync();

            return await ReadTokenFromResponse(response);
        }

        #endregion

        #region Revocate an OAuth2 token

        /// <summary>
        /// Revocates an OAuth2 access token or refresh token.
        /// </summary>
        /// <param name="token">Access token or refresh token</param>
        /// <param name="isRefreshToken">Indicates a refresh token or access token.</param>
        /// <returns></returns>
        public async Task<bool> RevocateOAuth2Token(string token, bool isRefreshToken=false)
        {
            var request =
                 _webApiHelper
                .NewPostRequest(_authorizationUri)
                .AddBodyProperty("token", token)
                .AddBodyProperty("token_type_hint", isRefreshToken? "refresh_token": "access_token")
                ;

            if (_clientCredentialAuthorizationMethod == ClientCredentialAuthorizationMethod.Query)
            {
                request.AddBodyProperty("client_id", _clientId)
                .AddBodyPropertyIf("client_secret", _clientSecret);
            }
            else
            {
                request.BasicAuthorization(_clientId, _clientSecret);
            }

            var response = await request.SendAsync();

            return response.IsSuccessStatusCode;
        }

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
        public async Task<UserInfo> GetUserInfo(string token, string scope, 
            Action<UserInfo, Dictionary<string,object>> userPropertyMap, string resourceUrl="")
        {
            if (string.IsNullOrEmpty(resourceUrl))
                resourceUrl = _resourceUri;
            var request =
                 _webApiHelper
                .NewGetRequest(resourceUrl)
                .AddBodyPropertyIf("scope", scope)
                .BearerAuthorization(token)
                ;

            var response = await request.SendAsync();

            return await response.GetJsonResult<UserInfo>(userPropertyMap);
        }

        #endregion

        OpenIdConnectConfiguration _openIdConfig = null;
        TokenValidationParameters _tokenValidationParameters;
        JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();

        /// <summary>
        /// Get Claim Principal from open id it token.
        /// </summary>
        /// <param name="idToken">Id token</param>
        /// <returns></returns>
        public async Task<ClaimsPrincipal> GetOpenConnectionClaims(string idToken)
        {
            string auth0Domain = _baseUri;
            string auth0Audience = _clientId;

            if (_openIdConfig == null)
            {
                IConfigurationManager<OpenIdConnectConfiguration> configurationManager =
                    new ConfigurationManager<OpenIdConnectConfiguration>($"{auth0Domain}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                _openIdConfig =
                    await configurationManager.GetConfigurationAsync(CancellationToken.None);
            }

            if (_tokenValidationParameters == null)
            {
                _tokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidIssuer = auth0Domain,
                        ValidAudiences = new[] { auth0Audience },
                        IssuerSigningKeys = _openIdConfig.SigningKeys,
                    };
                IdentityModelEventSource.ShowPII = true;
            }

            SecurityToken validatedToken;
            var user =
                _tokenHandler.ValidateToken(idToken,
                _tokenValidationParameters, out validatedToken);

            return user;
        }

        private async Task<OAuth2Token> ReadTokenFromResponse(WebApiResponse response)
        {
            return await response.GetJsonResult<OAuth2Token>(
                (r, d) =>
                {
                    if (r.IsSuccess)
                    {
                        r.Token = d.GetString("access_token");
                        r.RefreshToken = d.GetString("refresh_token");
                        r.TokenType = d.GetString("token_type");
                        r.ExpiresIn = d.GetCastValue("expires_in", 0);
                        r.RefreshTokenExpiresIn = d.GetCastValue("refresh_expires_in", 0);
                        r.IdToken = d.GetString("id_token");
                        var scopes = d.GetString("scope");
                        if (!string.IsNullOrEmpty(scopes))
                        {
                            r.Scopes = scopes.Split(' ');
                        }
                    }
                    else
                    {
                        //The valid error code are:
                        //invalid_request, invalid_client, invalid_grant, invalid_scope, unauthorized_client, unsupported_grant_type
                        r.ErrorCode = d.GetString("error");
                        r.ErrorDescription = d.GetString("error_description");
                        r.ErrorUrl = d.GetString("error_uri");
                        if (string.IsNullOrEmpty(r.ErrorDescription))
                        {
                            r.ErrorDescription = d.GetString("rawData");
                        }
                    }
                }
                );
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _webApiHelper.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Provider() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
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
