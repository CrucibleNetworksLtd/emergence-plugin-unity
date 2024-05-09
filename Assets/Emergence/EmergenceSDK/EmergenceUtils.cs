using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types.Delegates;
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
        internal static bool RequestError(UnityWebRequest request)
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
        /// Processes a UnityWebRequest response and returns the result as a response object.
        /// </summary>
        internal static bool ProcessRequest<T>(UnityWebRequest request, ErrorCallback errorCallback, out T response)
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
        internal static bool ProcessResponse<T>(UnityWebRequest request, out BaseResponse<T> response, out BaseResponse<string> errorResponse)
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
    }
}
