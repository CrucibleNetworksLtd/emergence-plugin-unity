using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using Cysharp.Threading.Tasks;

namespace EmergenceSDK
{
    public partial class Services : MonoBehaviour
    {

        private string currentAccessToken = string.Empty;

        public static Services Instance;

        private bool skipWallet = false;

        public delegate void GenericError(string message, long code);

        #region Monobehaviour

        private void Awake()
        {
            Instance = this;
        }

        private bool refreshingToken = false;
        private void Update()
        {
            if (ScreenManager.Instance == null)
            {
                return;
            }

            bool uiIsVisible = ScreenManager.Instance.gameObject.activeSelf;

            if (!skipWallet && uiIsVisible && !refreshingToken && HasAccessToken)
            {
                long now = DateTimeOffset.Now.ToUnixTimeSeconds();

                if (expiration.expiresOn - now < 0)
                {
                    refreshingToken = true;
                    ModalPromptOK.Instance.Show("Token expired. Check your wallet for renewal", () =>
                    {
                        GetAccessToken((token) =>
                        {
                            refreshingToken = false;
                        },
                        (error, code) =>
                        {
                            Debug.LogError("[" + code + "] " + error);
                            refreshingToken = false;
                        });
                    });
                }
            }
        }

        #endregion Monobehaviour

        #region Utilities

        private class Expiration
        {
            [JsonProperty("expires-on")]
            public long expiresOn;
        }

        private Expiration expiration;
        private void ProcessExpiration(string expirationMessage)
        {
            expiration = SerializationHelper.Deserialize<Expiration>(expirationMessage);
        }

        internal static bool RequestError(UnityWebRequest request)
        {
            bool error = false;
#if UNITY_2020_1_OR_NEWER
            error = (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError ||
                request.result == UnityWebRequest.Result.DataProcessingError);
#else
            error = (request.isHttpError || request.isNetworkError);
#endif

            if (error && request.responseCode == 512)
            {
                error = false;
            }

            return error;
        }

        internal static void PrintRequestResult(string name, UnityWebRequest request)
        {
            Debug.Log(name + " completed " + request.responseCode);
            if (RequestError(request))
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
            }
        }

        public bool ProcessRequest<T>(UnityWebRequest request, GenericError error, out T response)
        {
            Debug.Log("Processing request: " + request.url);
            
            bool isOk = false;
            response = default(T);

            if (RequestError(request))
            {
                error?.Invoke(request.error, request.responseCode);
            }
            else
            {
                BaseResponse<T> okresponse;
                BaseResponse<string> errorResponse;
                if (!ProcessResponse(request, out okresponse, out errorResponse))
                {
                    error?.Invoke(errorResponse.message, (long)errorResponse.statusCode);
                }
                else
                {
                    isOk = true;
                    response = okresponse.message;
                }
            }

            return isOk;
        }

        public bool ProcessResponse<T>(UnityWebRequest request, out BaseResponse<T> response, out BaseResponse<string> errorResponse)
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

        private async UniTask<string> PerformAsyncWebRequest(string url, string method, GenericError error, string bodyData = "", Dictionary<string, string> headers = null)
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
                Debug.Log("AccessToken: " + currentAccessToken);
                request.SetRequestHeader("Authorization", currentAccessToken);

                if (headers != null) {
                    foreach (var key in headers.Keys) {
                        request.SetRequestHeader(key, headers[key]);
                    }
                }
                return (await request.SendWebRequest()).downloadHandler.text;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                error?.Invoke(request.error, request.responseCode);
                return ex.Message;
            }
        }

        #endregion Utilities

        #region No Wallet Cheat

        public void SkipWallet(bool skip, string accessTokenJson)
        {
            skipWallet = skip;

            BaseResponse<AccessTokenResponse> response = SerializationHelper.Deserialize<BaseResponse<AccessTokenResponse>>(accessTokenJson);
            currentAccessToken = SerializationHelper.Serialize(response.message.AccessToken, false);
            ProcessExpiration(response.message.AccessToken.message);
        }

        #endregion No Wallet Cheat

    }
}