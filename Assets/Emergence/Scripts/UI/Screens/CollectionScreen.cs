using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UniGLTF;
using UniVRM10;
using Cysharp.Threading.Tasks;

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
        public TextMeshProUGUI dynamicMetadata;

        private bool isItemSelected = false;
        private InventoryItem selectedItem;

        private void Awake()
        {
            Instance = this;

            detailsPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(443.5f, 0, 0);
        }
        
        public void Refresh(bool dynamicMetada = false)
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

                        if (dynamicMetada)
                        {
                            entryButton.onClick.AddListener(() => OnInventoryItemPressed_DynamicMetadata(item));
                        }
                        else
                        {
                            entryButton.onClick.AddListener(() => OnInventoryItemPressed(item));
                        }
                        
                        
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
            dynamicMetadata.text = "Dynamic metadata: " + item.meta.dynamicMetadata;
            
            if (!isItemSelected)
            {
                DOTween.To(() => detailsPanel.GetComponent<RectTransform>().anchoredPosition,
                    x=> detailsPanel.GetComponent<RectTransform>().anchoredPosition = x, new Vector2(0, 0), 0.25f);
                
                DOTween.To(() => itemsListPanel.GetComponent<RectTransform>().offsetMax,
                    x=> itemsListPanel.GetComponent<RectTransform>().offsetMax = x, new Vector2(-443.5f, 0), 0.25f);

                isItemSelected = true;
                selectedItem = item;
            }
        }
        
        public void OnInventoryItemPressed_DynamicMetadata(InventoryItem item)
        {
            Debug.Log("Updating Dynamic metadata");
            if (string.IsNullOrEmpty(item.meta.dynamicMetadata)) return;
            var curMetadata = int.Parse(item.meta.dynamicMetadata);
            curMetadata++;

            Services.Instance.WriteDynamicMetadata(item.blockchain, item.contract, item.tokenId, curMetadata.ToString(), (string response) => {}, (string error, long code) => {});
        }

        public async void LoadItem()
        {
            if (selectedItem == null) return;

            if (selectedItem.meta.content[1].mimeType.Equals("model/gltf-binary"))
            {
                Debug.Log("Downloading from: " + selectedItem.meta.content[1].url);
                
                UnityWebRequest request = UnityWebRequest.Get(selectedItem.meta.content[1].url);
                byte[] response = (await request.SendWebRequest()).downloadHandler.data;

                var vrm10 = await Vrm10.LoadBytesAsync(response);

                GameObject playerArmature = GameObject.Find("PlayerArmature");
                
                vrm10.transform.position = playerArmature.transform.position;
                vrm10.transform.rotation = playerArmature.transform.rotation;
                // vrm10.transform.parent = playerArmature.transform;
                vrm10.transform.rotation = Quaternion.identity;
            
                // vrm10.name = vrm10.name + "_Imported_v1_0";
            
                await UniTask.DelayFrame(1);
            }
        }

    }


}
