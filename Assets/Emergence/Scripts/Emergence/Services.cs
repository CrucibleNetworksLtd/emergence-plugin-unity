using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace EmergenceSDK
{
    public partial class Services : MonoBehaviour
    {
        private string nodeURL = string.Empty;
        private string gameId = string.Empty;
        private string currentAccessToken = string.Empty;

        private string address = string.Empty;

        public static Services Instance;

        private bool skipWallet = false;

        private EnvValues envValues = null;

        public delegate void GenericError(string message, long code);

        #region Monobehaviour

        private void Awake()
        {
            Instance = this;
            TextAsset envFile;

            try
            {
                envFile = Resources.Load<TextAsset>("emergence.env.dev");

                if (envFile == null)
                {
                    envFile = Resources.Load<TextAsset>("emergence.env.staging");
                }

                if (envFile == null)
                {
                    envFile = Resources.Load<TextAsset>("emergence.env");
                }

                if (envFile == null)
                {
                    Debug.LogError("emergence.env file missing from Resources folder");
                    return;
                }

                envValues = SerializationHelper.Deserialize<EnvValues>(envFile.text);

                if (envValues == null)
                {
                    Debug.LogError("emergence.env file is corrupted");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
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

        #region Properties

        public bool HasAccessToken
        {
            get
            {
                return currentAccessToken.Length > 0;
            }
        }

        #endregion Properties

        #region Utilities

        private bool CheckEnv()
        {
            return envValues != null;
        }

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

        private bool RequestError(UnityWebRequest request)
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

        private void PrintRequestResult(string name, UnityWebRequest request)
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

        private bool ProcessRequest<T>(UnityWebRequest request, GenericError error, out T response)
        {
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

        private bool ProcessResponse<T>(UnityWebRequest request, out BaseResponse<T> response, out BaseResponse<string> errorResponse)
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

        #endregion Utilities

        #region No Wallet Cheat

        public void SkipWallet(bool skip, string accessTokenJson)
        {
            skipWallet = skip;

            BaseResponse<AccessTokenResponse> response = SerializationHelper.Deserialize<BaseResponse<AccessTokenResponse>>(accessTokenJson);
            currentAccessToken = SerializationHelper.Serialize(response.message.accessToken, false);
            ProcessExpiration(response.message.accessToken.message);
        }

        #endregion No Wallet Cheat

    }
}