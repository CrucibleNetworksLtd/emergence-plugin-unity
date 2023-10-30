using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Services;
using EmergenceSDK.Types.Inventory;
using UnityEngine;

namespace EmergenceSDK.Samples.Examples
{
    public class GetInventory : MonoBehaviour
    {
        IInventoryService inventoryService;
        
        public void Awake()
        {
            inventoryService = EmergenceServices.GetService<IInventoryService>();
        }
        
        /// <summary>
        /// Gets all inventory items for the given address. Note that it will be for any compatible chain. See <see cref="InventoryChain"/>
        /// </summary>
        /// <param name="address">The address of the wallet that you want to get the items from</param>
        /// <returns>A list of the items, see <see cref="InventoryItem"/> for what info is contained. Returns an empty list if there was an error.</returns>
        public async UniTask<List<InventoryItem>> GetInventoryAtAddress(string address)
        {
            List<InventoryItem> inventory = new List<InventoryItem>();
            //Make the call to the service
            var response = await inventoryService.InventoryByOwnerAsync(address, InventoryChain.AnyCompatible);
            //If the call was successful, set the inventory to the result
            if (response.Success)
            {
                inventory = response.Result;
            }
            return inventory;
        }
    }
}