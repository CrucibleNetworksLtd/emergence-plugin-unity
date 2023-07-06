using System;
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
        
        public bool DisconnectInProgress => disconnectInProgress;
        public event Action OnSessionDisconnected;
        private bool disconnectInProgress = false;
        
        public Expiration Expiration { get; private set; }

        private IPersonaService personaService;
        
        public SessionService(IPersonaService personaService)
        {
            this.personaService = personaService;
            
            if(personaService is PersonaService personaServiceInstance)
                OnSessionDisconnected += () => personaServiceInstance.OnSessionDisconnected();

            EmergenceSingleton.Instance.OnGameClosing += OnGameEnd;
        }

        private async void OnGameEnd() => await Disconnect(null, null);

        public void ProcessExpiration(string expirationMessage)
        {
            Expiration = SerializationHelper.Deserialize<Expiration>(expirationMessage);
        }

        public async UniTask IsConnected(IsConnectedSuccess success, ErrorCallback errorCallback)
        {
            string url = StaticConfig.APIBase + "isConnected";

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, errorCallback);
                if(response.IsSuccess == false)
                    return;
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.Message, e.HResult);
            }
            
            EmergenceUtils.PrintRequestResult("IsConnected", request);
            if (EmergenceUtils.ProcessRequest<IsConnectedResponse>(request, errorCallback, out var processedResponse))
            {
                success?.Invoke(processedResponse.isConnected);
            }
            WebRequestService.CleanupRequest(request);
        }

        public async UniTask Disconnect(DisconnectSuccess success, ErrorCallback errorCallback)
        {
            disconnectInProgress = true;
            string url = StaticConfig.APIBase + "killSession";

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
            request.SetRequestHeader("auth", personaService.CurrentAccessToken);
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, errorCallback);
                if(response.IsSuccess == false)
                    return;
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.Message, e.HResult);
            }
            EmergenceUtils.PrintRequestResult("Disconnect request completed", request);

            if (EmergenceUtils.RequestError(request))
            {
                disconnectInProgress = false;
                errorCallback?.Invoke(request.error, request.responseCode);
            }
            else
            {
                disconnectInProgress = false;
                OnSessionDisconnected?.Invoke();
                success?.Invoke();
            }
            WebRequestService.CleanupRequest(request);
        }
        
        public async UniTask GetQRCode(QRCodeSuccess success, ErrorCallback errorCallback)
        {
            string url = StaticConfig.APIBase + "qrcode";

            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, errorCallback);
                if(response.IsSuccess == false)
                    return;
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.Message, e.HResult);
            }

            EmergenceUtils.PrintRequestResult("GetQrCode", request);

            if (EmergenceUtils.RequestError(request))
            {
                errorCallback?.Invoke(request.error, request.responseCode);
            }
            else
            {
                string deviceId = request.GetResponseHeader("deviceId");
                EmergenceSingleton.Instance.CurrentDeviceId = deviceId;
                success?.Invoke((request.downloadHandler as DownloadHandlerTexture).texture);
            }
            WebRequestService.CleanupRequest(request);
        }
    }
}
