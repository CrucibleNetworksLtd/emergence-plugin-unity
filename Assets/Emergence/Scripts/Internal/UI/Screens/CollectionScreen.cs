using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UniVRM10;

namespace EmergenceSDK
{
    
    public class CollectionScreen : MonoBehaviour
    {
        struct Item
        {
            public InventoryItem inventoryItem;
            public GameObject entryGo;
            public Item(InventoryItem inventoryItem, GameObject entryGo)
            {
                this.inventoryItem = inventoryItem;
                this.entryGo = entryGo;
            }
        
        }
        
        private class FilterParams
        {
            public string searchString ;
            public bool avatars;
            public bool props;
            public bool clothing;
            public bool weapons;
            public string blockchain;
            public FilterParams()
            {
                searchString = "";
                avatars = true;
                props = true;
                clothing = true;
                weapons = true;
                blockchain = "ANY";
            }
        }
        
        public static CollectionScreen Instance;

        public GameObject contentGO;
        public GameObject itemEntryPrefab;
        public GameObject itemsListPanel;
        public GameObject detailsPanel;
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI itemDescriptionText;
        public TextMeshProUGUI dynamicMetadata;
        public TMP_InputField searchInputField;
        public Toggle avatarsToggle;
        public Toggle propsToggle;
        public Toggle clothingToggle;
        public Toggle weaponsToggle;
        public TMP_Dropdown blockchainDropdown;

        private bool isItemSelected = false;
        private InventoryItem selectedItem;

        private List<Item> items = new List<Item>();

        private FilterParams filterParams = new FilterParams();

        private void Awake()
        {
            Instance = this;
            detailsPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(Screen.width, 0, 0);
            
            searchInputField.onValueChanged.AddListener(OnSearchFieldValueChanged);
            avatarsToggle.onValueChanged.AddListener(onAvatarsToggleValueChanged);
            propsToggle.onValueChanged.AddListener(onPropsToggleValueChanged);
            clothingToggle.onValueChanged.AddListener(onClothingToggleValueChanged);
            weaponsToggle.onValueChanged.AddListener(onWeaponsToggleValueChanged);
            
            blockchainDropdown.onValueChanged.AddListener(onBlockchainDropdownValueChanged);
        }
        
        public void Refresh(Action<InventoryItem> customOnClickHandler)
        {
            EmergenceServices.Instance.InventoryByOwner(EmergenceSingleton.Instance.GetCachedAddress(), (inventoryItems) =>
                {

                    foreach (var item in items)
                    {
                        Destroy(item.entryGo);
                    }
                    items.Clear();
                    
                    
                    Debug.Log("Received items: " + inventoryItems.Count);
                    Modal.Instance.Show("Retrieving inventory items...");
                    
                    for (int i = 0; i < inventoryItems.Count; i++)
                    {
                        GameObject entry = Instantiate(itemEntryPrefab);
                        
                        items.Add(new Item(inventoryItems[i], entry));

                        Button entryButton = entry.GetComponent<Button>();
                        InventoryItem item = inventoryItems[i];

                        if (customOnClickHandler != null)
                        {
                            entryButton.onClick.AddListener(() => customOnClickHandler(item));
                        }
                        else
                        {
                            entryButton.onClick.AddListener(() => OnInventoryItemPressed(item));
                        }
                        
                        
                        InventoryItemEntry itemEntry = entry.GetComponent<InventoryItemEntry>();
                        itemEntry.SetItem(inventoryItems[i]);

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

        private void RefreshFilteredResults()
        {
            foreach (var item in items)
            {
                // Search string
                string itemName = item.inventoryItem.meta?.name.ToLower();
                string itemBlockchain = item.inventoryItem.blockchain;

                bool searchStringResult = string.IsNullOrEmpty(itemName) ||
                                          itemName.StartsWith(filterParams.searchString.ToLower()) ||
                                          string.IsNullOrEmpty(filterParams.searchString);

                bool blockchainResult = filterParams.blockchain.Equals("ANY") ||
                                        itemBlockchain.Equals(filterParams.blockchain);
                
                // if (string.IsNullOrEmpty(itemName) || itemName.StartsWith(filterParams.searchString.ToLower()) || string.IsNullOrEmpty(filterParams.searchString))
                if (searchStringResult && blockchainResult)
                {
                    item.entryGo.SetActive(true);
                }
                else
                {
                    item.entryGo.SetActive(false);
                }
            }
        }

        private void OnSearchFieldValueChanged(string searchString)
        {
            filterParams.searchString = searchString;
            RefreshFilteredResults();
        }

        private void onAvatarsToggleValueChanged(bool selected)
        {
            filterParams.avatars = selected;
            RefreshFilteredResults();
        }
        
        private void onPropsToggleValueChanged(bool selected)
        {
            filterParams.props = selected;
            RefreshFilteredResults();
        }
        
        private void onClothingToggleValueChanged(bool selected)
        {
            filterParams.clothing = selected;
            RefreshFilteredResults();
        }
        
        private void onWeaponsToggleValueChanged(bool selected)
        {
            filterParams.weapons = selected;
            RefreshFilteredResults();
        }

        private void onBlockchainDropdownValueChanged(int selection)
        {
            Debug.Log(blockchainDropdown.options[selection].text);
            filterParams.blockchain = blockchainDropdown.options[selection].text.ToUpper();
            RefreshFilteredResults();
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

        public void OnCloseDetailsPanelButtonPressed() {
            DOTween.To(() => detailsPanel.GetComponent<RectTransform>().anchoredPosition,
                x=> detailsPanel.GetComponent<RectTransform>().anchoredPosition = x, new Vector2(Screen.width / 2, 0), 0.25f);
                
            DOTween.To(() => itemsListPanel.GetComponent<RectTransform>().offsetMax,
                x=> itemsListPanel.GetComponent<RectTransform>().offsetMax = x, new Vector2(0, 0), 0.25f);
            
            isItemSelected = false;
        }
    }


}
