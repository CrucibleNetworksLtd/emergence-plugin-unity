using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class SessionEmergenceService : ISessionService, ISessionServiceInternal, IDisconnectableEmergenceService
    {
        public string CurrentAccessToken { get; private set; } = string.Empty;

        public bool DisconnectInProgress => disconnectInProgress;
        public event Action OnSessionDisconnected;
        private bool disconnectInProgress = false;
        
        public SessionEmergenceService()
        {
            OnSessionDisconnected += () =>
            {
                foreach (var disconnectable in EmergenceServiceProvider.GetServices<IDisconnectableEmergenceService>())
                {
                    disconnectable.HandleDisconnection();
                }
            };

            EmergenceSingleton.Instance.OnGameClosing += OnGameEnd;
        }

        private async void OnGameEnd() => await Disconnect(null, null);

        public async UniTask<ServiceResponse<IsConnectedResponse>> IsConnected()
        {
            string url = StaticConfig.APIBase + "isConnected";

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if(response.IsSuccess == false)
                {
                    WebRequestService.CleanupRequest(request);
                    return new ServiceResponse<IsConnectedResponse>(false);
                }
            }
            catch (Exception)
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse<IsConnectedResponse>(false);
            }
            
            EmergenceUtils.PrintRequestResult("IsConnected", request);
            var successfulRequest = EmergenceUtils.ProcessRequest<IsConnectedResponse>(request, EmergenceLogger.LogError, out var processedResponse);
            WebRequestService.CleanupRequest(request);
            if (successfulRequest)
            {
                return new ServiceResponse<IsConnectedResponse>(true, processedResponse);
            }

            return new ServiceResponse<IsConnectedResponse>(false);
        }

        public async UniTask<ServiceResponse> DisconnectAsync()
        {
            disconnectInProgress = true;
            string url = StaticConfig.APIBase + "killSession";
            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url);
            try
            {
                request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
                request.SetRequestHeader("auth", CurrentAccessToken);
                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if (response.IsSuccess == false)
                {
                    WebRequestService.CleanupRequest(request);
                    return new ServiceResponse(false);
                }
            }
            catch (ArgumentException)
            {
                WebRequestService.CleanupRequest(request);
                //Already disconnected
            }
            catch (Exception)
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse(false);
            }
            EmergenceUtils.PrintRequestResult("Disconnect request completed", request);

            if (EmergenceUtils.RequestError(request))
            {
                disconnectInProgress = false;
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse(false);
            }

            disconnectInProgress = false;
            OnSessionDisconnected?.Invoke();
            WebRequestService.CleanupRequest(request);
            return new ServiceResponse(true);
        }

        public async UniTask Disconnect(DisconnectSuccess success, ErrorCallback errorCallback)
        {
            var response = await DisconnectAsync();
            if(response.Success)
                success?.Invoke();
            else
                errorCallback?.Invoke("Error in Disconnect.", (long)response.Code);
        }

        public async UniTask<ServiceResponse<Texture2D>> GetQrCodeAsync()
        {
            string url = StaticConfig.APIBase + "qrcode";

            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if(response.IsSuccess == false)
                {
                    WebRequestService.CleanupRequest(request);
                    return new ServiceResponse<Texture2D>(false);
                }
            }
            catch (Exception)
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse<Texture2D>(false);
            }

            EmergenceUtils.PrintRequestResult("GetQrCode", request);

            if (EmergenceUtils.RequestError(request))
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse<Texture2D>(false);
            }

            string deviceId = request.GetResponseHeader("deviceId");
            EmergenceSingleton.Instance.CurrentDeviceId = deviceId;
            WebRequestService.CleanupRequest(request);
            return new ServiceResponse<Texture2D>(true, ((DownloadHandlerTexture)request.downloadHandler).texture);
        }

        public async UniTask GetQrCode(QRCodeSuccess success, ErrorCallback errorCallback)
        {
            var response = await GetQrCodeAsync();
            if(response.Success)
                success?.Invoke(response.Result);
            else
                errorCallback?.Invoke("Error in GetQRCode.", (long)response.Code);
        }
        
        public async UniTask<ServiceResponse<string>> GetAccessTokenAsync()
        {
            string url = StaticConfig.APIBase + "get-access-token";
            var headers = new Dictionary<string, string> { { "deviceId", EmergenceSingleton.Instance.CurrentDeviceId } };
            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, url, EmergenceLogger.LogError, "", headers);
            if(response.IsSuccess == false)
                return new ServiceResponse<string>(false);
            var accessTokenResponse = SerializationHelper.Deserialize<BaseResponse<AccessTokenResponse>>(response.Response);
            CurrentAccessToken = SerializationHelper.Serialize(accessTokenResponse.message.AccessToken, false);
            return new ServiceResponse<string>(true, CurrentAccessToken);
        }

        public void HandleDisconnection()
        {
            CurrentAccessToken = "";
        }

        public async UniTask GetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback)
        {
            var response = await GetAccessTokenAsync();
            if(response.Success)
                success?.Invoke(response.Result);
            else
                errorCallback?.Invoke("Error in GetAccessToken.", (long)response.Code);
        }

        public bool IsLoggedIn() => ((ISessionServiceInternal)this).CurrentAccessToken.Length != 0;
    }
}
