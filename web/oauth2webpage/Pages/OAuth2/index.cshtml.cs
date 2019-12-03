using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using qshine.oauth2;
using qshine.Utility;

namespace oauth2webpage
{

    public class OAuth2Model : PageModel
    {
        static IOAuth2Provider _oauth2Provider;

        [TempData]
        public string Message { get; set; }

        public IActionResult OnGet()
        {
            if (_oauth2Provider == null)
            {
                qshine.Configuration.ApplicationEnvironment.Build();

                _oauth2Provider = qshine.Configuration.ApplicationEnvironment.
                    Default.Services.GetProvider<IOAuth2Provider>("google");

                /*new Provider(
                "Google Login",//name:
                "https://accounts.google.com",//baseUri: or authority
                "https://accounts.google.com/o/oauth2/auth",//authorizationUri:
                tokenUri: "https://oauth2.googleapis.com/token",
                resourceUri: "https://accounts.google.com/o/oauth2/auth",
                revocationUri: "https://accounts.google.com/o/oauth2/auth",
                clientId: "341533428512-nunuqidp95dsp1h067ga4l9hqnl96qh0.apps.googleusercontent.com",
                clientSecret: "M_Aa9d1Hbzao5MNS_6w1-uQg",
                clientAuthorizationMethod: "Basic",
                callbackUrl: "https://localhost:44399/OAuth2/Code");*/
            }

            var reqestUrl = _oauth2Provider.AuthorizationCodeGrantUrl("MyKey", "email");

            return Redirect(reqestUrl);
        }

        /// <summary>
        /// callback url with a code
        /// </summary>
        /// <returns></returns>
        public async Task OnGetCodeAsync()
        {


            var code = Request.Query["code"];

            var token = await _oauth2Provider.GetTokenByAuthorizationCode(code);

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