using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
        private bool CheckForNewContract(ContractInfo contractInfo) => !loadedContractAddresses.Contains(contractInfo.ContractAddress);
        
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

        public async UniTask<ServiceResponse<WriteContractResponse>> WriteMethodAsync<T>(ContractInfo contractInfo, string localAccountNameIn, string gasPriceIn, string value, T body)
        {
            if(!await SwitchChain(contractInfo))
                return new ServiceResponse<WriteContractResponse>(false);
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
                return new ServiceResponse<WriteContractResponse>(false);
            
            var writeContractResponse = SerializationHelper.Deserialize<BaseResponse<WriteContractResponse>>(response.Response);
            CheckForTransactionSuccess(contractInfo, writeContractResponse.message.transactionHash).Forget();
            return new ServiceResponse<WriteContractResponse>(true, writeContractResponse.message);
        }
        
        private async UniTask CheckForTransactionSuccess(ContractInfo contractInfo, string transactionHash, int maxAttempts = 15)
        {
            int attempts = 0;
            int timeOut = 5000;
            while (attempts < maxAttempts)
            {
                await UniTask.Delay(timeOut);

                var transactionStatus = await EmergenceServices.GetService<IChainService>().GetTransactionStatusAsync(transactionHash, contractInfo.NodeUrl);
                if(transactionStatus.Result?.transaction?.Confirmations >= 3)
                {
                    WriteMethodConfirmed?.Invoke(new WriteContractResponse(transactionHash));
                    return;
                }
                attempts++;
            }
            EmergenceLogger.LogError("Transaction failed");
        }

        public async UniTask WriteMethod<T>(ContractInfo contractInfo, string localAccountNameIn, string gasPriceIn, string value, T body, WriteMethodSuccess success, ErrorCallback errorCallback)
        {
            var response = await WriteMethodAsync(contractInfo, localAccountNameIn, gasPriceIn, value, body);
            if(response.Success)
                success?.Invoke(response.Result);
            else
                errorCallback?.Invoke("Error in WriteMethod", (long)response.Code);
        }

        internal class SwitchChainRequest
        {
            public int chainId;
            public string chainName;
            public string[] rpcUrls;
        }
        
        private async UniTask<bool> SwitchChain(ContractInfo contractInfo)
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
                rpcUrls = new[]{contractInfo.NodeUrl}
            };

            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbPOST, url, EmergenceLogger.LogError,
                SerializationHelper.Serialize(data, false), headers);

            return response.IsSuccess;
        }
    }
}