using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
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
        public GameObject itemsListPanel;
        public GameObject detailsPanel;
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI itemDescriptionText;

        private bool isItemSelected = false;

        private void Awake()
        {
            Instance = this;

            detailsPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(443.5f, 0, 0);
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

                        Button entryButton = entry.GetComponent<Button>();
                        InventoryItem item = inventoryItems[i];
                        entryButton.onClick.AddListener(() => OnInventoryItemPressed(item));
                        
                        InventoryItemEntry itemEntry = entry.GetComponent<InventoryItemEntry>();
                        itemEntry.itemName.text = inventoryItems[i]?.meta?.name;
                        itemEntry.url = inventoryItems[i]?.meta?.content?.First().url;
                        itemEntry.SetImageUrl(inventoryItems[i]?.meta?.content?.First().url);
                        
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

        public void OnInventoryItemPressed(InventoryItem item)
        {
            
            

            itemNameText.text = item.meta.name;
            itemDescriptionText.text = item.meta.description;
            
            if (!isItemSelected)
            {
                DOTween.To(() => detailsPanel.GetComponent<RectTransform>().anchoredPosition,
                    x=> detailsPanel.GetComponent<RectTransform>().anchoredPosition = x, new Vector2(0, 0), 0.25f);
                
                DOTween.To(() => itemsListPanel.GetComponent<RectTransform>().offsetMax,
                    x=> itemsListPanel.GetComponent<RectTransform>().offsetMax = x, new Vector2(-443.5f, 0), 0.25f);

                isItemSelected = true;
            }
        }

    }


}
