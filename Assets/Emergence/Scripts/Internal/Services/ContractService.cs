using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.UI;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class ContractService : IContractService
    {
        public event WriteMethodSuccess WriteMethodConfirmed;
        
        private readonly List<string> loadedContractAddresses = new();
        private int desiredConfirmationCount = 1;
        private bool CheckForNewContract(ContractInfo contractInfo) => !loadedContractAddresses.Contains(contractInfo.ContractAddress);

        private const int MaxRetryAttempts = 1;
        
        /// <summary>
        /// Loads the contract if it is new
        /// </summary>
        /// <returns>Returns true if there was an error during loading</returns>
        private async Task<bool> AttemptToLoadContract(ContractInfo contractInfo)
        {
            if (CheckForNewContract(contractInfo))
            {
                bool loadedSuccessfully = await LoadContract(contractInfo.ContractAddress, contractInfo.ABI, contractInfo.Network);
                if (!loadedSuccessfully)
                {
                    EmergenceLogger.LogError("Error loading contract");
                    return false;
                }
            }
            return true;
        }

        private async UniTask<bool> LoadContract(string contractAddress, string ABI, string network)
        {
            Contract data = new Contract()
            {
                contractAddress = contractAddress,
                ABI = ABI,
                network = network,
            };

            string dataString = SerializationHelper.Serialize(data, false);
            string url = StaticConfig.APIBase + "loadContract";

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbPOST, url, dataString);
            request.downloadHandler = new DownloadHandlerBuffer();
            var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);

            if (response.IsSuccess && EmergenceUtils.ProcessRequest<LoadContractResponse>(request, EmergenceLogger.LogError, out var processedResponse))
            {
                loadedContractAddresses.Add(contractAddress);
            }
            WebRequestService.CleanupRequest(request);
            return loadedContractAddresses.Contains(contractAddress);
        }

        public async UniTask<ServiceResponse<ReadContractResponse>> ReadMethodAsync<T>(ContractInfo contractInfo, T body)
        {
            if (!await AttemptToLoadContract(contractInfo)) 
                return new ServiceResponse<ReadContractResponse>(false);
            
            string url = contractInfo.ToReadUrl();
            string dataString = SerializationHelper.Serialize(body, false);

            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbPOST, url, EmergenceLogger.LogError, dataString);
            if(response.IsSuccess == false)
                return new ServiceResponse<ReadContractResponse>(false);
            var readContractResponse = SerializationHelper.Deserialize<BaseResponse<ReadContractResponse>>(response.Response);
            return new ServiceResponse<ReadContractResponse>(true, readContractResponse.message);
        }
        
        public async UniTask ReadMethod<T>(ContractInfo contractInfo, T body, ReadMethodSuccess success, ErrorCallback errorCallback)
        {
            var response = await ReadMethodAsync(contractInfo, body);
            if(response.Success)
                success?.Invoke(response.Result);
            else
                errorCallback?.Invoke("Error in ReadMethod", (long)response.Code);
        }

        public async UniTask<ServiceResponse<WriteContractResponse>> WriteMethodAsync<T>(ContractInfo contractInfo,
            string localAccountNameIn, string gasPriceIn, string value, T body)
        {
            return await WriteMethodAsyncImpl<T>(contractInfo, localAccountNameIn, gasPriceIn, value, body, 0);
        }
        
        private async UniTask<ServiceResponse<WriteContractResponse>> WriteMethodAsyncRetry<T>(SerialisedWriteRequest<T> request)
        {
            if (request.Attempt <= MaxRetryAttempts)
                return await WriteMethodAsyncImpl(request.ContractInfo, request.LocalAccountNameIn, request.GasPriceIn, request.Value, request.Body, ++request.Attempt);
            return new ServiceResponse<WriteContractResponse>(false);
        }
        
        public async UniTask<ServiceResponse<WriteContractResponse>> WriteMethodAsyncImpl<T>(ContractInfo contractInfo, string localAccountNameIn, string gasPriceIn, string value, T body, int attempt)
        {
            var switchChainResonse = await SwitchChain(contractInfo);
            if (!switchChainResonse.IsSuccess)
                return await HandleWriteMethodError(switchChainResonse, new SerialisedWriteRequest<T>(contractInfo, localAccountNameIn, gasPriceIn, value, body, attempt));
            if (!await AttemptToLoadContract(contractInfo))
                return new ServiceResponse<WriteContractResponse>(false);
            
            string gasPrice = String.Empty;
            string localAccountName = String.Empty;

            if (!string.IsNullOrEmpty(gasPriceIn) && !string.IsNullOrEmpty(localAccountNameIn))
            {
                gasPrice = "&gasPrice=" + gasPriceIn;
                localAccountName = "&localAccountName=" + localAccountNameIn;
            }

            string url = contractInfo.ToWriteUrl(localAccountName, gasPrice, value);
            string dataString = SerializationHelper.Serialize(body, false);
            
            var headers = new Dictionary<string, string>();
            headers.Add("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbPOST, url, EmergenceLogger.LogError, dataString, headers);
            if(response.IsSuccess == false)
                return await HandleWriteMethodError(response,
                    new SerialisedWriteRequest<T>(contractInfo, localAccountNameIn, gasPriceIn, value, body, attempt));

            var writeContractResponse = SerializationHelper.Deserialize<BaseResponse<WriteContractResponse>>(response.Response);
            CheckForTransactionSuccess(contractInfo, writeContractResponse.message.transactionHash).Forget();
            return new ServiceResponse<WriteContractResponse>(true, writeContractResponse.message);
        }

        private async UniTask<ServiceResponse<WriteContractResponse>> HandleWriteMethodError<T>(WebResponse response, SerialisedWriteRequest<T> serialisedWriteRequest)
        {
            var ret = new ServiceResponse<WriteContractResponse>(false);
            switch (response.StatusCode)
            {
                case 502:
                case 500: 
                case 504:
                {
                    await ReconnectionQR.FireEventOnReconnection(async () => ret = await WriteMethodAsyncRetry(serialisedWriteRequest));
                    break;
                }
            }

            return ret;
        }

        private async UniTask CheckForTransactionSuccess(ContractInfo contractInfo, string transactionHash, int maxAttempts = 10)
        {
            int attempts = 0;
            int timeOut = 7500;
            int confirmations = 0;
            while (attempts < maxAttempts)
            {
                await UniTask.Delay(timeOut);

                var transactionStatus = await EmergenceServices.GetService<IChainService>().GetTransactionStatusAsync(transactionHash, contractInfo.NodeUrl);
                if (transactionStatus.Result?.transaction?.Confirmations != null)
                    confirmations = (int)transactionStatus.Result?.transaction?.Confirmations;
                if(transactionStatus.Result?.transaction?.Confirmations >= desiredConfirmationCount)
                {
                    WriteMethodConfirmed?.Invoke(new WriteContractResponse(transactionHash));
                    break;
                }
                attempts++;
            }
            if(confirmations != 0)
                EmergenceLogger.LogInfo($"Transaction received {confirmations} confirmations after {(timeOut*maxAttempts)/1000} seconds");
            else
                EmergenceLogger.LogWarning("Transaction failed to receive any confirmations");
        }
        
        public async Task<ServiceResponse<WriteContractResponse>> WriteMethodViaPrivateKeyAsync(ContractInfo contractInfo,
            string localAccountNameIn, string gasPriceIn, string value, string privateKey)
        {
            return await WriteMethodViaPrivateKeyImpl(contractInfo, localAccountNameIn, gasPriceIn, value, privateKey, 0);
        }
        
        private async Task<ServiceResponse<WriteContractResponse>> WriteMethodViaPrivateKeyAsyncRetry(WriteRequest request)
        {
            if (request.Attempt <= MaxRetryAttempts)
                return await WriteMethodViaPrivateKeyImpl(request.ContractInfo, request.LocalAccountNameIn, request.GasPriceIn, request.Value, request.PrivateKey, ++request.Attempt);
            return new ServiceResponse<WriteContractResponse>(false);
        }
        
        public async Task<ServiceResponse<WriteContractResponse>> WriteMethodViaPrivateKeyImpl(ContractInfo contractInfo, string localAccountNameIn, string gasPriceIn, string value, string privateKey, int attempt)
        {
            EmergenceEVMLocalServer.Lib.JSONManaged jsonParams = new()
            {
                a = 0, // UNUSED
                b = "", // UNUSED
                _address = contractInfo.ContractAddress,
                _ABI = contractInfo.ABI,
                _network = contractInfo.Network,
                _nodeUrl = contractInfo.NodeUrl,
                _methodName = contractInfo.MethodName,
                _localAccountName = "", // UNUSED
                _password = "", // UNUSED
                _keystorePath = "", // UNUSED
                _chainID = contractInfo.ChainId.ToString(),
                _gasPrice = gasPriceIn,
                _value = value,
                _privateKey = privateKey,
                _publicKey = "" // MAYBE UNUSED?
            };
            
            string transactionHash = EmergenceEVMLocalServer.Lib.JSONifyManaged(jsonParams);
            if (transactionHash != null)
            {
                await CheckForTransactionSuccessTask(contractInfo, transactionHash);
                return new ServiceResponse<WriteContractResponse>(true, new WriteContractResponse(transactionHash) { message = "WriteMethodViaPrivateKeyImpl" });
            }

            return await HandleWriteMethodViaPrivateKeyError(new WriteRequest(contractInfo, localAccountNameIn, gasPriceIn, value, attempt, privateKey));
        }
        
        private async Task<ServiceResponse<WriteContractResponse>> HandleWriteMethodViaPrivateKeyError(WriteRequest writeRequest)
        {
            return await WriteMethodViaPrivateKeyAsyncRetry(writeRequest);
        }
        
        private async Task CheckForTransactionSuccessTask(ContractInfo contractInfo, string transactionHash, int maxAttempts = 10)
        {
            int attempts = 0;
            int timeOut = 7500;
            int confirmations = 0;
            while (attempts < maxAttempts)
            {
                await Task.Delay(timeOut);

                var transactionStatus = await EmergenceServices.GetService<IChainService>().GetTransactionStatusAsync(transactionHash, contractInfo.NodeUrl);
                if (transactionStatus.Result?.transaction?.Confirmations != null)
                    confirmations = (int)transactionStatus.Result?.transaction?.Confirmations;
                if(transactionStatus.Result?.transaction?.Confirmations >= desiredConfirmationCount)
                {
                    WriteMethodConfirmed?.Invoke(new WriteContractResponse(transactionHash));
                    break;
                }
                attempts++;
            }
            if(confirmations != 0)
                EmergenceLogger.LogInfo($"Transaction received {confirmations} confirmations after {(timeOut*maxAttempts)/1000} seconds");
            else
                EmergenceLogger.LogWarning("Transaction failed to receive any confirmations");
        }

        public async UniTask WriteMethod<T>(ContractInfo contractInfo, string localAccountNameIn, string gasPriceIn, string value, T body, WriteMethodSuccess success, ErrorCallback errorCallback)
        {
            var response = await WriteMethodAsync(contractInfo, localAccountNameIn, gasPriceIn, value, body);
            if(response.Success)
                success?.Invoke(response.Result);
            else
                errorCallback?.Invoke("Error in WriteMethod", (long)response.Code);
        }

        public async Task WriteMethodViaPrivateKey(ContractInfo contractInfo, string localAccountNameIn, string gasPriceIn, string value, string privateKey, WriteMethodSuccess success, ErrorCallback errorCallback)
        {
            var response = await WriteMethodViaPrivateKeyAsync(contractInfo, localAccountNameIn, gasPriceIn, value, privateKey);
            if(response.Success)
                success?.Invoke(response.Result);
            else
                errorCallback?.Invoke("Error in WriteMethodViaPrivateKey", (long)response.Code);
        }

        private class SwitchChainRequest
        {
            public int chainId;
            public string chainName;
            public string[] rpcUrls;
            public string currencyName;
            public string currencySymbol;
            public int currencyDecimals = 18;
        }
        
        private async UniTask<WebResponse> SwitchChain(ContractInfo contractInfo)
        {
            string url = StaticConfig.APIBase + "switchChain";
            
            var headers = new Dictionary<string, string>
            {
                {"deviceId", EmergenceSingleton.Instance.CurrentDeviceId}
            };
            var data = new SwitchChainRequest()
            {
                chainId = contractInfo.ChainId,
                chainName = contractInfo.Network,
                rpcUrls = new[]{contractInfo.NodeUrl},
                currencyName = contractInfo.CurrencyName,
                currencySymbol = contractInfo.CurrencySymbol
            };

            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbPOST, url, EmergenceLogger.LogError,
                SerializationHelper.Serialize(data, false), headers);

            return response;
        }
        
        private class WriteRequest
        {
            public readonly ContractInfo ContractInfo;
            public readonly string LocalAccountNameIn;
            public readonly string GasPriceIn;
            public readonly string Value;
            public readonly string PrivateKey;
            public int Attempt;
            
            public WriteRequest(ContractInfo contractInfo, string localAccountNameIn, string gasPriceIn, string value, int attempt, string privateKey = "")
            {
                ContractInfo = contractInfo;
                LocalAccountNameIn = localAccountNameIn;
                GasPriceIn = gasPriceIn;
                Value = value;
                Attempt = attempt;
                PrivateKey = privateKey;
            }
        }
        
        private abstract class SerialisedWriteRequest : WriteRequest
        {
            protected SerialisedWriteRequest(ContractInfo contractInfo, string localAccountNameIn, string gasPriceIn, string value, int attempt, string privateKey = "") : base(contractInfo, localAccountNameIn, gasPriceIn, value, attempt, privateKey) {}
        }
        
        private class SerialisedWriteRequest<T> : SerialisedWriteRequest
        {
            public readonly T Body;
            
            public SerialisedWriteRequest(ContractInfo contractInfo, string localAccountNameIn, string gasPriceIn, string value, T body, int attempt, string privateKey = "") : base(contractInfo, localAccountNameIn, gasPriceIn, value, attempt, privateKey)
            {
                Body = body;
            }
        }
    }
}