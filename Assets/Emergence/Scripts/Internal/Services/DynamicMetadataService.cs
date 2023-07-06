using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class DynamicMetadataService : IDynamicMetadataService
    {
        public async UniTask WriteDynamicMetadata(string network, string contract, string tokenId, string metadata,
            SuccessWriteDynamicMetadata success, ErrorCallback errorCallback)
        {
            metadata = "{\"metadata\": \"" + metadata + "\"}";
            
            string url = EmergenceSingleton.Instance.Configuration.InventoryURL + "updateMetadata?network=" + network + "&contract=" + contract + "&tokenId=" + tokenId;

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization-header", "0iKoO1V2ZG98fPETreioOyEireDTYwby");
            var response = await WebRequestService.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbPOST, errorCallback, metadata, headers);
            if(response.IsSuccess == false)
                return;
            
            success?.Invoke(response.Response);
        }
    }
}