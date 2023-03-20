using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    public class InventoryService : IInventoryService
    {
        public async UniTask InventoryByOwner(string address, SuccessInventoryByOwner success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.InventoryURL + "byOwner?address=" + address;
            string response = await EmergenceUtils.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, errorCallback);

            InventoryByOwnerResponse inventoryResponse =
                SerializationHelper.Deserialize<InventoryByOwnerResponse>(response.ToString());

            success?.Invoke(inventoryResponse.message.items);
        }
    }
}