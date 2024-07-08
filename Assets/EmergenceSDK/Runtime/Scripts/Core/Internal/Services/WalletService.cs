#if DEVELOPMENT_BUILD || UNITY_EDITOR
#define INCLUDE_DEVELOPMENT_INTERFACES
#endif

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Internal.Types;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Delegates;
using EmergenceSDK.Runtime.Types.Responses;
using EmergenceSDK.Runtime.Utilities;

namespace EmergenceSDK.Runtime.Internal.Services
{
#if INCLUDE_DEVELOPMENT_INTERFACES
    internal class WalletService : IWalletService, IWalletServiceDevelopmentOnly, IWalletServiceInternal
#else
    internal class WalletService : IWalletService, IWalletServiceInternal
#endif
    {
        public bool IsValidWallet => !string.IsNullOrEmpty(WalletAddress?.Trim()) && !string.IsNullOrEmpty(ChecksummedWalletAddress?.Trim());
        public string WalletAddress { get; private set; } = string.Empty;
        public string ChecksummedWalletAddress { get; private set; } = string.Empty;
        private readonly ISessionServiceInternal sessionServiceInternal;
        private bool completedHandshake;

        public WalletService(ISessionServiceInternal sessionServiceInternal)
        {
            this.sessionServiceInternal = sessionServiceInternal;
        }

        public async UniTask<ServiceResponse<bool>> ReinitializeWalletConnect()
        {
            string url = StaticConfig.APIBase + "reinitializewalletconnect";

            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url);
            if(!response.Successful)
            {
                return new ServiceResponse<bool>(response, false, false);
            }

            var requestSuccessful = EmergenceUtils.ProcessResponse<ReinitializeWalletConnectResponse>(response, EmergenceLogger.LogError, out var processedResponse);
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
            
            //TODO spoof a response for tests here
            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Post, url, content, EmergenceSingleton.DeviceIdHeader);
            if(!response.Successful)
            {
                return new ServiceResponse<string>(response, false);
            }

            var requestSuccessful = EmergenceUtils.ProcessResponse<WalletSignMessage>(response, EmergenceLogger.LogError, out var processedResponse);
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

            //TODO need to spoof something here in support of testing.
            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url, headers: EmergenceSingleton.DeviceIdHeader, timeout: timeout, ct: ct);
                
            if (!response.Successful)
            {
                if (response is FailedWebResponse failedWebResponse)
                {
                    throw failedWebResponse.Exception;
                }

                return new ServiceResponse<string>(response, false);
            }

            if (EmergenceUtils.ProcessResponse<HandshakeResponse>(response, EmergenceLogger.LogError, out var processedResponse))
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
            
            // TODO once again spoof our response here, this time for balance
            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url);
            if(!response.Successful)
            {
                return new ServiceResponse<string>(response, false);
            }

            if (EmergenceUtils.ProcessResponse<GetBalanceResponse>(response, EmergenceLogger.LogError, out var processedResponse))
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
                if(!response.Successful)
                {
                    return new ServiceResponse<bool>(false);
                }
                
                if (EmergenceUtils.ProcessResponse<ValidateSignedMessageResponse>(response, EmergenceLogger.LogError, out var processedResponse))
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
        
#if INCLUDE_DEVELOPMENT_INTERFACES
        //TODO, this old spoofing stuff needs to be scrapped. Beter to spoof responses to toest full capability.
        public IDisposable SpoofedWallet(string wallet, string checksummedWallet) => new SpoofedWalletManager(wallet, checksummedWallet);

        public void RunWithSpoofedWalletAddress(string walletAddress, string checksummedWalletAddress, Action action)
        {
            using (SpoofedWallet(walletAddress, checksummedWalletAddress))
            {
                action.Invoke();
            }
        }

        public async UniTask RunWithSpoofedWalletAddressAsync(string walletAddress, string checksummedWalletAddress, Func<UniTask> action)
        {
            using (SpoofedWallet(walletAddress, checksummedWalletAddress))
            {
                await action();
            }
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
#endif
    }
}