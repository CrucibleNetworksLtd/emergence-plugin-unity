using System;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// A helper class for managing a local web server to handle OAuth callbacks.
/// </summary>
public static class LocalWebServerHelper
{
    private static HttpListener httpListener;
    private static bool isServerStarted = false;
    private const int ServerPort = 3000;
    private const string CallbackPath = "/callback";

    /// <summary>
    /// The expected state value for validating CSRF protection.
    /// </summary>
    public static string ExpectedState { get; set; }

    /// <summary>
    /// Starts the local web server to listen for the OAuth callback.
    /// </summary>
    /// <param name="onAuthCodeReceived">Callback function to handle the authorization code and state.</param>
    public static void StartServer(Action<string, string, string> onAuthCodeReceived)
    {
        if (isServerStarted)
        {
            Debug.Log("Local web server is already started.");
            return;
        }

        httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://localhost:{ServerPort}{CallbackPath}/");
        httpListener.Start();
        isServerStarted = true;

        Debug.Log($"Local web server started on http://localhost:{ServerPort}{CallbackPath}");

        // Start listening for requests asynchronously
        Task.Run(() => ListenForRequests(onAuthCodeReceived));
    }

    /// <summary>
    /// Stops the local web server.
    /// </summary>
    public static void StopServer()
    {
        if (isServerStarted)
        {
            httpListener.Stop();
            httpListener.Close();
            isServerStarted = false;
            Debug.Log("Local web server stopped.");
        }
    }

    /// <summary>
    /// Listens for incoming requests and handles the OAuth callback.
    /// </summary>
    /// <param name="onAuthCodeReceived">Callback function to handle the authorization code and state.</param>
    private static async Task ListenForRequests(Action<string, string, string> onAuthCodeReceived)
    {
        while (isServerStarted)
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

                    StopServer();
                    onAuthCodeReceived?.Invoke(authCode, state, ExpectedState);
                }
            }
            catch (HttpListenerException ex)
            {
                Debug.LogError("HttpListener exception: " + ex.Message);
            }
        }
    }
}
