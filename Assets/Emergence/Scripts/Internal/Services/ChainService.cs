using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    public class ChainService : IChainService
    {
        public async UniTask<ServiceResponse<GetTransactionStatusResponse>> GetTransactionStatusAsync(string transactionHash, string nodeURL)
        {
            string url = StaticConfig.APIBase + "GetTransactionStatus?transactionHash=" + transactionHash + "&nodeURL=" + nodeURL;
            WebResponse response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, url, EmergenceLogger.LogError);
            if(response.IsSuccess == false)
                return new ServiceResponse<GetTransactionStatusResponse>(false);
            var transactionStatusResponse = SerializationHelper.Deserialize<BaseResponse<GetTransactionStatusResponse>>(response.Response);
            return new ServiceResponse<GetTransactionStatusResponse>(true, transactionStatusResponse.message);
        }

        public async UniTask GetTransactionStatus(string transactionHash, string nodeURL, GetTransactionStatusSuccess success, ErrorCallback errorCallback)
        {
            var response = await GetTransactionStatusAsync(transactionHash, nodeURL);
            if(response.Success)
                success?.Invoke(response.Result);
            else
                errorCallback?.Invoke("Error in GetTransactionStatus.", (long)response.Code);
        }

        public async UniTask<ServiceResponse<GetBlockNumberResponse>> GetHighestBlockNumberAsync(string nodeURL)
        {
            string url = StaticConfig.APIBase + "getBlockNumber?nodeURL=" + nodeURL;
            WebResponse response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, url, EmergenceLogger.LogError);
            if(response.IsSuccess == false)
                return new ServiceResponse<GetBlockNumberResponse>(false);
            var blockNumberResponse = SerializationHelper.Deserialize<BaseResponse<GetBlockNumberResponse>>(response.Response);
            return new ServiceResponse<GetBlockNumberResponse>(true, blockNumberResponse.message);
        }

        public async UniTask GetHighestBlockNumber(string nodeURL, GetBlockNumberSuccess success, ErrorCallback errorCallback)
        {
            var response = await GetHighestBlockNumberAsync(nodeURL);
            if(response.Success)
                success?.Invoke(response.Result);
            else
                errorCallback?.Invoke("Error in GetHighestBlockNumber.", (long)response.Code);
        }
    }
}