using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class DynamicMetadataService : IDynamicMetadataService
    {
        public async UniTask<ServiceResponse<string>> WriteNewDynamicMetadataAsync(string network, string contract, string tokenId, string metadata)
        {
            string url = EmergenceSingleton.Instance.Configuration.InventoryURL + "putMetadata?network=" + network + "&contract=" + contract + "&tokenId=" + tokenId;

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization-header", "0iKoO1V2ZG98fPETreioOyEireDTYwby");
            var bodyData = "{\"metadata\": \"" + metadata + "\"}";
            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbPUT, url, EmergenceLogger.LogError, bodyData, headers);
            if(response.IsSuccess == false)
                return new ServiceResponse<string>(false);
            
            return new ServiceResponse<string>(true, response.Response);
        }

        public async UniTask WriteNewDynamicMetadata(string network, string contract, string tokenId, string metadata,
            SuccessWriteDynamicMetadata success, ErrorCallback errorCallback)
        {
            var response = await WriteNewDynamicMetadataAsync(network, contract, tokenId, metadata);
            if(response.Success)
                success?.Invoke(response.Result);
            else
                errorCallback?.Invoke("Error in WriteDynamicMetadata.", (long)response.Code);
        }
        
        public async UniTask<ServiceResponse<string>> WriteDynamicMetadataAsync(string network, string contract, string tokenId, string metadata)
        {
            string url = EmergenceSingleton.Instance.Configuration.InventoryURL + "updateMetadata?network=" + network + "&contract=" + contract + "&tokenId=" + tokenId;

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization-header", "0iKoO1V2ZG98fPETreioOyEireDTYwby");
            var bodyData = "{\"metadata\": \"" + metadata + "\"}";
            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbPOST, url, EmergenceLogger.LogError, bodyData, headers);
            if(response.IsSuccess == false)
                return new ServiceResponse<string>(false);
            
            return new ServiceResponse<string>(true, response.Response);
        }

        public async UniTask WriteDynamicMetadata(string network, string contract, string tokenId, string metadata,
            SuccessWriteDynamicMetadata success, ErrorCallback errorCallback)
        {
            var response = await WriteDynamicMetadataAsync(network, contract, tokenId, metadata);
            if(response.Success)
                success?.Invoke(response.Result);
            else
                errorCallback?.Invoke("Error in WriteDynamicMetadata.", (long)response.Code);
        }
    }
}