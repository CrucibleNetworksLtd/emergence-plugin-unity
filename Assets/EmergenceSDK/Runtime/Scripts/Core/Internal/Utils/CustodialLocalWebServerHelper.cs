using System;
using System.Net;
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

            Debug.Log($"Local web server started on http://localhost:{ServerPort}{CallbackPath}");

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
                Debug.Log("Local web server is already started.");
                return;
            }

            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://localhost:{ServerPort}/{callbackPath}/");
            httpListener.Start();
            isSigningServerStarted = true;

            Debug.Log($"Local web server started on http://localhost:{ServerPort}/{callbackPath}");

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
                            onSigningResponseReceived?.Invoke(responseJson);
                            StopSigningServer();
                        }
                        else
                        {
                            Debug.LogError("No response message received.");
                        }
                    }
                }
                catch (HttpListenerException ex)
                {
                    Debug.LogError("HttpListener exception: " + ex.Message);
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
                Debug.Log("Local web server stopped.");
            }
        }
    }
}
