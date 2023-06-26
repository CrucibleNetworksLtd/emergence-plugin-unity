using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class InventoryService : IInventoryService
    {
        public async UniTask InventoryByOwner(string address, InventoryChain chain, SuccessInventoryByOwner success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.InventoryURL + "byOwner?address=" + address + "&chain=" + InventoryKeys.ChainToKey[chain];
            string response = await WebRequestService.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, errorCallback);

            InventoryByOwnerResponse inventoryResponse = SerializationHelper.Deserialize<InventoryByOwnerResponse>(response);

            success?.Invoke(inventoryResponse.message.items);
        }
    }
}