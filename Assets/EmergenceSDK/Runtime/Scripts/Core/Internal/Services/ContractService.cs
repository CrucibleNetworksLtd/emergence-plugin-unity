using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    internal class ContractService : IContractService
    {
        public event WriteMethodSuccess WriteMethodConfirmed;
        
        private readonly List<string> loadedContractAddresses = new();
        private const int DesiredConfirmationCount = 1;
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
                bool loadedSuccessfully = await LoadContract(contractInfo.ContractAddress, contractInfo.ABI, contractInfo.Network, contractInfo.ChainId);
                if (!loadedSuccessfully)
                {
                    EmergenceLogger.LogError("Error loading contract");
                    return false;
                }
            }
            return true;
        }

        private async UniTask<bool> LoadContract(string contractAddress, string ABI, string network, int chainId)
        {
            Contract data = new Contract()
            {
                contractAddress = contractAddress,
                ABI = ABI,
                network = network,
                chainId = chainId
            };

            string dataString = SerializationHelper.Serialize(data, false);
            string url = StaticConfig.APIBase + "loadContract";

            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Post, url, dataString);

            if (response.Successful && EmergenceUtils.ProcessResponse<LoadContractResponse>(response, EmergenceLogger.LogError, out var processedResponse))
            {
                loadedContractAddresses.Add(contractAddress);
            }

            return loadedContractAddresses.Contains(contractAddress);
        }

        public async UniTask<ServiceResponse<ReadContractResponse>> ReadMethodAsync<T>(ContractInfo contractInfo, T parameters)
        {
            if (!await AttemptToLoadContract(contractInfo)) 
                return new ServiceResponse<ReadContractResponse>(false);
            
            string url = contractInfo.ToReadUrl();
            string dataString = SerializationHelper.Serialize(parameters, false);

            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Post, url, dataString);
            if(!response.Successful)
                return new ServiceResponse<ReadContractResponse>(false);
            var readContractResponse = SerializationHelper.Deserialize<BaseResponse<ReadContractResponse>>(response.ResponseText);
            return new ServiceResponse<ReadContractResponse>(true, readContractResponse.message);
        }
        
        public async UniTask ReadMethod<T>(ContractInfo contractInfo, T parameters, ReadMethodSuccess success, ErrorCallback errorCallback)
        {
            var response = await ReadMethodAsync(contractInfo, parameters);
            if(response.Successful)
                success?.Invoke(response.Result1);
            else
                errorCallback?.Invoke("Error in ReadMethod", (long)response.Code);
        }

        public async UniTask<ServiceResponse<WriteContractResponse>> WriteMethodAsync<T>(ContractInfo contractInfo,
            string value, T parameters)
        {
            return await WriteMethodAsyncImpl(contractInfo, value, parameters, 0);
        }

        private async UniTask<ServiceResponse<WriteContractResponse>> WriteMethodAsyncRetry<T>(SerialisedWriteRequest<T> request)
        {
            if (request.Attempt <= MaxRetryAttempts)
                return await WriteMethodAsyncImpl(request.ContractInfo, request.Value, request.Body, ++request.Attempt);
            return new ServiceResponse<WriteContractResponse>(false);
        }
        
        public async UniTask<ServiceResponse<WriteContractResponse>> WriteMethodAsyncImpl<T>(ContractInfo contractInfo, string value, T body, int attempt)
        {
            var custodialWriteService = EmergenceServiceProvider.GetService<ICustodialWriteService>();
            if (custodialWriteService == null)
            {
                // We don't need to perform switch chain requests for custodial logins.
                var switchChainResonse = await SwitchChain(contractInfo);
                if (!switchChainResonse.Successful)
                    return await HandleWriteMethodError(switchChainResonse, new SerialisedWriteRequest<T>(contractInfo, value, body, attempt));
            }

            if (!await AttemptToLoadContract(contractInfo))
                return new ServiceResponse<WriteContractResponse>(false);
            
            string gasPrice = String.Empty;
            string localAccountName = String.Empty;
            string url = contractInfo.ToWriteUrl(localAccountName, gasPrice, value);
            string dataString = SerializationHelper.Serialize(body, false);
            
            var headers = new Dictionary<string, string>();
            headers.Add("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
            WebResponse response = null;
            
            // If its not a custodial write we can perform typical write method flow.
            if (custodialWriteService == null)
            {
                response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Post, url, dataString, headers);
            }
            else // Otherwise we have to utilise the CustodialWriteService to interact with our Custodial supporting architecture.
            {
                response = await custodialWriteService.PerformCustodialWriteMethod(contractInfo,value,dataString);
            }
            
            if(!response.Successful)
                return await HandleWriteMethodError(response,
                    new SerialisedWriteRequest<T>(contractInfo, value, body, attempt));
            
            // Because the backend returns two different response types that don't deserialise the same we need to validate them differently.
            if(custodialWriteService == null)
            {
                var writeContractResponse = SerializationHelper.Deserialize<BaseResponse<WriteContractResponse>>(response.ResponseText);
                CheckForTransactionSuccess(contractInfo, writeContractResponse.message.transactionHash).Forget();
                return new ServiceResponse<WriteContractResponse>(true, writeContractResponse.message);
            }
            else 
            {
                 var custodialWriteContractResponse = SerializationHelper.Deserialize<CustodialWriteContractResponse>(response.ResponseText);
                 CheckForTransactionSuccess(contractInfo, custodialWriteContractResponse.hash).Forget(); // Check the transaction hash as usual.
                 return new ServiceResponse<WriteContractResponse>(true);
            }
        }

        private UniTask<ServiceResponse<WriteContractResponse>> HandleWriteMethodError<T>(WebResponse response, SerialisedWriteRequest<T> serialisedWriteRequest)
        {
            var ret = new ServiceResponse<WriteContractResponse>(false);
            return UniTask.FromResult(ret);
        }

        private async UniTask CheckForTransactionSuccess(ContractInfo contractInfo, string transactionHash, int maxAttempts = 10)
        {
            int attempts = 0;
            int timeOut = 7500;
            int confirmations = 0;
            while (attempts < maxAttempts)
            {
                await UniTask.Delay(timeOut);

                var transactionStatus = await EmergenceServiceProvider.GetService<IChainService>().GetTransactionStatusAsync(transactionHash, contractInfo.NodeUrl);
                if (transactionStatus.Result1?.transaction?.Confirmations != null)
                    confirmations = (int)transactionStatus.Result1?.transaction?.Confirmations;
                if(transactionStatus.Result1?.transaction?.Confirmations >= DesiredConfirmationCount)
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

        public async UniTask WriteMethod<T>(ContractInfo contractInfo, string value, T parameters, WriteMethodSuccess success, ErrorCallback errorCallback)
        {
            var response = await WriteMethodAsync(contractInfo, value, parameters);
            if(response.Successful)
                success?.Invoke(response.Result1);
            else
                errorCallback?.Invoke("Error in WriteMethod", (long)response.Code);
        }

        public UniTask WriteMethod<T>(ContractInfo contractInfo, string localAccountNameIn, string gasPriceIn, string value, T parameters, WriteMethodSuccess success,
            ErrorCallback errorCallback)
        {
            return WriteMethod(contractInfo, value, parameters, success, errorCallback);
        }

        public UniTask<ServiceResponse<WriteContractResponse>> WriteMethodAsync<T>(ContractInfo contractInfo, string localAccountNameIn, string gasPriceIn, string value, T parameters)
        {
            return WriteMethodAsync(contractInfo, value, parameters);
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

            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Post, url,
                SerializationHelper.Serialize(data, false), headers);

            return response;
        }
        
        private class SerialisedWriteRequest<T>
        {
            public readonly ContractInfo ContractInfo;
            public readonly string Value;
            public readonly T Body;
            public int Attempt;
            
            public SerialisedWriteRequest(ContractInfo contractInfo, string value, T body, int attempt)
            {
                ContractInfo = contractInfo;
                Value = value;
                Body = body;
                Attempt = attempt;
            }
        }
    }
}