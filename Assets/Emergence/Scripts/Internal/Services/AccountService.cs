using System.Collections;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    public class AccountService : MonoBehaviour, IAccountService
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

        public void IsConnected(IsConnectedSuccess success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineIsConnected(success, errorCallback));
        }

        private IEnumerator CoroutineIsConnected(IsConnectedSuccess success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "isConnected";
            Debug.Log("url: " + url);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("IsConnected", request);
                if (EmergenceUtils.ProcessRequest<IsConnectedResponse>(request, errorCallback, out var response))
                {
                    success?.Invoke(response.isConnected);
                }
            }
        }
        
        public void CreateKeyStore(string privateKey, string password, string publicKey, string path,
            CreateKeyStoreSuccess success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineKeyStore(privateKey, password, publicKey, path, success, errorCallback));
        }

        private IEnumerator CoroutineKeyStore(string privateKey, string password, string publicKey, string path,
            CreateKeyStoreSuccess success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "createKeyStore" + "?privateKey=" +
                         privateKey + "&password=" + password + "&publicKey=" + publicKey + "&path=" + path;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";

                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("Key Store", request);
                if (EmergenceUtils.ProcessRequest<string>(request, errorCallback, out var response))
                {
                    success?.Invoke();
                }
            }
        }
        
        public void LoadAccount(string name, string password, string path, string nodeURL, string chainId,
            LoadAccountSuccess success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineLoadAccount(name, password, path, nodeURL, chainId, success, errorCallback));
        }

        private IEnumerator CoroutineLoadAccount(string name, string password, string path, string nodeURL,
            string chainId, LoadAccountSuccess success, ErrorCallback errorCallback)
        {
            Account data = new Account()
            {
                name = name,
                password = password,
                path = path,
                nodeURL = nodeURL,
                chainId = chainId
            };

            string dataString = SerializationHelper.Serialize(data, false);
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "loadAccount";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("Load Account", request);
                if (EmergenceUtils.ProcessRequest<LoadAccountResponse>(request, errorCallback, out var response))
                {
                    success?.Invoke();
                }
            }
        }
        
        public void GetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineGetAccessToken(success, errorCallback));
        }

        private IEnumerator CoroutineGetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "get-access-token";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("GetAccessToken", request);
                if (EmergenceUtils.ProcessRequest<AccessTokenResponse>(request, errorCallback, out var response))
                {
                    currentAccessToken = SerializationHelper.Serialize(response.AccessToken, false);
                    EmergenceUtils.ProcessExpiration(response.AccessToken.message);
                    success?.Invoke(currentAccessToken);
                }
            }
        }
        
        public void ValidateAccessToken(ValidateAccessTokenSuccess success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineValidateAccessToken(success, errorCallback));
        }

        private IEnumerator CoroutineValidateAccessToken(ValidateAccessTokenSuccess success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "validate-access-token" +
                         "?accessToken=" + currentAccessToken;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("ValidateAccessToken", request);
                if (EmergenceUtils.ProcessRequest<ValidateAccessTokenResponse>(request, errorCallback, out var response))
                {
                    success?.Invoke(response.valid);
                }
            }
        }
        
        public void ValidateSignedMessage(string message, string signedMessage, string address,
            ValidateSignedMessageSuccess success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineValidateSignedMessage(message, signedMessage, address, success, errorCallback));
        }

        private IEnumerator CoroutineValidateSignedMessage(string message, string signedMessage, string address,
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

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";
                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("ValidateSignedMessage", request);
                if (EmergenceUtils.ProcessRequest<ValidateSignedMessageResponse>(request, errorCallback, out var response))
                {
                    success?.Invoke(response.valid);
                }
            }
        }
        
        public void Disconnect(DisconnectSuccess success, ErrorCallback errorCallback)
        {
            disconnectInProgress = true;
            StartCoroutine(CoroutineDisconnect(success, errorCallback));
        }

        private IEnumerator CoroutineDisconnect(DisconnectSuccess success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "killSession";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
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
        }
        
        public void Finish(SuccessFinish success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineFinish(success, errorCallback));
        }

        private IEnumerator CoroutineFinish(SuccessFinish success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "finish";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
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
}
