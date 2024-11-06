using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using EmergenceSDK.Runtime.Internal.Services;
using UnityEngine;

namespace EmergenceSDK.Runtime.Services
{
    /// <summary>
    /// Service for signing messages with the custodial service.
    /// </summary>
    public class CustodialSigningService : ICustodialSigningService
    {
        private const string BaseUrl = "https://signer.futureverse.app/";
        private const string CallbackPath = "signature-callback";

        /// <summary>
        /// Initiates the signing process for a message using the custodial service.
        /// </summary>
        /// <param name="custodialEOA">The Ethereum address for the custodial account.</param>
        /// <param name="message">The message to be signed.</param>
        /// <param name="ct">Cancellation token for managing async flow cancellation.</param>
        /// <returns>A UniTask containing the signed message.</returns>
        public async UniTask<ServiceResponse<string>> SignMessageAsync(string custodialEOA, string message, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(custodialEOA) || string.IsNullOrEmpty(message))
            {
                return new ServiceResponse<string>(false, "Invalid parameters provided.");
            }

            LocalWebServerHelper.StartServer(async (authCode, state, expectedState) =>
            {
                if (state != LocalWebServerHelper.ExpectedState)
                {
                    Debug.LogError("State mismatch. Possible CSRF attack detected.");
                    return;
                }

                var signedMessage = await OAuthHelper.SignMessageAsync(BaseUrl, custodialEOA, message, authCode, ct);
                if (signedMessage != null)
                {
                    Debug.Log($"Successfully signed message: {signedMessage}");
                }
                else
                {
                    Debug.LogError("Failed to retrieve signed message.");
                }

                LocalWebServerHelper.StopServer();
            });

            string nonce = GenerateSecureRandomString(128);
            string payload = CreateSigningPayload(custodialEOA, message);
            string signUrl = $"{BaseUrl}?request={payload}&nonce={nonce}";

            Application.OpenURL(signUrl);
            return new ServiceResponse<string>(true, "Signing process initiated.");
        }

        /// <summary>
        /// Creates a JSON payload for signing the message.
        /// </summary>
        /// <param name="custodialEOA">The Ethereum address for the custodial account.</param>
        /// <param name="message">The message to be signed.</param>
        /// <returns>The JSON payload as a string.</returns>
        private string CreateSigningPayload(string custodialEOA, string message)
        {
            string hexMessage = ConvertToHex(message);
            return $"{{ \"account\": \"{custodialEOA}\", \"message\": \"{hexMessage}\" }}";
        }

        /// <summary>
        /// Converts a message string to its hexadecimal representation.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>The hexadecimal representation of the message.</returns>
        private string ConvertToHex(string message)
        {
            StringBuilder hex = new StringBuilder(message.Length * 2);
            foreach (char c in message)
            {
                hex.AppendFormat("{0:X2}", (int)c);
            }
            return hex.ToString();
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
    }
}
