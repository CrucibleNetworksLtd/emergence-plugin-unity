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
        private List<string> loadedAddresses = new List<string>();

        private async UniTask<bool> LoadContract(string contractAddress, string ABI, string network, LoadContractSuccess success, ErrorCallback errorCallback)
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
            await WebRequestService.PerformAsyncWebRequest(request, errorCallback);

            if (EmergenceUtils.ProcessRequest<LoadContractResponse>(request, errorCallback, out var response))
            {
                loadedAddresses.Add(contractAddress);
                success?.Invoke();
            }
            
            return loadedAddresses.Contains(contractAddress);
        }

        private bool CheckForNewContract(ContractInfo contractInfo) => !loadedAddresses.Contains(contractInfo.ContractAddress);

        public async UniTask ReadMethod<T>(ContractInfo contractInfo, T body, ReadMethodSuccess success, ErrorCallback errorCallback)
        {
            if (await LoadContractIfNew(contractInfo, errorCallback)) 
                return;

            string url = contractInfo.ToReadUrl();
            string dataString = SerializationHelper.Serialize(body, false);

            var response = await WebRequestService.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbPOST, EmergenceLogger.LogError, dataString);
            var readContractResponse = SerializationHelper.Deserialize<BaseResponse<ReadContractResponse>>(response);
            success?.Invoke(readContractResponse.message);
        }

        private async Task<bool> LoadContractIfNew(ContractInfo contractInfo, ErrorCallback errorCallback)
        {
            if (CheckForNewContract(contractInfo))
            {
                bool loadedSuccessfully = await LoadContract(contractInfo.ContractAddress, contractInfo.ABI,
                    contractInfo.Network, null, errorCallback);
                if (!loadedSuccessfully)
                {
                    errorCallback?.Invoke("Error loading contract", -1);
                    return true;
                }
            }

            return false;
        }

        public async UniTask WriteMethod<T>(ContractInfo contractInfo, string localAccountNameIn, string gasPriceIn, string value, T body, WriteMethodSuccess success, ErrorCallback errorCallback)
        {
            if (await LoadContractIfNew(contractInfo, errorCallback)) 
                return;

            string gasPrice = String.Empty;
            string localAccountName = String.Empty;

            if (!string.IsNullOrEmpty(gasPriceIn) && !string.IsNullOrEmpty(localAccountNameIn))
            {
                gasPrice = "&gasPrice=" + gasPriceIn;
                localAccountName = "&localAccountName=" + localAccountNameIn;
            }

            string url = contractInfo.ToWriteUrl(localAccountName, gasPrice, value);
            string dataString = SerializationHelper.Serialize(body, false);
            
            var response = await WebRequestService.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbPOST, EmergenceLogger.LogError, dataString);
            var writeContractResponse = SerializationHelper.Deserialize<BaseResponse<WriteContractResponse>>(response);
            success?.Invoke(writeContractResponse.message);
        }
    }
}