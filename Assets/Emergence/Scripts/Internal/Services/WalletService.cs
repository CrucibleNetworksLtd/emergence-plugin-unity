using System.Collections;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    public class WalletService : MonoBehaviour, IWalletService
    {
        private string walletAddress = string.Empty;

        public bool HasAddress
        {
            get { return walletAddress != null && walletAddress.Trim() != string.Empty; }
        }

        public string WalletAddress
        {
            get { return walletAddress; }
            set { walletAddress = value; }
        }
        
        public void ReinitializeWalletConnect(ReinitializeWalletConnectSuccess success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineReinitializeWalletConnect(success, errorCallback));
        }

        private IEnumerator CoroutineReinitializeWalletConnect(ReinitializeWalletConnectSuccess success,
            ErrorCallback errorCallback)
        {
            Debug.Log("CoroutineReinitializeWalletConnect request started");

            string url = EmergenceSingleton.Instance.Configuration.APIBase + "reinitializewalletconnect";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                EmergenceServices.PrintRequestResult("ReinitializeWalletConnect", request);
                if (EmergenceServices.ProcessRequest<ReinitializeWalletConnectResponse>(request, errorCallback, out var response))
                {
                    success?.Invoke(response.disconnected);
                }
            }
        }

        public void RequestToSign(string messageToSign, RequestToSignSuccess success, ErrorCallback errorCallback)
        {

            StartCoroutine(CoroutineRequestToSignWalletConnect(messageToSign, success, errorCallback));
        }

        private IEnumerator CoroutineRequestToSignWalletConnect(string messageToSign, RequestToSignSuccess success,
            ErrorCallback errorCallback)
        {
            Debug.Log("CoroutineRequestToSignWalletConnect request started");
            var content = "{\"message\": \"" + messageToSign + "\"}";
            
            Debug.Log("content: " + content);

            string url = EmergenceSingleton.Instance.Configuration.APIBase + "request-to-sign";
            
            Debug.Log("url: " + url);
            
            Debug.Log("deviceId: " + EmergenceSingleton.Instance.CurrentDeviceId);

            using (UnityWebRequest request = UnityWebRequest.Post(url, ""))
            {
                request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(content));
                request.uploadHandler.contentType = "application/json";
                request.SetRequestHeader("accept", "application/json");

                yield return request.SendWebRequest();
                EmergenceServices.PrintRequestResult("RequestToSignWalletConnect", request);
                if (EmergenceServices.ProcessRequest<BaseResponse<string>>(request, errorCallback, out var response))
                {
                    success?.Invoke(response.message);
                }
            }
        }
        
        public void Handshake(HandshakeSuccess success, ErrorCallback errorCallback)
        {

            StartCoroutine(CoroutineHandshake(success, errorCallback));
        }

        private IEnumerator CoroutineHandshake(HandshakeSuccess success, ErrorCallback errorCallback)
        {
            Debug.Log("Handshake request started");
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "handshake" + "?nodeUrl=" +
                         EmergenceSingleton.Instance.Configuration.Chain.DefaultNodeURL;
            Debug.Log("Handshake: " + url);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
                yield return request.SendWebRequest();
                EmergenceServices.PrintRequestResult("Handshake", request);
                if (EmergenceServices.ProcessRequest<HandshakeResponse>(request, errorCallback, out var response))
                {
                    WalletAddress = response.address;
                    EmergenceSingleton.Instance.SetCachedAddress(response.address);
                    success?.Invoke(WalletAddress);
                }
            }
        }
        
        public void CreateWallet(string path, string password, CreateWalletSuccess success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineCreateWallet(path, password, success, errorCallback));
        }

        private IEnumerator CoroutineCreateWallet(string path, string password, CreateWalletSuccess success,
            ErrorCallback errorCallback)
        {
            Debug.Log("CreateWallet request started");

            string url = EmergenceSingleton.Instance.Configuration.APIBase + "createWallet" + "?path=" + path +
                         "&password=" + password;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";

                yield return request.SendWebRequest();
                EmergenceServices.PrintRequestResult("Create Wallet", request);
                if (EmergenceServices.ProcessRequest<string>(request, errorCallback, out var response))
                {
                    success?.Invoke();
                }
            }
        }
        
        public void GetBalance(BalanceSuccess success, ErrorCallback errorCallback)
        {
            if (EmergenceServices.Instance.DisconnectInProgress)
                return;
            StartCoroutine(CoroutineGetBalance(WalletAddress, success, errorCallback));
        }

        private IEnumerator CoroutineGetBalance(string address, BalanceSuccess success, ErrorCallback errorCallback)
        {
            Debug.Log("Get Balance request started");

            string url = EmergenceSingleton.Instance.Configuration.APIBase + "getbalance" + "?nodeUrl=" +
                         EmergenceSingleton.Instance.Configuration.Chain.DefaultNodeURL + "&address=" + address;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                EmergenceServices.PrintRequestResult("Get Balance", request);
                if (EmergenceServices.ProcessRequest<GetBalanceResponse>(request, errorCallback, out var response))
                {
                    success?.Invoke(response.balance);
                }
            }
        }
    }
}