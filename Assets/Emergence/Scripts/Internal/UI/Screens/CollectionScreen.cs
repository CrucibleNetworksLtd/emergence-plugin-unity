using System;
using System.Collections.Generic;
using System.Linq;
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
        private class InventoryUIItem
        {
            public Guid id;
            public InventoryItem inventoryItem;
            public GameObject entryGo;

            public InventoryUIItem()
            {
                id = Guid.NewGuid();
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

        private GenericPool<InventoryUIItem> items = new GenericPool<InventoryUIItem>();
        private List<Avatar> avatars = new List<Avatar>();

        private FilterParams filterParams = new FilterParams();
        private IInventoryService inventoryService;
        private IAvatarService avatarService;
        
        public event Action<InventoryItem> OnItemClicked;

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
        }

        public void Refresh()
        {
            inventoryService = EmergenceServices.GetService<IInventoryService>();
            inventoryService.InventoryByOwner(EmergenceSingleton.Instance.GetCachedAddress(), InventoryChain.AnyCompatible, InventoryByOwnerSuccess, InventoryRefreshErrorCallback);

            avatarService = EmergenceServices.GetService<IAvatarService>();
            avatarService.AvatarsByOwner(EmergenceSingleton.Instance.GetCachedAddress(), AvatarsByOwnerSuccess, InventoryRefreshErrorCallback);
        }
        
        private void InventoryByOwnerSuccess(List<InventoryItem> inventoryItems)
        {
            foreach (var item in items)
            {
                Destroy(item.entryGo);
            }
            items.ReturnAllUsedObjects();

            EmergenceLogger.LogInfo("Received items: " + inventoryItems.Count);
            Modal.Instance.Show("Retrieving inventory items...");

            var displayItems = inventoryItems.Where(item => item.Meta != null).ToList();
            for (int i = 0; i < displayItems.Count; i++)
            {
                GameObject entry = Instantiate(itemEntryPrefab, contentGO.transform, false);

                var newObject = items.GetNewObject();
                newObject.entryGo = entry;
                newObject.inventoryItem = displayItems[i];

                Button entryButton = entry.GetComponent<Button>();
                InventoryItem item = displayItems[i];
                entryButton.onClick.AddListener(() => OnInventoryItemPressed(item));
                entryButton.onClick.AddListener(() => OnItemClicked?.Invoke(item));

                InventoryItemEntry itemEntry = entry.GetComponent<InventoryItemEntry>();
                itemEntry.SetItem(displayItems[i]);
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
            foreach (var item in items)
            {
                //TODO: implement MVC based long term code here
                
                // Search string filter
                string itemName = item.inventoryItem.Meta?.Name.ToLower();
                bool searchStringResult = string.IsNullOrEmpty(itemName) || itemName.StartsWith(filterParams.searchString.ToLower()) || string.IsNullOrEmpty(filterParams.searchString);

                // Blockchain filter
                string itemBlockchain = item.inventoryItem.Blockchain;
                bool blockchainResult = filterParams.blockchain.Equals("ANY") || itemBlockchain.Equals(filterParams.blockchain);
                
                //Avatar filter
                bool isAvatar = avatars.Any(a => $"{item.inventoryItem.Blockchain.ToUpper()}:{item.inventoryItem.Contract.ToUpper()}"
                                                 == $"{a.chain.ToUpper()}:{a.contractAddress.ToUpper()}");
                bool avatarResult = filterParams.avatars || !isAvatar;
                
                if (searchStringResult && blockchainResult && avatarResult)
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
