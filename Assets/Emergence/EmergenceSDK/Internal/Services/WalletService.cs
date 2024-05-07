using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Implementations.Login;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Internal.Types;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class WalletService : IWalletService, IWalletServiceInternal
    {
        private bool completedHandshake = false;

        public bool IsValidWallet => WalletAddress != null && WalletAddress.Trim() != string.Empty &&
                                  ChecksummedWalletAddress != null && ChecksummedWalletAddress.Trim() != string.Empty;

        public string WalletAddress { get; private set; } = string.Empty;
        public string ChecksummedWalletAddress { get; private set; } = string.Empty;

        private ISessionServiceInternal sessionServiceInternal;

        public WalletService(ISessionServiceInternal sessionServiceInternal)
        {
            this.sessionServiceInternal = sessionServiceInternal;
        }

        public async UniTask<ServiceResponse<bool>> ReinitializeWalletConnect()
        {
            string url = StaticConfig.APIBase + "reinitializewalletconnect";

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url);
            var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
            if(response.Successful == false)
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse<bool>(response, false, false);
            }
            EmergenceUtils.PrintRequestResult("ReinitializeWalletConnect", request);
        
            var requestSuccessful = EmergenceUtils.ProcessRequest<ReinitializeWalletConnectResponse>(request, EmergenceLogger.LogError, out var processedResponse);
            WebRequestService.CleanupRequest(request);
            if (requestSuccessful)
            {
                return new ServiceResponse<bool>(response, true, processedResponse.disconnected);
            }
            return new ServiceResponse<bool>(response, false);
        }
        
        public async UniTask<ServiceResponse<string>> RequestToSignAsync(string messageToSign)
        {
            var content = SerializationHelper.Serialize(
                new
                {
                    message = messageToSign
                }
            );

            string url = StaticConfig.APIBase + "request-to-sign";

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbPOST, url, content);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
        
            var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
            if(response.Successful == false)
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse<string>(response, false);
            }
            EmergenceUtils.PrintRequestResult("RequestToSignWalletConnect", request);

            try
            {
                var requestSuccessful = EmergenceUtils.ProcessRequest<WalletSignMessage>(request, EmergenceLogger.LogError, out var processedResponse);
                if (requestSuccessful)
                {
                    if (processedResponse == null)
                    {
                        EmergenceLogger.LogWarning("Request was successful but processedResponse was null, response body was: `" + request.downloadHandler.text + "`");
                        return new ServiceResponse<string>(response, false);
                    }
                    return new ServiceResponse<string>(response, true, processedResponse.signedMessage);
                }
                return new ServiceResponse<string>(response, false);
            }
            finally
            {
                WebRequestService.CleanupRequest(request);
            }
        }

        public async UniTask RequestToSign(string messageToSign, RequestToSignSuccess success, ErrorCallback errorCallback)
        {
            var response = await RequestToSignAsync(messageToSign);
            if(response.Successful)
                success?.Invoke(response.Result1);
            else
                errorCallback?.Invoke("Error in RequestToSign.", (long)response.Code);
        }

        public async UniTask<ServiceResponse<string>> HandshakeAsync(float timeout, CancellationToken ct)
        {
            string url = StaticConfig.APIBase + "handshake" + "?nodeUrl=" +
                         EmergenceSingleton.Instance.Configuration.Chain.DefaultNodeURL;

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);

            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError, timeout, ct: ct);
                if (!response.Successful)
                {
                    if (response is FailedWebResponse failedWebResponse)
                    {
                        throw failedWebResponse.Exception;
                    }

                    return new ServiceResponse<string>(response, false);
                }
                
                EmergenceUtils.PrintRequestResult("Handshake", request);
        
                if (EmergenceUtils.ProcessRequest<HandshakeResponse>(request, EmergenceLogger.LogError, out var processedResponse))
                {
                    if (processedResponse == null)
                    {
                        string errorMessage = completedHandshake ? "Handshake already completed." : "Handshake failed, check server status.";
                        int errorCode = completedHandshake ? 0 : -1;
                        EmergenceLogger.LogError(errorMessage, errorCode);
                        WebRequestService.CleanupRequest(request);
                        return new ServiceResponse<string>(response, false);
                    }
                
                    completedHandshake = true;
                    WalletAddress = processedResponse.address;
                    ChecksummedWalletAddress = processedResponse.checksummedAddress;
                    WebRequestService.CleanupRequest(request);
                    return new ServiceResponse<string>(response, true, processedResponse.address);
                }
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse<string>(response, false);
            }
            finally
            {
                WebRequestService.CleanupRequest(request);
            }
        }

        public void RunWithSpoofedWalletAddress(string walletAddress, string checksummedWalletAddress, Action action)
        {
            var oldWalletAddress = WalletAddress;
            var oldChecksummedWalletAddress = ChecksummedWalletAddress;
            WalletAddress = walletAddress;
            ChecksummedWalletAddress = checksummedWalletAddress;

            try
            {
                action.Invoke();
            }
            finally
            {
                WalletAddress = oldWalletAddress;
                ChecksummedWalletAddress = oldChecksummedWalletAddress;
            }
        }

        public async UniTask RunWithSpoofedWalletAddressAsync(string walletAddress, string checksummedWalletAddress, Func<UniTask> action)
        {
            var oldWalletAddress = WalletAddress;
            var oldChecksummedWalletAddress = ChecksummedWalletAddress;
            WalletAddress = walletAddress;
            ChecksummedWalletAddress = checksummedWalletAddress;

            try
            {
                await action();
            }
            finally
            {
                WalletAddress = oldWalletAddress;
                ChecksummedWalletAddress = oldChecksummedWalletAddress;
            }
        }

        public async UniTask Handshake(HandshakeSuccess success, ErrorCallback errorCallback, float timeout, CancellationCallback cancellationCallback,
            CancellationToken ct = default)
        {
            try
            {
                var response = await HandshakeAsync(timeout, ct);
                if (response.Successful)
                    success?.Invoke(response.Result1);
                else
                    errorCallback?.Invoke("Error in Handshake.", (long)response.Code);
            }
            catch (OperationCanceledException)
            {
                cancellationCallback?.Invoke();
            }
            catch (TimeoutException)
            {
                errorCallback?.Invoke("Handshake timed out.", (long)ServiceResponseCode.Failure);
            }
        }

        public async UniTask<ServiceResponse<string>> GetBalanceAsync()
        {
            if (((ISessionService)sessionServiceInternal).DisconnectInProgress)
                return new ServiceResponse<string>(false);
    
            string url = StaticConfig.APIBase + "getbalance" + 
                         "?nodeUrl=" + EmergenceSingleton.Instance.Configuration.Chain.DefaultNodeURL +
                         "&address=" + WalletAddress;

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url);
            
            var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
            if(response.Successful == false)
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse<string>(response, false);
            }
        
            EmergenceUtils.PrintRequestResult("Get Balance", request);
        
            if (EmergenceUtils.ProcessRequest<GetBalanceResponse>(request, EmergenceLogger.LogError, out var processedResponse))
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse<string>(response, true, processedResponse.balance);
            }
            WebRequestService.CleanupRequest(request);
            return new ServiceResponse<string>(response, false);
        }

        public async UniTask GetBalance(BalanceSuccess success, ErrorCallback errorCallback)
        {
            var response = await GetBalanceAsync();
            if(response.Successful)
                success?.Invoke(response.Result1);
            else
                errorCallback?.Invoke("Error in GetBalance.", (long)response.Code);
        }

        public UniTask<ServiceResponse<bool>> ValidateSignedMessageAsync(string message, string signedMessage, string address)
        {
            return ValidateSignedMessageAsync(new ValidateSignedMessageRequest(message, signedMessage, address));
        }

        public async UniTask<ServiceResponse<bool>> ValidateSignedMessageAsync(ValidateSignedMessageRequest data)
        {
            string dataString = SerializationHelper.Serialize(data, false);

            string url = StaticConfig.APIBase + "validate-signed-message" + "?request=" + sessionServiceInternal.CurrentAccessToken;

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbPOST, url, dataString);
            try
            {
                var response  = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if(response.Successful == false)
                {
                    WebRequestService.CleanupRequest(request);
                    return new ServiceResponse<bool>(false);
                }
            }
            catch (Exception)
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse<bool>(false);
            }
            EmergenceUtils.PrintRequestResult("ValidateSignedMessage", request);
            if (EmergenceUtils.ProcessRequest<ValidateSignedMessageResponse>(request, EmergenceLogger.LogError, out var processedResponse))
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse<bool>(true, processedResponse.valid);
            }
            WebRequestService.CleanupRequest(request);
            return new ServiceResponse<bool>(false);
        }

        public async UniTask ValidateSignedMessage(string message, string signedMessage, string address,
            ValidateSignedMessageSuccess success, ErrorCallback errorCallback)
        {
            var response = await ValidateSignedMessageAsync(new ValidateSignedMessageRequest(message, signedMessage, address));
            if(response.Successful)
                success?.Invoke(response.Result1);
            else
                errorCallback?.Invoke("Error in ValidateSignedMessage.", (long)response.Code);
        }
    }
}