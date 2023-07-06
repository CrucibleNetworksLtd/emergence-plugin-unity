using System;
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
        
        private bool completedHandshake = false;

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
            string url = StaticConfig.APIBase + "reinitializewalletconnect";

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url);
            try
            {
                var response  = await WebRequestService.PerformAsyncWebRequest(request, errorCallback);
                if(response.IsSuccess == false)
                    return;
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.Message, e.HResult);
            }
            EmergenceUtils.PrintRequestResult("ReinitializeWalletConnect", request);
        
            if (EmergenceUtils.ProcessRequest<ReinitializeWalletConnectResponse>(request, errorCallback, out var processedResponse))
            {
                success?.Invoke(processedResponse.disconnected);
            }
            WebRequestService.CleanupRequest(request);
        }
        
        public async UniTask RequestToSign(string messageToSign, RequestToSignSuccess success, ErrorCallback errorCallback)
        {
            var content = "{\"message\": \"" + messageToSign + "\"}";

            string url = StaticConfig.APIBase + "request-to-sign";

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbPOST, url, content);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
        
            try
            {
                var response  = await WebRequestService.PerformAsyncWebRequest(request, errorCallback);
                if(response.IsSuccess == false)
                {
                    WebRequestService.CleanupRequest(request);
                    return;
                }
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.Message, e.HResult);
            }
            EmergenceUtils.PrintRequestResult("RequestToSignWalletConnect", request);
        
            if (EmergenceUtils.ProcessRequest<WalletSignMessage>(request, errorCallback, out var processedResponse))
            {
                success?.Invoke(processedResponse.signedMessage);
            }
            WebRequestService.CleanupRequest(request);
        }
        
        public async UniTask Handshake(HandshakeSuccess success, ErrorCallback errorCallback)
        {
            string url = StaticConfig.APIBase + "handshake" + "?nodeUrl=" +
                         EmergenceSingleton.Instance.Configuration.Chain.DefaultNodeURL;

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
        
            try
            {
                var response  = await WebRequestService.PerformAsyncWebRequest(request, errorCallback);
                if(response.IsSuccess == false)
                {
                    WebRequestService.CleanupRequest(request);
                    return;
                }
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.Message, e.HResult);
            }
        
            EmergenceUtils.PrintRequestResult("Handshake", request);
        
            if (EmergenceUtils.ProcessRequest<HandshakeResponse>(request, errorCallback, out var processedResponse))
            {
                if (processedResponse == null)
                {
                    string errorMessage = completedHandshake ? "Handshake already completed." : "Handshake failed, check server status.";
                    int errorCode = completedHandshake ? 0 : -1;
                    errorCallback?.Invoke(errorMessage, errorCode);
                    WebRequestService.CleanupRequest(request);
                    return;
                }
                
                completedHandshake = true;
                WalletAddress = processedResponse.address;
                EmergenceSingleton.Instance.SetCachedAddress(processedResponse.address);
                success?.Invoke(WalletAddress);
            }
            WebRequestService.CleanupRequest(request);
        }
        
        public async UniTask GetBalance(BalanceSuccess success, ErrorCallback errorCallback)
        {
            if (sessionService.DisconnectInProgress)
                return;
    
            string url = StaticConfig.APIBase + "getbalance" + "?nodeUrl=" +
                         EmergenceSingleton.Instance.Configuration.Chain.DefaultNodeURL + "&address=" + WalletAddress;

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url);
            try
            {
                var response  = await WebRequestService.PerformAsyncWebRequest(request, errorCallback);
                if(response.IsSuccess == false)
                {
                    WebRequestService.CleanupRequest(request);
                    return;
                }
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.Message, e.HResult);
            }
        
            EmergenceUtils.PrintRequestResult("Get Balance", request);
        
            if (EmergenceUtils.ProcessRequest<GetBalanceResponse>(request, errorCallback, out var processedResponse))
            {
                success?.Invoke(processedResponse.balance);
            }
            WebRequestService.CleanupRequest(request);
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

            string url = StaticConfig.APIBase + "validate-signed-message" + "?request=" + personaService.CurrentAccessToken;

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbPOST, url, dataString);
            try
            {
                var response  = await WebRequestService.PerformAsyncWebRequest(request, errorCallback);
                if(response.IsSuccess == false)
                {
                    WebRequestService.CleanupRequest(request);
                    return;
                }
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.Message, e.HResult);
            }
            EmergenceUtils.PrintRequestResult("ValidateSignedMessage", request);
            if (EmergenceUtils.ProcessRequest<ValidateSignedMessageResponse>(request, errorCallback, out var processedResponse))
            {
                success?.Invoke(processedResponse.valid);
            }
            WebRequestService.CleanupRequest(request);
        }
        
    }
}