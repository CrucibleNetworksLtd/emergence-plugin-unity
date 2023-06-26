using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Services;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class WebRequestService
    {
        private static WebRequestService instance;
        public static WebRequestService Instance => instance ?? (instance = new WebRequestService());

        private ConcurrentDictionary<UnityWebRequest, DateTime> openRequests = new ConcurrentDictionary<UnityWebRequest, DateTime>();

        //This timeout avoids this issue: https://forum.unity.com/threads/catching-curl-error-28.1274846/
        private const int TimeoutMilliseconds = 4999;

        public static UnityWebRequest CreateRequest(string method, string url, string bodyData = "")
        {
            return GetRequestFromMethodType(method, url, bodyData);
        }
        
        /// <summary>
        /// Performs an asynchronous UnityWebRequest and returns the result as a string.
        /// </summary>
        public static async UniTask<string> PerformAsyncWebRequest(string url, string method, ErrorCallback errorCallback, string bodyData = "", Dictionary<string, string> headers = null)
        {
            UnityWebRequest request = GetRequestFromMethodType(method, url, bodyData);
            Instance.AddOpenRequest(request);

            SetupRequestHeaders(request, headers);
            return await PerformAsyncWebRequest(request, errorCallback);
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

            return ret;
        }

        private static UnityWebRequest GenerateGetRequest(string url) => UnityWebRequest.Get(url);

        private static UnityWebRequest GeneratePostRequest(string url, string bodyData)
        {
            var request = UnityWebRequest.Post(url, string.Empty);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyData));
            request.uploadHandler.contentType = "application/json";
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

        public static async UniTask<string> PerformAsyncWebRequest(UnityWebRequest request, ErrorCallback errorCallback)
        {
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
                        return request.downloadHandler.text;
                    }
                    else
                    {
                        errorCallback?.Invoke(request.error, request.responseCode);
                        return request.error;
                    }
                }
                catch (TimeoutException)
                {
                    request.Abort(); // Abort the request

                    errorCallback?.Invoke("Request timed out.", 0);
                    return "Request timed out.";
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                errorCallback?.Invoke(request.error, request.responseCode);
                return ex.Message;
            }
            finally
            {
                Instance.RemoveOpenRequest(request); // Remove the request from tracking
            }
        }
    }
}
