using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types.Responses;
using UnityEngine.Networking;

namespace EmergenceSDK
{
    /// <summary>
    /// A static class containing utility methods for making HTTP requests and handling responses in the Emergence SDK.
    /// </summary>
    public static class EmergenceUtils
    {
        /// <summary>
        /// Checks if there was an error in the UnityWebRequest result.
        /// </summary>
        public static bool RequestError(UnityWebRequest request)
        {
             bool error = request.result == UnityWebRequest.Result.ConnectionError ||
                          request.result == UnityWebRequest.Result.ProtocolError ||
                          request.result == UnityWebRequest.Result.DataProcessingError;

            if (error && request.responseCode == 512)
            {
                error = false;
            }

            return error;
        }

        /// <summary>
        /// Prints the result of a UnityWebRequest to the console.
        /// </summary>
        public static void PrintRequestResult(string name, UnityWebRequest request)
        {
            EmergenceLogger.LogInfo(name + " completed " + request.responseCode);
            if (RequestError(request))
            {
                EmergenceLogger.LogError(request.error);
            }
            else
            {
                EmergenceLogger.LogInfo(request.downloadHandler.text);
            }
        }
        
        /// <summary>
        /// Processes a UnityWebRequest response and returns the result as a response object.
        /// </summary>
        public static bool ProcessRequest<T>(UnityWebRequest request, ErrorCallback errorCallback, out T response)
        {
            EmergenceLogger.LogInfo("Processing request: " + request.url);
            
            bool isOk = false;
            response = default(T);

            if (RequestError(request))
            {
                errorCallback?.Invoke(request.error, request.responseCode);
            }
            else
            {
                BaseResponse<T> okresponse;
                BaseResponse<string> errorResponse;
                if (!ProcessResponse(request, out okresponse, out errorResponse))
                {
                    errorCallback?.Invoke(errorResponse.message, (long)errorResponse.statusCode);
                }
                else
                {
                    isOk = true;
                    response = okresponse.message;
                }
            }

            return isOk;
        }

        /// <summary>
        /// Processes the response of a UnityWebRequest and returns the result as a response object or an error response object.
        /// </summary>
        public static bool ProcessResponse<T>(UnityWebRequest request, out BaseResponse<T> response, out BaseResponse<string> errorResponse)
        {
            bool isOk = true;
            errorResponse = null;
            response = null;

            if (request.responseCode == 512)
            {
                isOk = false;
                errorResponse = SerializationHelper.Deserialize<BaseResponse<string>>(request.downloadHandler.text);
            }
            else
            {
                response = SerializationHelper.Deserialize<BaseResponse<T>>(request.downloadHandler.text);
            }

            return isOk;
        }

        /// <summary>
        /// Performs an asynchronous UnityWebRequest and returns the result as a string.
        /// </summary>
        public static async UniTask<string> PerformAsyncWebRequest(string url, string method, ErrorCallback errorCallback, string bodyData = "", Dictionary<string, string> headers = null)
        {
            UnityWebRequest request;
            if (method.Equals(UnityWebRequest.kHttpVerbGET))
            {
                request = UnityWebRequest.Get(url);
            }
            else
            {
                request = UnityWebRequest.Post(url, string.Empty);
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(bodyData));
                request.uploadHandler.contentType = "application/json";
            }
            try
            {
                var personaService = EmergenceServices.GetService<IPersonaService>();
                EmergenceLogger.LogInfo("AccessToken: " + personaService.CurrentAccessToken);
                request.SetRequestHeader("Authorization", personaService.CurrentAccessToken);

                if (headers != null) {
                    foreach (var key in headers.Keys) {
                        request.SetRequestHeader(key, headers[key]);
                    }
                }
                return (await request.SendWebRequest()).downloadHandler.text;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                errorCallback?.Invoke(request.error, request.responseCode);
                return ex.Message;
            }
        }
    }
}
