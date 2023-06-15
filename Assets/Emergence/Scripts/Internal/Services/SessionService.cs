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
        }
        
        public void ProcessExpiration(string expirationMessage)
        {
            Expiration = SerializationHelper.Deserialize<Expiration>(expirationMessage);
        }

        public async UniTask IsConnected(IsConnectedSuccess success, ErrorCallback errorCallback)
        {
            string url = StaticConfig.APIBase + "isConnected";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
            try
            {
                await request.SendWebRequest().ToUniTask();
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.Message, e.HResult);
            }
            
            EmergenceUtils.PrintRequestResult("IsConnected", request);
            if (EmergenceUtils.ProcessRequest<IsConnectedResponse>(request, errorCallback, out var response))
            {
                success?.Invoke(response.isConnected);
            }
        }

        public async UniTask Disconnect(DisconnectSuccess success, ErrorCallback errorCallback)
        {
            disconnectInProgress = true;
            string url = StaticConfig.APIBase + "killSession";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
            request.SetRequestHeader("auth", personaService.CurrentAccessToken);
            try
            {
                await request.SendWebRequest().ToUniTask();
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
        }
        
        public async UniTask GetQRCode(QRCodeSuccess success, ErrorCallback errorCallback)
        {
            string url = StaticConfig.APIBase + "qrcode";

            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            try
            {
                await request.SendWebRequest().ToUniTask();
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
        }
    }
}
