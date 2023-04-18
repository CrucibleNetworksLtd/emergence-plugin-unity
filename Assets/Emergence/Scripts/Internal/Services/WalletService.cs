using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class WalletService : IWalletService
    {
        private string walletAddress = string.Empty;

        public bool HasAddress => walletAddress != null && walletAddress.Trim() != string.Empty;

        public string WalletAddress
        {
            get => walletAddress;
            set => walletAddress = value;
        }

        private IPersonaService personaService;
        private ISessionService sessionService;

        public WalletService(IPersonaService personaService, ISessionService sessionService)
        {
            this.personaService = personaService;
            this.sessionService = sessionService;
        }

        public async UniTask ReinitializeWalletConnect(ReinitializeWalletConnectSuccess success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "reinitializewalletconnect";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest().ToUniTask();
            EmergenceUtils.PrintRequestResult("ReinitializeWalletConnect", request);
        
            if (EmergenceUtils.ProcessRequest<ReinitializeWalletConnectResponse>(request, errorCallback, out var response))
            {
                success?.Invoke(response.disconnected);
            }
        }
        
        public async UniTask RequestToSign(string messageToSign, RequestToSignSuccess success, ErrorCallback errorCallback)
        {
            var content = "{\"message\": \"" + messageToSign + "\"}";

            string url = EmergenceSingleton.Instance.Configuration.APIBase + "request-to-sign";

            using UnityWebRequest request = UnityWebRequest.Post(url, "");
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
            request.method = "POST";
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(content));
            request.uploadHandler.contentType = "application/json";
        
            await request.SendWebRequest().ToUniTask();
            EmergenceUtils.PrintRequestResult("RequestToSignWalletConnect", request);
        
            if (EmergenceUtils.ProcessRequest<WalletSignMessage>(request, errorCallback, out var response))
            {
                success?.Invoke(response.signedMessage);
            }
        }
        
        public async UniTask Handshake(HandshakeSuccess success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "handshake" + "?nodeUrl=" +
                         EmergenceSingleton.Instance.Configuration.Chain.DefaultNodeURL;

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
        
            await request.SendWebRequest().ToUniTask();
        
            EmergenceUtils.PrintRequestResult("Handshake", request);
        
            if (EmergenceUtils.ProcessRequest<HandshakeResponse>(request, errorCallback, out var response))
            {
                WalletAddress = response.address;
                EmergenceSingleton.Instance.SetCachedAddress(response.address);
                success?.Invoke(WalletAddress);
            }
        }
        
        public async UniTask GetBalance(BalanceSuccess success, ErrorCallback errorCallback)
        {
            if (sessionService.DisconnectInProgress)
                return;
    
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "getbalance" + "?nodeUrl=" +
                         EmergenceSingleton.Instance.Configuration.Chain.DefaultNodeURL + "&address=" + WalletAddress;

            using UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest().ToUniTask();
        
            EmergenceUtils.PrintRequestResult("Get Balance", request);
        
            if (EmergenceUtils.ProcessRequest<GetBalanceResponse>(request, errorCallback, out var response))
            {
                success?.Invoke(response.balance);
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

            string url = EmergenceSingleton.Instance.Configuration.APIBase + "validate-signed-message" + "?request=" + personaService.CurrentAccessToken;

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
        
    }
}