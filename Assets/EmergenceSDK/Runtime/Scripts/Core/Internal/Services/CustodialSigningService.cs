using System;
using System.Text;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Internal.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace EmergenceSDK.Runtime.Internal.Services
{
    /// <summary>
    /// Service for signing messages with the custodial service.
    /// </summary>
    public class CustodialSigningService : ICustodialSigningService
    {
        private const string BaseUrl = "https://signer.futureverse.cloud/";
        private const string CallbackPath = "signature-callback";
        
        private const string Base64Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        /// <summary>
        /// UniTask for handling requests to proccess messages and send them to an external signing service.
        /// </summary>
        /// <param name="custodialEOA">The Custodial EOA or wallet to sign the message.</param>
        /// <param name="messageToSign">The message to be signed.</param>
        /// <param name="timestamp">The timestamp of the message being signed.</param>
        /// <returns></returns>
        public async UniTask<string> RequestToSignAsync(string custodialEOA, string messageToSign, string timestamp)
        {
            var tcs = new UniTaskCompletionSource<bool>();// Ensures the thread still awaits the callback from the external service
            string signedMessage = "";
            
            // We start a local web listener and assign a method that will take a response and verify and convert it
            CustodialLocalWebServerHelper.StartSigningServer(CallbackPath, (responseJson) =>
            {
                if (!string.IsNullOrEmpty(responseJson))
                {
                    var data = ConvertFromBase64String(responseJson);
                    string response = Encoding.UTF8.GetString(data);
                    JObject jsonResponse = JObject.Parse(response);// As this data isn't reused we just use a JObject rather than a dedicated struct
                    string signature = jsonResponse["result"]?["data"]?["signature"]?.ToString();
                    signedMessage = ConvertCustodialSignedMessageToEmergenceAt(signature, custodialEOA, timestamp);
                    tcs.TrySetResult(true); // The callback will mark itself as completed allowing the task to proceed and return a response.
                }
                else
                {
                    tcs.TrySetResult(false);
                    Debug.LogError("No response message received.");
                }
            });

            // Create our payload for the signing service.
            string hexMessage = ConvertToHex(messageToSign);
            var signTransactionPayload = new
            {
                account = custodialEOA,
                message = hexMessage,
                callbackUrl = "http://localhost:3000/signature-callback"
            };
            var encodedPayload = new
            {
                id = "client:2", // Must be formatted as `client:${ identifier number }`
                tag = "fv/sign-msg", // Do not change this
                payload = signTransactionPayload
            };

            string jsonPayload = JsonConvert.SerializeObject(encodedPayload);
            string base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonPayload)).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            string url = $"{BaseUrl}?request={base64Payload}";
            Application.OpenURL(url); // Contact external signing service with our encoded payload.
            
            await tcs.Task; // Await callback handler to be triggered by external service.
            
            return signedMessage;
        }

        /// <summary>
        /// Converts a string to a hex string.
        /// </summary>
        /// <param name="message">Message to be converted</param>
        /// <returns></returns>
        private string ConvertToHex(string message)
        {
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(message);
        
            StringBuilder hexBuilder = new StringBuilder("0x");

            foreach (byte b in utf8Bytes)
            {
                hexBuilder.AppendFormat("{0:X2}", b);
            }

            return hexBuilder.ToString().ToLower();
        }

        /// <summary>
        /// Custom function for Base 64 Conversion, because the .Net native one just fails without any output and kills the whole UniTask.
        /// </summary>
        /// <param name="base64">The Base64 string to be converted.</param>
        /// <returns></returns>
        public static byte[] ConvertFromBase64String(string base64)
        {
            base64 = base64.TrimEnd('=');
            int padding = base64.Length % 4;
            if (padding > 0)
            {
                base64 += new string('=', 4 - padding);
            }

            byte[] bytes = new byte[base64.Length * 3 / 4];
            int byteIndex = 0;

            for (int i = 0; i < base64.Length; i += 4)
            {
                int b1 = Base64Characters.IndexOf(base64[i]);
                int b2 = Base64Characters.IndexOf(base64[i + 1]);
                int b3 = Base64Characters.IndexOf(base64[i + 2]);
                int b4 = Base64Characters.IndexOf(base64[i + 3]);

                bytes[byteIndex++] = (byte)((b1 << 2) | (b2 >> 4));
                if (b3 != -1)
                    bytes[byteIndex++] = (byte)(((b2 & 0x0F) << 4) | (b3 >> 2));
                if (b4 != -1)
                    bytes[byteIndex++] = (byte)(((b3 & 0x03) << 6) | b4);
            }

            return bytes;
        }

        /// <summary>
        /// Converts a Custodial response to an Emergence Access token
        /// </summary>
        /// <param name="signedMessage"></param>
        /// <param name="eoa"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        private string ConvertCustodialSignedMessageToEmergenceAt(string signedMessage, string eoa, string timestamp)
        {
            string eAccessToken = $"{{\"signedMessage\" : \"{signedMessage}\"," +
                             $"\"message\" : \"{{\\\"expires-on\\\": {timestamp}}}\"," +
                             $"\"address\" : \"{eoa}\"}}";
            return eAccessToken;
        }
    }
}
