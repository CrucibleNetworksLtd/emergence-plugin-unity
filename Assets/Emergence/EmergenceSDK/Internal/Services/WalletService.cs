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
        public IDisposable SpoofedWallet(string wallet, string checksummedWallet) => new SpoofedWalletManager(wallet, checksummedWallet);
        public bool IsValidWallet => !string.IsNullOrEmpty(WalletAddress?.Trim()) && !string.IsNullOrEmpty(ChecksummedWalletAddress?.Trim());
        public string WalletAddress { get; internal set; } = string.Empty;
        public string ChecksummedWalletAddress { get; internal set; } = string.Empty;
        private readonly ISessionServiceInternal _sessionServiceInternal;
        private bool _completedHandshake;

        public WalletService(ISessionServiceInternal sessionServiceInternal)
        {
            this._sessionServiceInternal = sessionServiceInternal;
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
                    string errorMessage = _completedHandshake ? "Handshake already completed." : "Handshake failed, check server status.";
                    int errorCode = _completedHandshake ? 0 : -1;
                    EmergenceLogger.LogError(errorMessage, errorCode);
                    return new ServiceResponse<string>(response, false);
                }
                
                _completedHandshake = true;
                WalletAddress = processedResponse.address;
                ChecksummedWalletAddress = processedResponse.checksummedAddress;
                return new ServiceResponse<string>(response, true, processedResponse.address);
            }

            return new ServiceResponse<string>(response, false);
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
            if (((ISessionService)_sessionServiceInternal).DisconnectInProgress)
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

            string url = StaticConfig.APIBase + "validate-signed-message" + "?request=" + _sessionServiceInternal.EmergenceAccessToken;

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
        
        private class SpoofedWalletManager : FlagLifecycleManager<string, string>
        {
            public SpoofedWalletManager(string walletAddress, string checksummedWalletAddress) : base(walletAddress, checksummedWalletAddress) {}

            // These are virtual and called in the constructor, so we can't store a reference to the wallet service since it will cause a NullReferenceException
            // We can directly request a WalletService rather than IWalletService, since we're already in the WalletService class
            // It's actually needed to be able to set WalletAddress and ChecksummedWalletAddress, whose setters aren't exposed to any interface.
            protected override string GetCurrentFlag1Value() => EmergenceServiceProvider.GetService<WalletService>().WalletAddress;
            protected override void SetFlag1Value(string newValue) => EmergenceServiceProvider.GetService<WalletService>().WalletAddress = newValue;
            protected override string GetCurrentFlag2Value() => EmergenceServiceProvider.GetService<WalletService>().ChecksummedWalletAddress;
            protected override void SetFlag2Value(string newValue) => EmergenceServiceProvider.GetService<WalletService>().ChecksummedWalletAddress = newValue;
        }
    }
}