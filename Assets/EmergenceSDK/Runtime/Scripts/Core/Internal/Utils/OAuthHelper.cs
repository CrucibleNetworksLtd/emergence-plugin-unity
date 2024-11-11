using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Futureverse.Internal;
using EmergenceSDK.Runtime.Types;
using Newtonsoft.Json;
using UnityEngine;

namespace EmergenceSDK.Runtime.Internal.Services
{
    /// <summary>
    /// Provides methods for handling OAuth authorization flow, including 
    /// requesting authorization codes and exchanging them for access tokens.
    /// </summary>
    internal static class OAuthHelper
    {
        /// <summary>
        /// Initiates the OAuth authorization flow by opening the authorization URL.
        /// </summary>
        /// <param name="authMessage">The full web request text.</param>
        /// <param name="ct">Cancellation token to manage async flow cancellation.</param>
        /// <returns>The HTTP status code indicating the result of the request.</returns>
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
        /// <param name="baseUrl">The base URL for the OAuth provider.</param>
        /// <param name="clientID">The client ID for the OAuth application.</param>
        /// <param name="codeVerifier">The code verifier used in the OAuth request.</param>
        /// <param name="authCode">The authorization code received from the OAuth provider.</param>
        /// <param name="redirectUri">The redirect URI for the OAuth application.</param>
        /// <param name="ct">Cancellation token to manage async flow cancellation.</param>
        /// <returns>The access token if successful, otherwise null.</returns>
        public static async UniTask<CustodialAccessTokenResponse> ParseAndExchangeCodeForCustodialResponseAsync(string baseUrl, string clientID, string codeVerifier, string authCode, string redirectUri, CancellationToken ct)
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

            var tokenResponse = JsonConvert.DeserializeObject<CustodialAccessTokenResponse>(response.ResponseText);
            return tokenResponse;
        }
        
        /// <summary>
        /// Signs a message using the custodial service by sending a request with the message and account information.
        /// </summary>
        /// <param name="baseUrl">The base URL for the signing service.</param>
        /// <param name="fvcCustodialEOA">The Ethereum address for the custodial account.</param>
        /// <param name="message">The message to be signed.</param>
        /// <param name="nonce">A unique nonce for the signing request.</param>
        /// <param name="ct">Cancellation token for managing async flow cancellation.</param>
        /// <returns>The signed message if successful, otherwise null.</returns>
        public static async UniTask<string> SignMessageAsync(string baseUrl, string fvcCustodialEOA, string message, string nonce, CancellationToken ct)
        {
            var headers = new Dictionary<string, string> { { "Content-Type", "application/json" } };
            
            // Create the payload for signing
            var payload = new
            {
                account = fvcCustodialEOA,
                message = ConvertToHex(message),
                nonce = nonce
            };

            string jsonPayload = JsonUtility.ToJson(payload);
            string url = $"{baseUrl}?request={Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonPayload))}";

            // Send the request to the signing service
            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Post, url, jsonPayload, headers);

            if (!response.Successful)
            {
                Debug.LogError("Failed to sign message.");
                return null;
            }
            return response.ResponseText;
        }

        /// <summary>
        /// Converts a message string to its hexadecimal representation.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>The hexadecimal representation of the message.</returns>
        private static string ConvertToHex(string message)
        {
            StringBuilder hex = new StringBuilder(message.Length * 2);
            foreach (char c in message)
            {
                hex.AppendFormat("{0:X2}", (int)c);
            }
            return hex.ToString();
        }
    }
}
