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
    internal class WebRequestService
    {
        internal class WebRequestInfo
        {
            private static int _lastId = -1;
            public readonly DateTime Time;
            public readonly int Id;
            /// <summary>
            /// The Request headers, only populated if the headers are passed to
            /// <see cref="WebRequestService.PerformAsyncWebRequest"/>
            /// to send the request
            /// </summary>
            public readonly Dictionary<string, string> Headers;
            public WebResponse Response { get; internal set; }

            public WebRequestInfo(Dictionary<string, string> requestHeaders, UnityWebRequest request)
            {
                Id = ++_lastId;
                Time = DateTime.Now;
                Headers = requestHeaders ?? new Dictionary<string, string>();
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
        
        private static WebRequestService _instance;
        
        // ReSharper disable once MemberCanBePrivate.Global
        public static WebRequestService Instance => _instance ??= new WebRequestService();
        private readonly ConcurrentDictionary<UnityWebRequest, WebRequestInfo> _openRequests = new();
        private readonly ConcurrentDictionary<UnityWebRequest, WebRequestInfo> _allRequests = new();

        //This timeout avoids this issue: https://forum.unity.com/threads/catching-curl-error-28.1274846/
        private const int DefaultTimeoutMilliseconds = 100000;

        private WebRequestService()
        {
            EmergenceSingleton.Instance.OnGameClosing += CancelAllRequests;
        }

        private void CancelAllRequests()
        {
            foreach (var openRequest in _openRequests)
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
                    request = new UnityWebRequest(url, "POST");
                    request.downloadHandler = new DownloadHandlerBuffer();
                    if (string.IsNullOrEmpty(bodyData))
                        break;
                    request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyData));
                    request.uploadHandler.contentType = "application/json"; // Default content type is JSON for POST/PUT/PATCH requests
                    break;
                case RequestMethod.Put:
                    request = UnityWebRequest.Put(url, bodyData);
                    request.uploadHandler.contentType = "application/json"; // Default content type is JSON for POST/PUT/PATCH requests
                    break;
                case RequestMethod.Patch:
                    request = UnityWebRequest.Post(url, bodyData);
                    request.method = "PATCH";
                    request.uploadHandler.contentType = "application/json"; // Default content type is JSON for POST/PUT/PATCH requests
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
        /// </summary>
        public static async UniTask<WebResponse> SendAsyncWebRequest(RequestMethod method, string url,
            string bodyData = "", Dictionary<string, string> headers = null, float timeout = DefaultTimeoutMilliseconds, CancellationToken ct = default)
        {
            return await PerformAsyncWebRequest(CreateRequest(method, url, bodyData), headers, timeout, ct);
        }
        
        /// <summary>
        /// Performs an asynchronous UnityWebRequest designed to download a texture, and returns the result as a <see cref="WebResponse"/>.
        /// </summary>
        public static async UniTask<WebResponse> DownloadTextureAsync(RequestMethod method, string url,
            string bodyData = "", Dictionary<string, string> headers = null, float timeout = DefaultTimeoutMilliseconds, bool nonReadable = false, CancellationToken ct = default)
        {
            UnityWebRequest request = CreateRequest(method, url, bodyData);
            request.downloadHandler = new DownloadHandlerTexture(!nonReadable);
            return await PerformAsyncWebRequest(request, headers, timeout, ct);
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
            _openRequests.TryAdd(request, webRequestInfo);
            _allRequests.TryAdd(request, webRequestInfo);
            return webRequestInfo;
        }

        private void CloseRequest(UnityWebRequest request)
        {
            _openRequests.TryRemove(request, out _);
        }

        internal void RemoveRequest(UnityWebRequest request)
        {
            _openRequests.TryRemove(request, out _);
            _allRequests.TryRemove(request, out _);
        }

        internal WebRequestInfo GetRequestInfo(UnityWebRequest request)
        {
            return _allRequests.GetValueOrDefault(request);
        }

        internal WebRequestInfo GetRequestInfoByWebResponse(WebResponse response)
        {
            if (response != null)
            {
                foreach (var webRequestInfo in _allRequests)
                {
                    if (webRequestInfo.Value.Response != null && webRequestInfo.Value.Response == response)
                    {
                        return webRequestInfo.Value;
                    }
                }
            }

            return null;
        }
        
        private static async UniTask<WebResponse> PerformAsyncWebRequest(UnityWebRequest request,
            Dictionary<string, string> headers = null,
            float timeout = DefaultTimeoutMilliseconds,
            CancellationToken ct = default)
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

                try
                {
                    await sendTask.Timeout(TimeSpan.FromMilliseconds(timeout));

                    // Rest of the code if the request completes within the timeout
                    response = request.downloadHandler is DownloadHandlerTexture
                        ? new TextureWebResponse(request)
                        : new WebResponse(request);
                    requestInfo.Response = response;

                    return response;
                }
                catch (TimeoutException e)
                {
                    request.Abort(); // Abort the request
                    response = new FailedWebResponse(e, request);
                    requestInfo.Response = response;
                    
                    return response;
                }
            }
            catch (UnityWebRequestException e)
            {
                response = new FailedWebResponse(e, request);
                requestInfo.Response = response;
                
                return response;
            }
            catch (OperationCanceledException e)
            {
                response = new FailedWebResponse(e, request);
                requestInfo.Response = response;
                
                throw;
            }
            catch (Exception e)
            {
                response = new FailedWebResponse(e, request);
                requestInfo.Response = response;
                
                return response;
            }
            finally
            {
                EmergenceLogger.LogWebResponse(response);
                Instance.CloseRequest(request); // Remove the request from tracking
            }
        }
    }
}
