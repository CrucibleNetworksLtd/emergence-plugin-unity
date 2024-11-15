using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Futureverse;
using EmergenceSDK.Runtime.Futureverse.Internal;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.ScriptableObjects;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using UnityEngine;

namespace EmergenceSDK.Runtime.Internal.Services
{
    /// <summary>
    /// Handles the custodial login process, including generating the necessary parameters 
    /// for OAuth and exchanging authorization codes for access tokens.
    /// </summary>
    internal class CustodialLoginService : ICustodialLoginService
    {
        public CustodialAccessTokenResponse CachedAccessTokenResponse { get; set; }

        private string currentState;
        private string currentCodeVerifier;

        // Need to add support for Developer Custom URLs, lets use a serialised object.
        private string DevelopmentClientID = "3KMMFCuY59SA4DDV8ggwc";
        private string StagingClientID = "3KMMFCuY59SA4DDV8ggwc";
        private string ProductionClientID = "G9mOSDHNklm_dCN0DHvfX";
        private string ProductionBaseUrl = "https://login.pass.online/";
        private string StagingBaseUrl = "https://login.passonline.cloud/";
        
        private const string RedirectUri = "http://localhost:3000/callback";

        /// <summary>
        /// Gets the client ID based on the current environment.
        /// </summary>
        public string ClientID => FutureverseSingleton.Instance.Environment switch
        {
            EmergenceEnvironment.Development => DevelopmentClientID,
            EmergenceEnvironment.Staging => StagingClientID,
            EmergenceEnvironment.Production => ProductionClientID,
            _ => throw new ArgumentOutOfRangeException(nameof(EmergenceSingleton.Instance.Environment), "Unknown environment")
        };
        
        /// <summary>
        /// Gets the Login Base URL based on the current environment.
        /// </summary>
        public string BaseUrl => FutureverseSingleton.Instance.Environment switch
        {
            EmergenceEnvironment.Development => StagingBaseUrl,
            EmergenceEnvironment.Staging => StagingBaseUrl,
            EmergenceEnvironment.Production => ProductionBaseUrl,
            _ => throw new ArgumentOutOfRangeException(nameof(EmergenceSingleton.Instance.Environment), "Unknown environment")
        };

        /// <summary>
        /// The constructor for this service will attempt to parse configuration details from a scriptable object in the Resources folder.
        /// There is a default configuration object in EmergenceSDK/Runtime/Resources.
        /// </summary>
        public CustodialLoginService()
        {
            // Attempt to load the ScriptableObject
            var config = Resources.Load<CustodialLoginConfiguration>("CustodialServicesConfiguration");

            if (config != null)
            {
                // Override hardcoded values with values from the ScriptableObject
                DevelopmentClientID = config.DevelopmentClientID;
                StagingClientID = config.StagingClientID;
                ProductionClientID = config.ProductionClientID;
                ProductionBaseUrl = config.ProductionLoginBaseUrl;
                StagingBaseUrl = config.StagingLoginBaseUrl;
            }
            else
            {
                EmergenceLogger.LogWarning("CustodialServicesConfiguration ScriptableObject not found. Using default values.");
            }
        }

        /// <summary>
        /// Starts the custodial login process, generating the necessary parameters and starting 
        /// the local web server to handle OAuth callbacks.
        /// </summary>
        /// <param name="onSuccessfulLogin">Callback invoked when login is successful.</param>
        /// <param name="ct">Cancellation token to manage async flow cancellation.</param>
        /// <returns>A service response indicating the result of the login attempt.</returns>
        public async UniTask<ServiceResponse<string>> StartCustodialLoginAsync(Func<CustodialAccessTokenResponse,CancellationToken, UniTask> onSuccessfulLogin,CancellationToken ct)
        {
            currentState = GenerateSecureRandomString(128);
            currentCodeVerifier = GenerateSecureRandomString(64);
            string codeChallenge = GenerateCodeChallenge(currentCodeVerifier);

            CustodialLocalWebServerHelper.ExpectedState = currentState;
            
            // Used to hold thread whilst awaiting callback from HTTP listener.
            var tcs = new UniTaskCompletionSource<bool>();
            
            var walletServiceInternal = EmergenceServiceProvider.GetService<IWalletServiceInternal>();

            CustodialLocalWebServerHelper.StartTokenAuthListener(async (authCode,state,expectedState) =>
            {
                CachedAccessTokenResponse = await OAuthHelper.ParseAndExchangeCodeForCustodialResponseAsync(BaseUrl, ClientID, currentCodeVerifier, authCode, RedirectUri, ct);
                walletServiceInternal.AssignCustodialWalletAddress(CachedAccessTokenResponse.DecodedToken.Eoa);
                if (CachedAccessTokenResponse != null)
                {
                    // Call the callback method on the Login Manager
                    await onSuccessfulLogin(CachedAccessTokenResponse, ct);
                    tcs.TrySetResult(true);
                }
                else
                {
                    Debug.LogError("Failed to retrieve access token.");
                    tcs.TrySetResult(false);
                }
            });

            string nonce = GenerateSecureRandomString(128);

            string authUrl = $"{BaseUrl}auth?" +
                             "response_type=code" +
                             $"&client_id={ClientID}" +
                             $"&redirect_uri={HttpUtility.UrlEncode(RedirectUri)}" +
                             "&scope=openid" +
                             $"&code_challenge={codeChallenge}" +
                             "&code_challenge_method=S256" +
                             "&response_mode=query" +
                             "&prompt=login" +
                             $"&state={currentState}" +
                             $"&nonce={nonce}";

            await OAuthHelper.RequestAuthorizationCodeAsync(authUrl, ct);
            await tcs.Task; // Await external callback to trigger task completion.
            return new ServiceResponse<string>(true, authUrl); 
        }

        /// <summary>
        /// Generates a secure random string of the specified length.
        /// </summary>
        /// <param name="length">The length of the random string.</param>
        /// <returns>A secure random string.</returns>
        private string GenerateSecureRandomString(int length)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var data = new byte[length];
                rng.GetBytes(data);
                return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            }
        }

        /// <summary>
        /// Generates a code challenge based on the provided code verifier.
        /// </summary>
        /// <param name="codeVerifier">The code verifier used to create the challenge.</param>
        /// <returns>The generated code challenge.</returns>
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
