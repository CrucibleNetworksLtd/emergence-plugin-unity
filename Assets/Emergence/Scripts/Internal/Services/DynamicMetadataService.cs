using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    public class DynamicMetadataService : IDynamicMetadataService
    {
        public async UniTask WriteDynamicMetadata(string network, string contract, string tokenId, string metadata,
            SuccessWriteDynamicMetadata success, ErrorCallback errorCallback)
        {
            metadata = "{\"metadata\": \"" + metadata + "\"}";
            
            Debug.Log("Writing dynamic metadata for contract: " + contract);
            Debug.Log("Write dynamic metadata request started");
            string url = EmergenceSingleton.Instance.Configuration.InventoryURL + "updateMetadata?network=" + network + "&contract=" + contract + "&tokenId=" + tokenId;
            
            Debug.Log("Dynamic metadata url: " + url);
            Debug.Log("updated metadata: " + metadata);

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization-header", "0iKoO1V2ZG98fPETreioOyEireDTYwby");
            string response = await EmergenceServices.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbPOST, errorCallback, metadata, headers);
            
            Debug.Log("Write dynamic metadata response: " + response);
            BaseResponse<string> dynamicMetadataResponse = SerializationHelper.Deserialize<BaseResponse<string>>(response);
            
            success?.Invoke(response);
        }
    }
}