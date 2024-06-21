using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Integrations.Futureverse.Services;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;
using EmergenceSDK.Types.Inventory;
using EmergenceSDK.Types.Responses;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class InventoryService : IInventoryService
    {
        public async UniTask<ServiceResponse<List<InventoryItem>>> InventoryByOwnerAsync(string address, InventoryChain chain)
        {
            string url = EmergenceSingleton.Instance.Configuration.InventoryURL + "byOwner?address=" + address + "&chain=" + InventoryKeys.ChainToKey[chain];
            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url);
            if(!response.Successful)
                return new ServiceResponse<List<InventoryItem>>(false);
            
            InventoryByOwnerResponse inventoryResponse = SerializationHelper.Deserialize<InventoryByOwnerResponse>(response.ResponseText);

            return new ServiceResponse<List<InventoryItem>>(true, inventoryResponse.message.items);
        }

        public async UniTask InventoryByOwner(string address, InventoryChain chain, SuccessInventoryByOwner success, ErrorCallback errorCallback)
        {
            var response = await InventoryByOwnerAsync(address, chain);
            if(response.Successful)
                success?.Invoke(response.Result1);
            else
                errorCallback?.Invoke("Error in InventoryByOwner.", (long)response.Code);
        }
    }
}