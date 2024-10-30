using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Responses;
using UnityEngine;

namespace EmergenceSDK.Runtime.Internal.Services
{
    internal static class OAuthHelper
    {
        /// <summary>
        /// Initiates the OAuth authorization flow and retrieves an authorization code.
        /// </summary>
        /// <param name="authMessage">The full webREquest Text</param>
        /// <returns>The authorization code if successful, otherwise null.</returns>
        public static async UniTask<HttpStatusCode> RequestAuthorizationCodeAsync(string authMessage, CancellationToken ct)
        {
            try
            {
                Application.OpenURL(authMessage);
                return HttpStatusCode.Accepted;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error while requesting authorization code: " + ex.Message);
                return HttpStatusCode.BadRequest;
            }
        }

        /// <summary>
        /// Exchanges the authorization code for an access token.
        /// </summary>
        public static async UniTask<string> ParseAndExchangeCodeForTokenAsync(string baseUrl, string clientID, string codeVerifier, string authCode, string redirectUri, CancellationToken ct)
        {
            var headers = new Dictionary<string, string> { { "Content-Type", "application/x-www-form-urlencoded" } };
            
            var body = $"grant_type=authorization_code" +
                           $"&code={authCode}" +
                           $"&redirect_uri={HttpUtility.UrlEncode(redirectUri)}" +
                           $"&client_id={clientID}" +
                           $"&code_verifier={codeVerifier}";

            string url = $"{baseUrl}token";
            await UniTask.SwitchToMainThread();
            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Post, url, body, headers);

            if (!response.Successful)
            {
                Debug.LogError("Failed to exchange authorization code for token.");
                return null;
            }

            var tokenResponse = SerializationHelper.Deserialize<AccessTokenResponse>(response.ResponseText);
            return tokenResponse.AccessToken.Token;
        }
    }
}
