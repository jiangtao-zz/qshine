//using System;
//using System.Collections.Generic;
//using System.Text;
//using qshine.Utility.Http;

//namespace qshine.oauth2
//{
//    public class AccessTokenRequest
//    {

//        /// <summary>
//        /// Get grant response
//        /// </summary>
//        /// <param name="token"></param>
//        /// <param name="refreshToken"></param>
//        /// <param name="expiresIn"></param>
//        /// <param name="scope"></param>
//        /// <returns></returns>
//        public WebApiResponse GrantResponse(string token, int expiresIn, string refreshToken = "", string scope = null)
//        {
//            return
//                new WebApiHelper()
//                .NewResponse
//                .Status(200)
//                .JSON()
//                .TurnOffCache()
//                .WithData("access_token", token)
//                .WithData("token_type", "bearer")
//                .WithData("expires_in", expiresIn)
//                .WithDataIf("refresh_token", refreshToken)
//                .WithDataIf("scope", scope)
//                ;
//        }

//        /// <summary>
//        /// Get error response
//        /// </summary>
//        /// <param name="error"></param>
//        /// <param name="errorDescription"></param>
//        /// <param name="errorUri"></param>
//        /// <returns></returns>
//        public WebApiResponse ErrorResponse(ErrorResponse error, string errorDescription=null, string errorUri=null)
//        {
//            return
//                new HttpHelper()
//                .Status(400)
//                .JSON()
//                .TurnOffCache()
//                .WithData("error", error.ToString())
//                .WithDataIf("error_description", errorDescription)
//                .WithDataIf("error_uri", errorUri)
//                ;
//        }



//        /// <summary>
//        /// Get Client Credentials grant request
//        /// </summary>
//        /// <param name="clientId"></param>
//        /// <param name="clientSecret"></param>
//        /// <param name="scope"></param>
//        /// <returns></returns>
//        public WebApiResponse ClientCredentialsGrantRequest(string clientId, string clientSecret, string scope = null)
//        {
//            return
//                new HttpHelper()
//                .WithPostMethod()
//                .ApplicationFormUrl()
//                .WithData("grant_type", "client_credentials")
//                /*.AuthorizationBasic(clientId, clientSecret)*/
//                .WithData("client_id", clientId)
//                .WithData("client_secret", clientSecret)
//                .WithDataIf("scope", scope)
//                ;
//        }

//        /// <summary>
//        /// Get Refresh token grant request
//        /// </summary>
//        /// <param name="refreshCode"></param>
//        /// <param name="clientId"></param>
//        /// <param name="clientSecret"></param>
//        /// <param name="scope"></param>
//        /// <returns></returns>
//        public WebApiResponse RefreshTokenGrantRequest(string refreshCode, string clientId, string clientSecret, string scope = null)
//        {
//            return
//                new HttpHelper()
//                .WithPostMethod()
//                .ApplicationFormUrl()
//                .WithData("grant_type", "refresh_token")
//                /*.AuthorizationBasic(clientId, clientSecret)*/
//                .WithData("client_id", clientId)
//                .WithData("client_secret", clientSecret)
//                .WithDataIf("scope", scope)
//                ;
//        }

//    }
//}
