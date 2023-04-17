using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class SessionService : ISessionService
    {
        public string CurrentAccessToken
        {
            get => currentAccessToken;
            set => currentAccessToken = value;
        }
        private string currentAccessToken = string.Empty;
        public bool HasAccessToken => currentAccessToken.Length > 0;
        
        public bool DisconnectInProgress => disconnectInProgress;
        private bool disconnectInProgress = false;
        
        public Expiration Expiration { get; private set; }

        public void ProcessExpiration(string expirationMessage)
        {
            Expiration = SerializationHelper.Deserialize<Expiration>(expirationMessage);
        }

        public async UniTask IsConnected(IsConnectedSuccess success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "isConnected";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
            await request.SendWebRequest().ToUniTask();
            
            EmergenceUtils.PrintRequestResult("IsConnected", request);
            if (EmergenceUtils.ProcessRequest<IsConnectedResponse>(request, errorCallback, out var response))
            {
                success?.Invoke(response.isConnected);
            }
        }

        public async UniTask CreateKeyStore(string privateKey, string password, string publicKey, string path,
            CreateKeyStoreSuccess success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "createKeyStore" + "?privateKey=" +
                         privateKey + "&password=" + password + "&publicKey=" + publicKey + "&path=" + path;

            using UnityWebRequest request = UnityWebRequest.Post(url, "");
            await request.SendWebRequest().ToUniTask();
            
            EmergenceUtils.PrintRequestResult("Key Store", request);
            if (EmergenceUtils.ProcessRequest<string>(request, errorCallback, out var response))
            {
                success?.Invoke();
            }
        }

        public async UniTask LoadAccount(Account account, LoadAccountSuccess success, ErrorCallback errorCallback)
        {
            string dataString = SerializationHelper.Serialize(account, false);
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "loadAccount";

            using UnityWebRequest request = UnityWebRequest.Post(url, "");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
            request.uploadHandler.contentType = "application/json";

            await request.SendWebRequest().ToUniTask();
            EmergenceUtils.PrintRequestResult("Load Account", request);
            if (EmergenceUtils.ProcessRequest<LoadAccountResponse>(request, errorCallback, out var response))
            {
                success?.Invoke();
            }
        }
        
        public async UniTask GetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "get-access-token";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
            await request.SendWebRequest().ToUniTask();
            EmergenceUtils.PrintRequestResult("GetAccessToken", request);
            if (EmergenceUtils.ProcessRequest<AccessTokenResponse>(request, errorCallback, out var response))
            {
                currentAccessToken = SerializationHelper.Serialize(response.AccessToken, false);
                ProcessExpiration(response.AccessToken.message);
                success?.Invoke(currentAccessToken);
            }
        }
        
        public async UniTask ValidateAccessToken(ValidateAccessTokenSuccess success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "validate-access-token" +
                         "?accessToken=" + currentAccessToken;

            using UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest().ToUniTask();
            EmergenceUtils.PrintRequestResult("ValidateAccessToken", request);
            if (EmergenceUtils.ProcessRequest<ValidateAccessTokenResponse>(request, errorCallback, out var response))
            {
                success?.Invoke(response.valid);
            }
        }
        
        public async UniTask ValidateSignedMessage(string message, string signedMessage, string address,
            ValidateSignedMessageSuccess success, ErrorCallback errorCallback)
        {
            ValidateSignedMessageRequest data = new ValidateSignedMessageRequest()
            {
                message = message,
                signedMessage = signedMessage,
                address = address
            };

            string dataString = SerializationHelper.Serialize(data, false);

            string url = EmergenceSingleton.Instance.Configuration.APIBase + "validate-signed-message" + "?request=" +
                         currentAccessToken;

            using UnityWebRequest request = UnityWebRequest.Post(url, "");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
            request.uploadHandler.contentType = "application/json";
            await request.SendWebRequest().ToUniTask();
            EmergenceUtils.PrintRequestResult("ValidateSignedMessage", request);
            if (EmergenceUtils.ProcessRequest<ValidateSignedMessageResponse>(request, errorCallback, out var response))
            {
                success?.Invoke(response.valid);
            }
        }

        public async UniTask Disconnect(DisconnectSuccess success, ErrorCallback errorCallback)
        {
            disconnectInProgress = true;
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "killSession";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
            request.SetRequestHeader("auth", currentAccessToken);
            await request.SendWebRequest().ToUniTask();
            EmergenceUtils.PrintRequestResult("Disconnect request completed", request);

            if (EmergenceUtils.RequestError(request))
            {
                disconnectInProgress = false;
                errorCallback?.Invoke(request.error, request.responseCode);
            }
            else
            {
                disconnectInProgress = false;
                success?.Invoke();
            }
        }

        public async UniTask Finish(SuccessFinish success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "finish";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest().ToUniTask();
            EmergenceUtils.PrintRequestResult("Finish request completed", request);

            if (EmergenceUtils.RequestError(request))
            {
                errorCallback?.Invoke(request.error, request.responseCode);
            }
            else
            {
                success?.Invoke();
            }
        }

    }
}
