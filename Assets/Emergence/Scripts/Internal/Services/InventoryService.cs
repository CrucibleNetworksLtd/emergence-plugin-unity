using Cysharp.Threading.Tasks;
using EmergenceSDK;
using UnityEngine;
using UnityEngine.Networking;

public class InventoryService : IInventoryService
{
    public async UniTask InventoryByOwner(string address, SuccessInventoryByOwner success, ErrorCallback errorCallback)
    {
        Debug.Log("Getting inventory for address: " + address);
        string url = EmergenceSingleton.Instance.Configuration.InventoryURL + "byOwner?address=" + address;
        Debug.Log("Requesting inventory from URL: " + url);
        string response = await Services.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, errorCallback);

        Debug.Log("Inventory response: " + response.ToString());
        InventoryByOwnerResponse inventoryResponse =
            SerializationHelper.Deserialize<InventoryByOwnerResponse>(response.ToString());

        success?.Invoke(inventoryResponse.message.items);
    }
}