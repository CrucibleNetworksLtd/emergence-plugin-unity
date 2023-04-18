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
        private bool disconnectInProgress = false;
        
        public Expiration Expiration { get; private set; }

        private IPersonaService personaService;
        
        public SessionService(IPersonaService personaService)
        {
            this.personaService = personaService;
        }
        
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

        public async UniTask Disconnect(DisconnectSuccess success, ErrorCallback errorCallback)
        {
            disconnectInProgress = true;
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "killSession";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
            request.SetRequestHeader("auth", personaService.CurrentAccessToken);
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

        //Local EVM only
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
