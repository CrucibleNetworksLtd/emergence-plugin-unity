using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Types;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;
using UnityEngine.Networking;
using WebResponse = EmergenceSDK.Internal.Types.WebResponse;

namespace EmergenceSDK.Internal.Services
{
    /// <summary>
    /// This class handles all web requests and provides a debugging layer by monitoring all requests and storing their creation parameters.
    /// It should be used for all internally created web requests, as it allows us to easily understand and monitor the web flow within the SDK.
    /// <remarks>
    /// Even though this class has the same naming pattern and namespace of all Services, it is not managed by the ServiceProvider.
    /// </remarks>
    /// </summary>
    internal class WebRequestService
    {
        /// <summary>
        /// Class containing the information of the UnityWebRequest that was created
        /// </summary>
        internal class WebRequestInfo
        {
            /// <summary>
            /// Stores the next key for the next web request, incremental.
            /// </summary>
            private static ulong nextId;
            
            /// <summary>
            /// Time when the request was created.
            /// </summary>
            internal readonly DateTime Time;
            
            /// <summary>
            /// Unique key for this web request
            /// </summary>
            internal readonly ulong Id;
            
            /// <summary>
            /// The Request headers, only populated if the headers are passed to.
            /// <see cref="WebRequestService.PerformAsyncWebRequest"/>
            /// to send the request
            /// </summary>
            internal readonly Dictionary<string, string> Headers;
            
            /// <summary>
            /// Reference to the WebResponse that the UnityWebRequest will produce.
            /// </summary>
            internal WebResponse Response { get; set; }
            
            /// <summary>
            /// Whether the request had an UploadHandler upon creation.
            /// </summary>
            internal readonly bool HadUploadHandler;
            
            /// <summary>
            /// Whether the request had a DownloadHandler upon creation.
            /// </summary>
            internal readonly bool HadDownloadHandler;
            
            internal WebRequestInfo(Dictionary<string, string> requestHeaders, UnityWebRequest request)
            {
                Id = nextId++;
                Time = DateTime.Now;
                Headers = requestHeaders ?? new Dictionary<string, string>();
                HadUploadHandler = request.uploadHandler != null;
                HadDownloadHandler = request.downloadHandler != null;
                var contentTypeFound = false;
                foreach (var key in Headers.Keys)
                {
                    if (key.ToLower() == "content-type")
                    {
                        contentTypeFound = true;
                    }
                }

                if (!contentTypeFound && request.uploadHandler != null)
                {
                    Headers.Add("Content-Type", request.uploadHandler.contentType);
                }
            }
        }
        
        private static readonly Lazy<WebRequestService> LazyInstance = new(() => new WebRequestService());
        internal static WebRequestService Instance => LazyInstance.Value;
        
        /// <summary>
        /// ConcurrentDictionary containing all open requests
        /// </summary>
        private readonly ConcurrentDictionary<UnityWebRequest, WebRequestInfo> openRequests = new();
        
        /// <summary>
        /// ConcurrentDictionary containing all requests that have not been disposed yet
        /// </summary>
        private readonly ConcurrentDictionary<UnityWebRequest, WebRequestInfo> allRequests = new();

        //This timeout avoids this issue: https://forum.unity.com/threads/catching-curl-error-28.1274846/
        internal const int DefaultTimeoutMilliseconds = 100000;

        private WebRequestService()
        {
            EmergenceSingleton.Instance.OnGameClosing += CancelAllRequests;
        }

        private void CancelAllRequests()
        {
            foreach (var openRequest in openRequests)
            {
                openRequest.Key.Abort();
            }
        }

        private static UnityWebRequest CreateRequest(RequestMethod method, string url, string bodyData = "")
        {
            UnityWebRequest request;
            switch (method)
            {
                case RequestMethod.Get:
                    request = UnityWebRequest.Get(url);
                    break;
                case RequestMethod.Head:
                    request = UnityWebRequest.Head(url);
                    break;
                case RequestMethod.Post:
                    request = RequestWithJsonBody(url, "POST", bodyData);
                    break;
                case RequestMethod.Put:
                    request = RequestWithJsonBody(url, "PUT", bodyData);
                    break;
                case RequestMethod.Patch:
                    request = RequestWithJsonBody(url, "PATCH", bodyData);
                    break;
                case RequestMethod.Delete:
                    request = UnityWebRequest.Delete(url);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), "Unsupported HTTP method: " + method);
            }
            
            return request;
        }

        /// <summary>
        /// Performs an asynchronous UnityWebRequest and returns the result as a <see cref="WebResponse"/>.
        /// <returns><see cref="WebResponse"/>, or <see cref="FailedWebResponse"/></returns>
        /// </summary>
        internal static async UniTask<WebResponse> SendAsyncWebRequest(RequestMethod method, string url,
            string bodyData = "", Dictionary<string, string> headers = null, float timeout = DefaultTimeoutMilliseconds, CancellationToken ct = default)
        {
            return await PerformAsyncWebRequest(CreateRequest(method, url, bodyData), headers, timeout, ct);
        }

        /// <summary>
        /// Performs an asynchronous UnityWebRequest designed to download a texture, and returns the result as a <see cref="WebResponse"/>.
        /// <returns><see cref="TextureWebResponse"/>, or <see cref="FailedWebResponse"/>></returns>
        /// </summary>
        internal static async UniTask<WebResponse> DownloadTextureAsync(RequestMethod method, string url,
            string bodyData = "", Dictionary<string, string> headers = null, float timeout = DefaultTimeoutMilliseconds, bool nonReadable = false, CancellationToken ct = default)
        {
            UnityWebRequest request = CreateRequest(method, url, bodyData);
            request.downloadHandler = new DownloadHandlerTexture(!nonReadable);
            return await PerformAsyncWebRequest(request, headers, timeout, ct);
        }

        private static UnityWebRequest RequestWithJsonBody(string url, string method, string bodyData)
        {
            var request = new UnityWebRequest(url, method);
            request.downloadHandler = new DownloadHandlerBuffer();
            if (string.IsNullOrEmpty(bodyData))
                return request;
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyData));
            request.uploadHandler.contentType = "application/json"; // Default content type is JSON for POST/PUT/PATCH requests
            return request;
        }

        private static void SetupRequestHeaders(UnityWebRequest request, Dictionary<string, string> headers)
        {
            if (headers != null)
            {
                foreach (var key in headers.Keys)
                {
                    request.SetRequestHeader(key, headers[key]);
                }
            }
        }

        private WebRequestInfo AddRequest(UnityWebRequest request, Dictionary<string, string> headers)
        {
            var webRequestInfo = new WebRequestInfo(headers, request);
            openRequests.TryAdd(request, webRequestInfo);
            allRequests.TryAdd(request, webRequestInfo);
            return webRequestInfo;
        }

        private void CloseRequest(UnityWebRequest request)
        {
            openRequests.TryRemove(request, out _);
        }

        internal void RemoveRequest(UnityWebRequest request)
        {
            openRequests.TryRemove(request, out _);
            allRequests.TryRemove(request, out _);
        }

        internal WebRequestInfo GetRequestInfo(UnityWebRequest request)
        {
            return allRequests.GetValueOrDefault(request);
        }

        internal WebRequestInfo GetRequestInfoByWebResponse(WebResponse response)
        {
            if (response != null)
            {
                foreach (var webRequestInfo in allRequests)
                {
                    if (webRequestInfo.Value.Response != null && webRequestInfo.Value.Response == response)
                    {
                        return webRequestInfo.Value;
                    }
                }
            }

            return null;
        }
        
        private static async UniTask<WebResponse> PerformAsyncWebRequest(UnityWebRequest request, Dictionary<string, string> headers = null, float timeout = DefaultTimeoutMilliseconds, CancellationToken ct = default)
        {
            WebResponse response = null;
            var requestInfo = Instance.AddRequest(request, headers);
            if (headers != null)
            {
                SetupRequestHeaders(request, headers);
            }

            try
            {
                EmergenceLogger.LogInfo($"Request #{requestInfo.Id}: Performing {request.method} request to {request.url}, DeviceId: {EmergenceSingleton.Instance.CurrentDeviceId}");
                var sendTask = request.SendWebRequest().WithCancellation(ct);

                await sendTask.Timeout(TimeSpan.FromMilliseconds(timeout));

                // Rest of the code if the request completes within the timeout
                response = request.downloadHandler is DownloadHandlerTexture
                    ? new TextureWebResponse(request)
                    : new WebResponse(request);
            }
            catch (Exception e)
            {
                response = HandleRequestException(e, request);

                if (e is OperationCanceledException)
                {
                    throw;
                }
            }
            finally
            {
                FinalizeRequest(requestInfo, request, response);
            }

            return response;
        }

        private static WebResponse HandleRequestException(Exception e, UnityWebRequest request)
        {
            if (e is TimeoutException)
            {
                request.Abort(); // Abort the request in case of timeout
            }

            return new FailedWebResponse(e, request);
        }

        private static void FinalizeRequest(WebRequestInfo requestInfo, UnityWebRequest request, WebResponse response)
        {
            requestInfo.Response = response;
            EmergenceLogger.LogWebResponse(response);
            Instance.CloseRequest(request); // Remove the request from tracking
        }
    }
}
