using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    public class ChainService : IChainService
    {
        public async UniTask GetTransactionStatus<T>(string transactionHash, string nodeURL, GetTransactionStatusSuccess<T> success, ErrorCallback errorCallback)
        {
            string url = StaticConfig.APIBase + "GetTransactionStatus?transactionHash=" + transactionHash + "&nodeURL=" + nodeURL;
    
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
        
                EmergenceUtils.PrintRequestResult("Get Transaction Status", request);
        
                if (EmergenceUtils.ProcessRequest<T>(request, errorCallback, out var response))
                {
                    success?.Invoke(response);
                }
            }
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