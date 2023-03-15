using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Services;
using EmergenceSDK.Internal.UI;
using EmergenceSDK.Internal.UI.Screens;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types.Responses;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// The services singleton provides you with all the methods you need to get going with Emergence.
    /// </summary>
    /// <remarks>See our prefabs for examples of how to use it!</remarks>
    public partial class EmergenceServices : MonoBehaviour
    {
        public string CurrentAccessToken => AccountService.CurrentAccessToken;
        
        public bool DisconnectInProgress => AccountService.DisconnectInProgress;

        public static EmergenceServices Instance;

        public IPersonaService PersonaService { get; private set; }

        public IAvatarService AvatarService { get; private set; }
        
        public IInventoryService InventoryService { get; private set; }
        
        public IDynamicMetadataService DynamicMetadataService { get; private set; }
        
        public IAccountService AccountService { get; private set; }
        
        public IWalletService WalletService { get; private set; }
        
        public IQRCodeService QRCodeService { get; private set; }
 
        private bool skipWallet = false;

        #region Monobehaviour

        private void Awake()
        {
            Instance = this;
            PersonaService = gameObject.AddComponent<PersonaService>();
            AvatarService = new AvatarService();
            InventoryService = new InventoryService();
            DynamicMetadataService = new DynamicMetadataService();
            AccountService = gameObject.AddComponent<AccountService>();
            WalletService = gameObject.AddComponent<WalletService>();
            QRCodeService = gameObject.AddComponent<QRCodeService>();
        }

        private bool refreshingToken = false;
        private void Update()
        {
            if (ScreenManager.Instance == null)
            {
                return;
            }

            bool uiIsVisible = ScreenManager.Instance.gameObject.activeSelf;

            if (!skipWallet && uiIsVisible && !refreshingToken && AccountService.HasAccessToken)
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

        public void ProcessExpiration(string expirationMessage)
        {
            expiration = SerializationHelper.Deserialize<Expiration>(expirationMessage);
        }

        public static bool RequestError(UnityWebRequest request)
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

        public static void PrintRequestResult(string name, UnityWebRequest request)
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

        //TODO: move this to a utility class
        public static bool ProcessRequest<T>(UnityWebRequest request, ErrorCallback errorCallback, out T response)
        {
            Debug.Log("Processing request: " + request.url);
            
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

        //TODO: move this to a utility class
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

        //TODO: move this to a utility class
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
                Debug.Log("AccessToken: " + EmergenceServices.Instance.CurrentAccessToken);
                request.SetRequestHeader("Authorization", EmergenceServices.Instance.CurrentAccessToken);

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

        #endregion Utilities

        #region No Wallet Cheat

        public void SkipWallet(bool skip, string accessTokenJson)
        {
            skipWallet = skip;

            BaseResponse<AccessTokenResponse> response = SerializationHelper.Deserialize<BaseResponse<AccessTokenResponse>>(accessTokenJson);
            AccountService.CurrentAccessToken = SerializationHelper.Serialize(response.message.AccessToken, false);
            ProcessExpiration(response.message.AccessToken.message);
        }

        #endregion No Wallet Cheat

    }
}