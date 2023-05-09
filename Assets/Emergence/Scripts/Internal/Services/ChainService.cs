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
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "GetTransactionStatus?transactionHash=" + transactionHash + "&nodeURL=" + nodeURL;
    
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

        public async UniTask GetBlockNumber<T, U>(string transactionHash, string nodeURL, U body, GetBlockNumberSuccess<T> success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "getBlockNumber?nodeURL=" + nodeURL;
    
            string dataString = SerializationHelper.Serialize(body, false);
    
            using (UnityWebRequest request = UnityWebRequest.Post(url, ""))
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";
        
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