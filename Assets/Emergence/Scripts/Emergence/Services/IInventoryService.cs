using Cysharp.Threading.Tasks;

namespace EmergenceSDK.Services
{
    public interface IInventoryService
    {
        /// <summary>
        /// Attempts to get the inventory for the given address. If successful, the success callback will be called with the inventory.
        /// </summary>
        UniTask InventoryByOwner(string address, SuccessInventoryByOwner success, ErrorCallback errorCallback);
    }
}