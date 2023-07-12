using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class WebRequestService
    {
        private static WebRequestService instance;
        public static WebRequestService Instance => instance ??= new WebRequestService();

        private ConcurrentDictionary<UnityWebRequest, DateTime> openRequests = new ConcurrentDictionary<UnityWebRequest, DateTime>();

        //This timeout avoids this issue: https://forum.unity.com/threads/catching-curl-error-28.1274846/
        private const int TimeoutMilliseconds = 100000;

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

        public static UnityWebRequest CreateRequest(string method, string url, string bodyData = "")
        {
            return GetRequestFromMethodType(method, url, bodyData);
        }
        
        /// <summary>
        /// Performs an asynchronous UnityWebRequest and returns the result as a string.
        /// </summary>
        public static async UniTask<WebResponse> PerformAsyncWebRequest(string url, string method, ErrorCallback errorCallback, string bodyData = "", Dictionary<string, string> headers = null)
        {
            UnityWebRequest request = GetRequestFromMethodType(method, url, bodyData);
            Instance.AddOpenRequest(request);

            SetupRequestHeaders(request, headers);
            var ret = await PerformAsyncWebRequest(request, errorCallback);
            CleanupRequest(request);
            return ret;
        }

        public static void CleanupRequest(UnityWebRequest request)
        {
            Instance.RemoveOpenRequest(request);
            request.uploadHandler?.Dispose();
        }

        private static void SetupRequestHeaders(UnityWebRequest request, Dictionary<string, string> headers)
        {
            var personaService = EmergenceServices.GetService<IPersonaService>();
            request.SetRequestHeader("Authorization", personaService.CurrentAccessToken);

            if (headers != null)
            {
                foreach (var key in headers.Keys)
                {
                    request.SetRequestHeader(key, headers[key]);
                }
            }
        }
        
        private static UnityWebRequest GetRequestFromMethodType(string method, string url, string bodyData)
        {
            UnityWebRequest ret;
            switch (method)
            {
                case UnityWebRequest.kHttpVerbGET:
                    ret = GenerateGetRequest(url);
                    break;
                case UnityWebRequest.kHttpVerbPOST:
                    ret = GeneratePostRequest(url, bodyData);
                    break;
                default:
                    throw new Exception("Unsupported HTTP method: " + method);
            }
            
            ret.disposeUploadHandlerOnDispose = true;
            ret.disposeDownloadHandlerOnDispose = true;
            
            return ret;
        }

        private static UnityWebRequest GenerateGetRequest(string url) => UnityWebRequest.Get(url);

        private static UnityWebRequest GeneratePostRequest(string url, string bodyData)
        {
            var request = UnityWebRequest.Post(url, string.Empty);
            if(bodyData.Length > 0)
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyData));
                request.uploadHandler.contentType = "application/json";
            }
            return request;
        }

        private void AddOpenRequest(UnityWebRequest request)
        {
            openRequests.TryAdd(request, DateTime.UtcNow);
        }

        private void RemoveOpenRequest(UnityWebRequest request)
        {
            openRequests.TryRemove(request, out _);
        }

        public static async UniTask<WebResponse> PerformAsyncWebRequest(UnityWebRequest request, ErrorCallback errorCallback)
        {
            EmergenceLogger.LogInfo($"Performing {request.method} request to {request.url}, DeviceId: {EmergenceSingleton.Instance.CurrentDeviceId}");
            try
            {
                var sendTask = request.SendWebRequest().ToUniTask();

                try
                {
                    await sendTask.Timeout(TimeSpan.FromMilliseconds(TimeoutMilliseconds));

                    // Rest of the code if the request completes within the timeout

                    var response = request.result;
                    if (response == UnityWebRequest.Result.Success)
                    {
                        return new WebResponse(true, request.downloadHandler.text);
                    }
                    else
                    {
                        errorCallback?.Invoke(request.error, request.responseCode);
                        return new WebResponse(false, request.error);
                    }
                }
                catch (TimeoutException)
                {
                    request.Abort(); // Abort the request

                    errorCallback?.Invoke("Request timed out.", 0);
                    return new WebResponse(false,"Request timed out.");
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                errorCallback?.Invoke(request.error, request.responseCode);
                return new WebResponse(false, ex.Message);
            }
            finally
            {
                Instance.RemoveOpenRequest(request); // Remove the request from tracking
            }
        }
    }
}
