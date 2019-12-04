using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using qshine.Caching;
using qshine.oauth2;
using qshine.Utility;

namespace oauth2webpage
{

    public class OAuth2Model : PageModel
    {
        IOAuth2Provider _oauth2Provider;
        ICache _cache;

        public OAuth2Model(IOAuth2Provider oauth2Provider, ICache cache)
        {
            _oauth2Provider = oauth2Provider;
            _cache = cache;
        }


        //ApplicationEnvironment.Default.MapProvider<ICache>("unitTestCache*", cache);

        [TempData]
        public string Message { get; set; }

        public IActionResult OnGet()
        {

            var state = "1"+_oauth2Provider.GenerateUniqueId();
            var requestUrl = _oauth2Provider.AuthorizationCodeGrantUrl(state, "email");
            return Redirect(requestUrl);
        }

        public IActionResult OnGetPkce()
        {
            var state = "2"+_oauth2Provider.GenerateUniqueId();
            var verifier = _oauth2Provider.GenerateUniqueId();
            _cache.Set(state, verifier, null, new TimeSpan(1, 0, 0), CacheItemPriority.Default);
            var requestUrl = _oauth2Provider.AuthorizationCodeGrantPkceUrl(verifier, state, "email");

            return Redirect(requestUrl);
        }

        /// <summary>
        /// callback url with a code
        /// </summary>
        /// <returns></returns>
        public async Task OnGetCodeAsync()
        {
            var code = Request.Query["code"];

            string state = Request.Query["state"];

            OAuth2Token token = null;
            if (state.StartsWith("1"))
            {
                token = await _oauth2Provider.GetTokenByAuthorizationCode(code);
            }
            else if (state.StartsWith("2"))
            {
                var verifier = _cache.Get(state).ToString();
                token = await _oauth2Provider.GetTokenByAuthorizationCodePKCE(code, verifier, true);
            }
            else
            {
                throw new NotImplementedException("Unknow state");
            }

            int scopesNumber = 0;
            string scopes = "";
            if(token.Scopes!=null)
            {
                scopesNumber = token.Scopes.Length;
                scopes = String.Join(',', token.Scopes);
            }

            StringBuilder tokenStringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(token.IdToken))
            {
                var user = await _oauth2Provider.GetOpenConnectionClaims(token.IdToken);

                tokenStringBuilder.AppendFormat("Name:{0}:", user.Identity.Name);
                tokenStringBuilder.AppendFormat("Claims::");
                foreach (var c in user.Claims)
                {
                    tokenStringBuilder.AppendFormat("{0}={1}\n", c.Type, c.Value);
                }
            }


                        Message = $@"Get access token from the authorization code:
            Code={code}
            Token={token.Token}
            TokenType={token.TokenType}
            ExpiresIn={token.ExpiresIn}
            IdToken={token.IdToken}
            Scopes {scopesNumber} = {scopes}
=== Error Code: {token.ErrorCode}
=== Error Description: {token.ErrorDescription}
=== Content Data: {token.Data["rawBody"]}
=== Original Data: {token.Data.Count}
=== User: {tokenStringBuilder.ToString()}
";
        }
    }
}