using System;
using System.Collections.Generic;
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

            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url);
            if(response.Successful == false)
            {
                return new ServiceResponse<bool>(response, false, false);
            }

            var requestSuccessful = EmergenceUtils.ProcessRequest<ReinitializeWalletConnectResponse>(response.Request, EmergenceLogger.LogError, out var processedResponse);
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
            
            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url, headers: EmergenceSingleton.DeviceIdHeader);
            if(response.Successful == false)
            {
                return new ServiceResponse<string>(response, false);
            }

            var requestSuccessful = EmergenceUtils.ProcessRequest<WalletSignMessage>(response.Request, EmergenceLogger.LogError, out var processedResponse);
            if (requestSuccessful)
            {
                if (processedResponse == null)
                {
                    EmergenceLogger.LogWarning("Request was successful but processedResponse was null, response body was: `" + response.ResponseText + "`");
                    return new ServiceResponse<string>(response, false);
                }
                return new ServiceResponse<string>(response, true, processedResponse.signedMessage);
            }
            return new ServiceResponse<string>(response, false);
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
            var url = StaticConfig.APIBase + "handshake" + "?nodeUrl=" + EmergenceSingleton.Instance.Configuration.Chain.DefaultNodeURL;
            
            try
            {
                var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url, headers: EmergenceSingleton.DeviceIdHeader, timeout: timeout, ct: ct);
                
                if (!response.Successful)
                {
                    if (response is FailedWebResponse failedWebResponse)
                    {
                        throw failedWebResponse.Exception;
                    }

                    return new ServiceResponse<string>(response, false);
                }

                if (EmergenceUtils.ProcessRequest<HandshakeResponse>(response.Request, EmergenceLogger.LogError, out var processedResponse))
                {
                    if (processedResponse == null)
                    {
                        string errorMessage = completedHandshake ? "Handshake already completed." : "Handshake failed, check server status.";
                        int errorCode = completedHandshake ? 0 : -1;
                        EmergenceLogger.LogError(errorMessage, errorCode);
                        return new ServiceResponse<string>(response, false);
                    }
                
                    completedHandshake = true;
                    WalletAddress = processedResponse.address;
                    ChecksummedWalletAddress = processedResponse.checksummedAddress;
                    return new ServiceResponse<string>(response, true, processedResponse.address);
                }

                return new ServiceResponse<string>(response, false);
            }
            finally
            {
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
            
            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url);
            if(response.Successful == false)
            {
                return new ServiceResponse<string>(response, false);
            }

            if (EmergenceUtils.ProcessRequest<GetBalanceResponse>(response.Request, EmergenceLogger.LogError, out var processedResponse))
            {
                return new ServiceResponse<string>(response, true, processedResponse.balance);
            }

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

            string url = StaticConfig.APIBase + "validate-signed-message" + "?request=" + sessionServiceInternal.EmergenceAccessToken;

            try
            {
                var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Post, url, dataString);
                if(response.Successful == false)
                {
                    return new ServiceResponse<bool>(false);
                }
                
                if (EmergenceUtils.ProcessRequest<ValidateSignedMessageResponse>(response.Request, EmergenceLogger.LogError, out var processedResponse))
                {
                    return new ServiceResponse<bool>(true, processedResponse.valid);
                }

                return new ServiceResponse<bool>(false);
            }
            catch (Exception)
            {
                return new ServiceResponse<bool>(false);
            }
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