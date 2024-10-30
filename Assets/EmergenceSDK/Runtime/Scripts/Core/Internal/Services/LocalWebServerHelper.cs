using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using UnityEngine;

public static class LocalWebServerHelper
{
    private static HttpListener _httpListener;
    private static bool _isServerStarted = false;
    private const int ServerPort = 3000;
    private const string CallbackPath = "/callback";
    public static string ExpectedState { get; set; }

    /// <summary>
    /// Starts the local web server to listen for the OAuth callback.
    /// </summary>
    public static void StartServer(Action<string, string, string> onAuthCodeReceived)
    {
        if (_isServerStarted)
        {
            Debug.Log("Local web server is already started.");
            return;
        }

        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add($"http://localhost:{ServerPort}{CallbackPath}/");
        _httpListener.Start();
        _isServerStarted = true;

        Debug.Log($"Local web server started on http://localhost:{ServerPort}{CallbackPath}");

        // Start listening for requests asynchronously
        Task.Run(() => ListenForRequests(onAuthCodeReceived));
    }

    /// <summary>
    /// Stop the local web server.
    /// </summary>
    public static void StopServer()
    {
        if (_isServerStarted)
        {
            _httpListener.Stop();
            _httpListener.Close();
            _isServerStarted = false;
            Debug.Log("Local web server stopped.");
        }
    }

    /// <summary>
    /// Listens for incoming requests and handles the OAuth callback.
    /// </summary>
    private static async Task ListenForRequests(Action<string, string, string> onAuthCodeReceived)
    {
        while (_isServerStarted)
        {
            try
            {
                var context = await _httpListener.GetContextAsync();
                var request = context.Request;

                if (request.HttpMethod == "GET" && request.Url.AbsolutePath == CallbackPath)
                {
                    var queryParams = request.QueryString;
                    string authCode = queryParams["code"];
                    string state = queryParams["state"];

                    // Validate the auth code
                    if (string.IsNullOrEmpty(authCode))
                    {
                        Debug.LogError("Authorization code not found in the request.");
                        return;
                    }
                    // Stop the server after processing the request
                    StopServer();
                    // Pass the auth code and state to the callback function
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
