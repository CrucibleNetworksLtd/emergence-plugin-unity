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

        public async UniTask GetHighestBlockNumber<T>(string nodeURL, GetBlockNumberSuccess<T> success, ErrorCallback errorCallback)
        {
            string url = StaticConfig.APIBase + "getBlockNumber?nodeURL=" + nodeURL;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                try
                {
                    await request.SendWebRequest().ToUniTask();
                }
                catch (Exception e)
                {
                    errorCallback?.Invoke(e.Message, e.HResult);
                }
        
                EmergenceUtils.PrintRequestResult("Get Block Number", request);
        
                if (EmergenceUtils.ProcessRequest<T>(request, errorCallback, out var response))
                {
                    success?.Invoke(response);
                }
            }
        }
    }
}