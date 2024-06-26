using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Types.Delegates;
using EmergenceSDK.Runtime.Types.Responses;
using WebResponse = EmergenceSDK.Runtime.Internal.Types.WebResponse;

namespace EmergenceSDK.Runtime.Utilities
{
    /// <summary>
    /// A static class containing utility methods for making HTTP requests and handling responses in the Emergence SDK.
    /// </summary>
    public static class EmergenceUtils
    {
        /// <summary>
        /// Checks if there was an error in the WebResponse result.
        /// </summary>
        internal static bool ResponseError(WebResponse request)
        {
            bool error = !request.InProgress && !request.Successful;

            if (error && request.StatusCode == 512)
            {
                error = false;
            }

            return error;
        }

        /// <summary>
        /// Processes a UnityWebRequest response and returns the result as a response object.
        /// </summary>
        internal static bool ProcessResponse<T>(WebResponse webResponse, ErrorCallback errorCallback, out T responseObject)
        {
            EmergenceLogger.LogInfo("Processing request: " + webResponse.Url);
            
            bool isOk = false;
            responseObject = default(T);

            if (ResponseError(webResponse))
            {
                errorCallback?.Invoke(webResponse.Error, webResponse.StatusCode);
            }
            else
            {
                BaseResponse<T> okresponse;
                BaseResponse<string> errorResponse;
                if (!ProcessResponse(webResponse, out okresponse, out errorResponse))
                {
                    errorCallback?.Invoke(errorResponse.message, (long)errorResponse.statusCode);
                }
                else
                {
                    isOk = true;
                    responseObject = okresponse.message;
                }
            }

            return isOk;
        }

        /// <summary>
        /// Processes the response of a UnityWebRequest and returns the result as a response object or an error response object.
        /// </summary>
        internal static bool ProcessResponse<T>(WebResponse webResponse, out BaseResponse<T> responseObject, out BaseResponse<string> errorResponse)
        {
            bool isOk = true;
            errorResponse = null;
            responseObject = null;

            if (webResponse.StatusCode == 512)
            {
                isOk = false;
                errorResponse = SerializationHelper.Deserialize<BaseResponse<string>>(webResponse.ResponseText);
            }
            else
            {
                responseObject = SerializationHelper.Deserialize<BaseResponse<T>>(webResponse.ResponseText);
            }

            return isOk;
        }
    }
}
