using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK
{
    public class CollectionScreen : MonoBehaviour
    {
        public static CollectionScreen Instance;

        public GameObject contentGO;
        public GameObject itemEntryPrefab;

        private enum States
        {
            CreateAvatar,
            CreateInformation,
            EditInformation,
            EditAvatar,
        }

        private States state;

        private void Awake()
        {
            Instance = this;
        }
        
        public void Refresh()
        {
            Services.Instance.InventoryByOwner(EmergenceSingleton.Instance.GetCachedAddress(), (inventoryItems) =>
                {
                    Debug.Log("Received items: " + inventoryItems.Count);
                    Modal.Instance.Show("Retrieving inventory items...");
                    
                    for (int i = 0; i < inventoryItems.Count; i++)
                    {
                        GameObject entry = Instantiate(itemEntryPrefab);
                        InventoryItemEntry itemEntry = entry.GetComponent<InventoryItemEntry>();
                        itemEntry.itemName.text = inventoryItems[i].meta.name;
                        itemEntry.url = inventoryItems[i].meta.content.First().url;
                        itemEntry.SetImageUrl(inventoryItems[i].meta.content.First().url);
                        
                        entry.transform.SetParent(contentGO.transform, false);
                    }
                    
                    Modal.Instance.Hide();
                    
                },
                (error, code) =>
                {
                    Debug.LogError("[" + code + "] " + error);
                    Modal.Instance.Hide();
                });
        }

    }


}
