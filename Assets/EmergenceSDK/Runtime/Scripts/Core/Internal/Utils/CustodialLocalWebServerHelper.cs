using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace EmergenceSDK.Runtime.Internal.Utils
{
    /// <summary>
    /// A helper class for managing http listeners related to Custodial authentication.
    /// </summary>
    public static class CustodialLocalWebServerHelper
    {
        private static HttpListener httpListener;
        private static bool isTokenAuthListenerStarted = false;
        private static bool isSigningServerStarted = false;
        private const int ServerPort = 3000;
        private const string CallbackPath = "/callback";
        
        private const string Base64Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        /// <summary>
        /// The expected state value for validating CSRF protection.
        /// </summary>
        public static string ExpectedState { get; set; }

        /// <summary>
        /// Starts a local web server to listen for the OAuth callback.
        /// </summary>
        /// <param name="onAuthCodeReceived">Callback function to handle the authorization code and state.</param>
        public static void StartTokenAuthListener(Action<string, string, string> onAuthCodeReceived)
        {
            if (isTokenAuthListenerStarted)
            {
                Debug.Log("Local web server is already started.");
                return;
            }

            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://localhost:{ServerPort}{CallbackPath}/");
            httpListener.Start();
            isTokenAuthListenerStarted = true;

            // Start listening for requests asynchronously
            Task.Run(() => ListenForTokenAuthRequests(onAuthCodeReceived));
        }

        /// <summary>
        /// Stops the local web server.
        /// </summary>
        public static void StopTokenAuthListener()
        {
            if (isTokenAuthListenerStarted)
            {
                httpListener.Stop();
                httpListener.Close();
                isTokenAuthListenerStarted = false;
                Debug.Log("Local web server stopped.");
            }
        }

        /// <summary>
        /// Listens for incoming requests and handles the OAuth callback.
        /// </summary>
        /// <param name="onAuthCodeReceived">Callback function to handle the authorization code and state.</param>
        private static async UniTask ListenForTokenAuthRequests(Action<string, string, string> onAuthCodeReceived)
        {
            while (isTokenAuthListenerStarted)
            {
                try
                {
                    var context = await httpListener.GetContextAsync();
                    var request = context.Request;

                    if (request.HttpMethod == "GET" && request.Url.AbsolutePath == CallbackPath)
                    {
                        var queryParams = request.QueryString;
                        string authCode = queryParams["code"];
                        string state = queryParams["state"];

                        if (string.IsNullOrEmpty(authCode))
                        {
                            Debug.LogError("Authorization code not found in the request.");
                            return;
                        }

                        StopTokenAuthListener();
                        onAuthCodeReceived?.Invoke(authCode, state, ExpectedState);
                    }
                }
                catch (HttpListenerException ex)
                {
                    Debug.LogError("HttpListener exception: " + ex.Message);
                }
            }
        }
    
        /// <summary>
        /// Starts a local web server to listen for the Signing Callbacks.
        /// </summary>
        /// <param name="onSigningResponseReceived">Callback function to handle the signing response.</param>
        public static void StartSigningServer(string callbackPath, Action<string> onSigningResponseReceived)
        {
            if (isSigningServerStarted)
            {
                EmergenceLogger.LogWarning("Local web server is already started.");
                return;
            }

            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://localhost:{ServerPort}/{callbackPath}/");
            httpListener.Start();
            isSigningServerStarted = true;


            // Start listening for requests asynchronously
            Task.Run(() => ListenForSigningRequests(callbackPath, onSigningResponseReceived));
        }

        /// <summary>
        /// Listens for incoming Signature response callback.
        /// </summary>
        /// <param name="callbackPath"></param>
        /// <param name="onSigningResponseReceived"></param>
        private static async Task ListenForSigningRequests(string callbackPath, Action<string> onSigningResponseReceived)
        {
            while (isSigningServerStarted)
            {
                try
                {
                    var context = await httpListener.GetContextAsync();
                    var request = context.Request;

                    if (request.HttpMethod == "GET" && request.Url.AbsolutePath == $"/{callbackPath}")
                    {
                        string responseJson = request.QueryString["response"];
                    
                        if (!string.IsNullOrEmpty(responseJson))
                        {
                            onSigningResponseReceived?.Invoke(Encoding.UTF8.GetString(ConvertFromBase64String(responseJson)));
                            StopSigningServer();
                        }
                        else
                        {
                            EmergenceLogger.LogError("No response message received.");
                        }
                    }
                }
                catch (HttpListenerException ex)
                {
                    EmergenceLogger.LogError("HttpListener exception: " + ex.Message);
                }
            }
        }
    
        
        /// <summary>
        /// Terminates signing server activity and closes the local HTTP Listener.
        /// </summary>
        public static void StopSigningServer()
        {
            if (isSigningServerStarted)
            {
                httpListener.Stop();
                httpListener.Close();
                isSigningServerStarted = false;
            }
        }
        
        /// <summary>
        /// Custom function to handle bit shifting for Base 64 Conversion, Necessary as the .Net function caused failures.
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
    }
}
