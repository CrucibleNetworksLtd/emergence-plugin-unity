using System;
using System.Collections.Generic;
using System.Threading;
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
    internal class SessionService : ISessionService, ISessionServiceInternal, ISessionConnectableService
    {
        public bool IsLoggedIn { get; private set; }
        public LoginSettings? CurrentLoginSettings { get; private set; }
        public event Action OnSessionConnected;
        public event Action OnSessionDisconnected;
        public string EmergenceAccessToken { get; private set; } = string.Empty;
        public bool DisconnectInProgress { get; private set; }

        public SessionService()
        {
            EmergenceSingleton.Instance.OnGameClosing += OnGameEnd;
        }
        
        public bool HasLoginSettings(LoginSettings loginSettings)
        {
            return CurrentLoginSettings != null && (CurrentLoginSettings & loginSettings) == loginSettings;
        }

        private async void OnGameEnd() => await DisconnectAsync();

        public async UniTask<ServiceResponse<IsConnectedResponse>> IsConnected()
        {
            var url = StaticConfig.APIBase + "isConnected";

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if(response.Successful == false)
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
            if (HasLoginSettings(LoginSettings.DisableEmergenceAccessToken)) { return new ServiceResponse(true); }
            
            DisconnectInProgress = true;
            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, StaticConfig.APIBase + "killSession");
            try
            {
                request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
                request.SetRequestHeader("auth", EmergenceAccessToken);
                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if (response.Successful == false)
                {
                    WebRequestService.CleanupRequest(request);
                    return new ServiceResponse(false);
                }

                EmergenceUtils.PrintRequestResult("Disconnect request completed", request);

                if (EmergenceUtils.RequestError(request))
                {
                    DisconnectInProgress = false;
                    WebRequestService.CleanupRequest(request);
                    return new ServiceResponse(false);
                }

                RunDisconnectionEvents();

                return new ServiceResponse(true);
            }
            catch (ArgumentException)
            {
                // Already disconnected
                return new ServiceResponse(true);
            }
            catch (Exception)
            {
                return new ServiceResponse(false);
            }
            finally
            {
                WebRequestService.CleanupRequest(request);
                DisconnectInProgress = false;
            }
        }

        public void RunConnectionEvents(LoginSettings loginSettings)
        {
            IsLoggedIn = true;
            CurrentLoginSettings = loginSettings;
   
            foreach (var connectable in EmergenceServiceProvider.GetServices<ISessionConnectableService>())
            {
                connectable.HandleConnection(this);
            }
            OnSessionConnected?.Invoke();
        }

        public void RunDisconnectionEvents()
        {
            foreach (var connectable in EmergenceServiceProvider.GetServices<ISessionConnectableService>())
            {
                connectable.HandleDisconnection(this);
            }
            OnSessionDisconnected?.Invoke();

            IsLoggedIn = false;
            CurrentLoginSettings = null;
        }

        public async UniTask Disconnect(DisconnectSuccess success, ErrorCallback errorCallback)
        {
            var response = await DisconnectAsync();
            if(response.Successful)
                success?.Invoke();
            else
                errorCallback?.Invoke("Error in Disconnect.", (long)response.Code);
        }

        public async UniTask<ServiceResponse<Texture2D>> GetQrCodeAsync(CancellationToken ct)
        {
            string url = StaticConfig.APIBase + "qrcode";

            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError, ct: ct);
                if (!response.Successful)
                {
                    WebRequestService.CleanupRequest(request);
                    return new ServiceResponse<Texture2D>(false);
                }
            }
            catch (Exception e) when (e is not OperationCanceledException) 
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

        public async UniTask GetQrCode(QRCodeSuccess success, ErrorCallback errorCallback, CancellationCallback cancellationCallback, CancellationToken ct)
        {
            try
            {
                var response = await GetQrCodeAsync(ct);
                if (response.Successful)
                    success?.Invoke(response.Result1);
                else
                    errorCallback?.Invoke("Error in GetQRCode.", (long)response.Code);
            }
            catch (OperationCanceledException)
            {
                cancellationCallback?.Invoke();
            }
        }
        
        public async UniTask<ServiceResponse<string>> GetAccessTokenAsync()
        {
            string url = StaticConfig.APIBase + "get-access-token";
            var headers = new Dictionary<string, string> { { "deviceId", EmergenceSingleton.Instance.CurrentDeviceId } };
            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, url, EmergenceLogger.LogError, "", headers);
            if(response.Successful == false)
                return new ServiceResponse<string>(false);
            var accessTokenResponse = SerializationHelper.Deserialize<BaseResponse<AccessTokenResponse>>(response.ResponseText);
            EmergenceAccessToken = SerializationHelper.Serialize(accessTokenResponse.message.AccessToken, false);
            return new ServiceResponse<string>(true, EmergenceAccessToken);
        }

        public void HandleDisconnection(ISessionService sessionService)
        {
            EmergenceAccessToken = "";
        }
        
        public void HandleConnection(ISessionService sessionService) { }

        public async UniTask GetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback)
        {
            var response = await GetAccessTokenAsync();
            if(response.Successful)
                success?.Invoke(response.Result1);
            else
                errorCallback?.Invoke("Error in GetAccessToken.", (long)response.Code);
        }
    }
}
