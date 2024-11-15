using System;
using System.Text;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Futureverse;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.ScriptableObjects;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Responses;
using Newtonsoft.Json;
using UnityEngine;

namespace EmergenceSDK.Runtime.Internal.Services
{
    /// <summary>
    /// Service for signing messages with the custodial service.
    /// </summary>
    public class CustodialSigningService : ICustodialSigningService
    {
        private const string CallbackPath = "signature-callback";
        
        private string ProductionBaseUrl = "https://signer.pass.online/";
        private string StagingBaseUrl = "https://signer.passonline.cloud/";
        
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
        public CustodialSigningService()
        {
            var config = Resources.Load<CustodialLoginConfiguration>("CustodialServicesConfiguration");

            if (config != null)
            {
                // Override hardcoded values with values from the ScriptableObject
                ProductionBaseUrl = config.ProductionSigningBaseUrl;
                StagingBaseUrl = config.StagingSigningBaseUrl;
            }
            else
            {
                EmergenceLogger.LogWarning("CustodialServicesConfiguration ScriptableObject not found. Using default values.");
            }
        }

        /// <summary>
        /// Unitask for handling the creation of an Emergence Access Token using a Custodial login.
        /// </summary>
        /// <param name="custodialEOA">The Custodial EOA or wallet to sign the message.</param>
        /// <param name="messageToSign">The message to be signed.</param>
        /// <param name="expirationTimestamp">The duration of the EAT</param>
        /// <returns></returns>
        public async UniTask<string> GenerateEAT(string custodialEOA, string messageToSign, string expirationTimestamp)
        {
            string signature = await RequestToSignAsync(custodialEOA, messageToSign);
            string signedMessage = ConvertCustodialSignedMessageToEmergenceAt(signature, custodialEOA, expirationTimestamp);
            return signedMessage;
        }
        

        /// <summary>
        /// UniTask for handling requests to proccess messages and send them to an external signing service.
        /// </summary>
        /// <param name="custodialEOA">The Custodial EOA or wallet to sign the message.</param>
        /// <param name="messageToSign">The message to be signed.</param>
        /// <returns></returns>
        public async UniTask<string> RequestToSignAsync(string custodialEOA, string messageToSign)
        {
            var tcs = new UniTaskCompletionSource<bool>();// Ensures the thread still awaits the callback from the external service
            string signature = "";
            
            // We start a local web listener and assign a method that will take a response and verify and convert it
            CustodialLocalWebServerHelper.StartSigningServer(CallbackPath, (responseJson) =>
            {
                if (!string.IsNullOrEmpty(responseJson))
                {
                    CustodialSignerServiceResponse signingServerResponse = JsonConvert.DeserializeObject<CustodialSignerServiceResponse>(responseJson);
                    signature = signingServerResponse.Result.Data.Signature;
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
            
            return signature;
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
