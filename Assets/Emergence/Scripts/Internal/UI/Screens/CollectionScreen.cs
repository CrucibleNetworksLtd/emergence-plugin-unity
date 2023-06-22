using System;
using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Internal.UI.Inventory;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Avatar = EmergenceSDK.Types.Avatar;

namespace EmergenceSDK.Internal.UI.Screens
{
    
    public class CollectionScreen : MonoBehaviour
    {
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

        private List<Avatar> avatars = new List<Avatar>();

        private FilterParams filterParams = new FilterParams();
        private IInventoryService inventoryService;
        private IAvatarService avatarService;
        
        public event Action<InventoryItem> OnItemClicked;
        
        private InventoryItemStore inventoryItemStore;

        private void Awake()
        {
            Instance = this;
            detailsPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(Screen.width, 0, 0);
            
            searchInputField.onValueChanged.AddListener(OnSearchFieldValueChanged);
            avatarsToggle.onValueChanged.AddListener(OnAvatarsToggleValueChanged);
            propsToggle.onValueChanged.AddListener(OnPropsToggleValueChanged);
            clothingToggle.onValueChanged.AddListener(OnClothingToggleValueChanged);
            weaponsToggle.onValueChanged.AddListener(OnWeaponsToggleValueChanged);
            
            blockchainDropdown.onValueChanged.AddListener(OnBlockchainDropdownValueChanged);
            
            inventoryItemStore = new InventoryItemStore(InstantiateItemEntry);
        }

        private GameObject InstantiateItemEntry() => Instantiate(itemEntryPrefab, contentGO.transform, false);
        
        public void Refresh()
        {
            inventoryService = EmergenceServices.GetService<IInventoryService>();
            inventoryService.InventoryByOwner(EmergenceSingleton.Instance.GetCachedAddress(), InventoryChain.AnyCompatible, InventoryByOwnerSuccess, InventoryRefreshErrorCallback);

            avatarService = EmergenceServices.GetService<IAvatarService>();
            avatarService.AvatarsByOwner(EmergenceSingleton.Instance.GetCachedAddress(), AvatarsByOwnerSuccess, InventoryRefreshErrorCallback);
        }
        
        private void InventoryByOwnerSuccess(List<InventoryItem> inventoryItems)
        {
            EmergenceLogger.LogInfo("Received items: " + inventoryItems.Count);
            Modal.Instance.Show("Retrieving inventory items...");
            
            inventoryItemStore.SetItems(inventoryItems);

            foreach (var entry in inventoryItemStore.GetAllEntries())
            {
                Button entryButton = entry.GetComponent<Button>();
                InventoryItem item = entry.Item;
                entryButton.onClick.AddListener(() => OnInventoryItemPressed(item));
                entryButton.onClick.AddListener(() => OnItemClicked?.Invoke(item));
            }
            Modal.Instance.Hide();
        }

        private void AvatarsByOwnerSuccess(List<Avatar> avatar)
        {
            avatars = avatar;
        }

        private void InventoryRefreshErrorCallback(string error, long code)
        {
            EmergenceLogger.LogError(error, code);
            Modal.Instance.Hide();
        }

        private void RefreshFilteredResults()
        {
            foreach (var itemEntry in inventoryItemStore.GetAllEntries())
            {
                var item = itemEntry.Item;
                // Search string filter
                string itemName = item.Meta?.Name.ToLower();
                bool searchStringResult = string.IsNullOrEmpty(itemName) || itemName.StartsWith(filterParams.searchString.ToLower()) || string.IsNullOrEmpty(filterParams.searchString);

                // Blockchain filter
                string itemBlockchain = item.Blockchain;
                bool blockchainResult = filterParams.blockchain.Equals("ANY") || itemBlockchain.Equals(filterParams.blockchain);
                
                //Avatar filter
                bool isAvatar = avatars.Any(a => $"{item.Blockchain.ToUpper()}:{item.Contract.ToUpper()}"
                                                 == $"{a.chain.ToUpper()}:{a.contractAddress.ToUpper()}");
                bool avatarResult = filterParams.avatars || !isAvatar;
                
                if (searchStringResult && blockchainResult && avatarResult)
                {
                    itemEntry.gameObject.SetActive(true);
                }
                else
                {
                    itemEntry.gameObject.SetActive(false);
                }
            }
        }

        private void OnSearchFieldValueChanged(string searchString)
        {
            filterParams.searchString = searchString;
            RefreshFilteredResults();
        }

        private void OnAvatarsToggleValueChanged(bool selected)
        {
            filterParams.avatars = selected;
            RefreshFilteredResults();
        }
        
        private void OnPropsToggleValueChanged(bool selected)
        {
            EmergenceLogger.LogWarning("Prop filtering is currently not implemented");
            filterParams.props = selected;
            RefreshFilteredResults();
        }
        
        private void OnClothingToggleValueChanged(bool selected)
        {
            EmergenceLogger.LogWarning("Clothing filtering is currently not implemented");
            filterParams.clothing = selected;
            RefreshFilteredResults();
        }
        
        private void OnWeaponsToggleValueChanged(bool selected)
        {
            EmergenceLogger.LogWarning("Weapon filtering is currently not implemented");
            filterParams.weapons = selected;
            RefreshFilteredResults();
        }

        private void OnBlockchainDropdownValueChanged(int selection)
        {
            EmergenceLogger.LogInfo(blockchainDropdown.options[selection].text);
            filterParams.blockchain = blockchainDropdown.options[selection].text.ToUpper();
            RefreshFilteredResults();
        }

        public void OnInventoryItemPressed(InventoryItem item)
        {
            OpenSidebar(item);
        }

        public void OpenSidebar(InventoryItem item)
        {
            itemNameText.text = item.Meta.Name;
            itemDescriptionText.text = item.Meta.Description;
            dynamicMetadata.text = "Dynamic metadata: " + item.Meta.DynamicMetadata;

            if (!isItemSelected)
            {
                DG.Tweening.DOTween.To(() => detailsPanel.GetComponent<RectTransform>().anchoredPosition,
                    x => detailsPanel.GetComponent<RectTransform>().anchoredPosition = x, new Vector2(0, 0), 0.25f);

                DG.Tweening.DOTween.To(() => itemsListPanel.GetComponent<RectTransform>().offsetMax,
                    x => itemsListPanel.GetComponent<RectTransform>().offsetMax = x, new Vector2(-443.5f, 0), 0.25f);

                isItemSelected = true;
                selectedItem = item;
            }
        }

        public void OnCloseDetailsPanelButtonPressed() 
        {
            DG.Tweening.DOTween.To(() => detailsPanel.GetComponent<RectTransform>().anchoredPosition,
                x=> detailsPanel.GetComponent<RectTransform>().anchoredPosition = x, new Vector2(Screen.width / 2, 0), 0.25f);
                
            DG.Tweening.DOTween.To(() => itemsListPanel.GetComponent<RectTransform>().offsetMax,
                x=> itemsListPanel.GetComponent<RectTransform>().offsetMax = x, new Vector2(0, 0), 0.25f);
            
            isItemSelected = false;
        }
    }


}
