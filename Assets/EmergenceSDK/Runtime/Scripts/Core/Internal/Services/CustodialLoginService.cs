using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Types.Exceptions.Login;
using EmergenceSDK.Runtime.Types.Responses;
using UnityEngine;

namespace EmergenceSDK.Runtime.Internal.Services
{
    internal class CustodialLoginService : ICustodialLoginService
    {
        private string cachedAccessToken;
        private string currentState;
        private string currentCodeVerifier;

        private const string DevelopmentClientID = "3KMMFCuY59SA4DDV8ggwc";
        private const string StagingClientID = "some-staging-client-id";
        private const string ProductionClientID = "G9mOSDHNklm_dCN0DHvfX";
        private const string RedirectUri = "http://localhost:3000/callback";
        private const string BaseUrl = "https://login.futureverse.cloud/";

        public CustodialLoginService()
        {
        }

        public string ClientID => EmergenceSingleton.Instance.Environment switch
        {
            EmergenceEnvironment.Development => DevelopmentClientID,
            EmergenceEnvironment.Staging => StagingClientID,
            EmergenceEnvironment.Production => ProductionClientID,
            _ => throw new ArgumentOutOfRangeException(nameof(EmergenceSingleton.Instance.Environment), "Unknown environment")
        };

        public async UniTask<ServiceResponse<string>> StartCustodialLoginAsync(Action<string,CancellationToken> onSuccessfulLogin,CancellationToken ct)
        {
            // Generate state and code verifier for PKCE
            currentState = GenerateSecureRandomString(128);
            currentCodeVerifier = GenerateSecureRandomString(64);
            string codeChallenge = GenerateCodeChallenge(currentCodeVerifier);

            // Set expected state in LocalWebServerHelper for validation
            LocalWebServerHelper.ExpectedState = currentState;

            // Start local server listener before initiating the OAuth request
            LocalWebServerHelper.StartServer(async (authCode,state,expectedState) =>
            {
                // Exchange the authorization code for an access token when received
                cachedAccessToken = await OAuthHelper.ParseAndExchangeCodeForTokenAsync(BaseUrl, ClientID, currentCodeVerifier, authCode, RedirectUri, ct);
                if (cachedAccessToken != null)
                {
                    onSuccessfulLogin(cachedAccessToken, ct);
                }
                else
                {
                    Debug.LogError("Failed to retrieve access token.");
                }
            });
            
            string nonce = GenerateSecureRandomString(128);

            // Build the OAuth URL
            string authUrl = $"{BaseUrl}auth?" +
                             "response_type=code" +
                             $"&client_id={ClientID}" +
                             $"&redirect_uri={HttpUtility.UrlEncode(RedirectUri)}" +
                             "&scope=openid" +
                             $"&code_challenge={codeChallenge}" +
                             "&code_challenge_method=S256"+
                             "&response_mode=query" +
                             "&prompt=login"+
                             $"&state={currentState}" +
                             $"&nonce={nonce}";

            // Fire the request (browser or embedded, depending on environment)
            await OAuthHelper.RequestAuthorizationCodeAsync(authUrl,ct);

            return new ServiceResponse<string>(true, authUrl); // Returns the auth URL for testing or display purposes
        }

        private string GenerateSecureRandomString(int length)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var data = new byte[length];
                rng.GetBytes(data);
                return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            }
        }

        private string GenerateCodeChallenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            }
        }
    }
}
