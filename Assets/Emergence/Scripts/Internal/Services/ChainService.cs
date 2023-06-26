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
        public async UniTask GetTransactionStatus(string transactionHash, string nodeURL, GetTransactionStatusSuccess success, ErrorCallback errorCallback)
        {
            string url = StaticConfig.APIBase + "GetTransactionStatus?transactionHash=" + transactionHash + "&nodeURL=" + nodeURL;
            string response = await WebRequestService.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, errorCallback);
            var transactionStatusResponse = SerializationHelper.Deserialize<BaseResponse<GetTransactionStatusResponse>>(response);
            success?.Invoke(transactionStatusResponse.message);
        }

        public async UniTask GetHighestBlockNumber(string nodeURL, GetBlockNumberSuccess success, ErrorCallback errorCallback)
        {
            //GetBlockNumberResponse
            string url = StaticConfig.APIBase + "getBlockNumber?nodeURL=" + nodeURL;
            string response = await WebRequestService.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, errorCallback);
            var blockNumberResponse = SerializationHelper.Deserialize<BaseResponse<GetBlockNumberResponse>>(response);
            success?.Invoke(blockNumberResponse.message);
        }
    }
}