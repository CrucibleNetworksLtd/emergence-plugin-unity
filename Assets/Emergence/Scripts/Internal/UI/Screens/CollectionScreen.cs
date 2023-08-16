using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.UI.Inventory;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Avatar = EmergenceSDK.Types.Avatar;

namespace EmergenceSDK.Internal.UI.Screens
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
        public TMP_InputField searchInputField;
        public Toggle avatarsToggle;
        public Toggle propsToggle;
        public Toggle clothingToggle;
        public Toggle weaponsToggle;
        public TMP_Dropdown blockchainDropdown;
        public Button PreviousPageButton;
        public Button NextPageButton;

        private bool isItemSelected = false;
        private InventoryItem selectedItem;

        private List<Avatar> avatars = new List<Avatar>();

        private CollectionScreenFilterParams collectionScreenFilterParams = new CollectionScreenFilterParams();
        
        public event Action<InventoryItem> OnItemClicked;
        
        private InventoryItemStore inventoryItemStore;
        private InventoryItemUIManager inventoryItemUIManager;

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
            
            PreviousPageButton.onClick.AddListener(OnPreviousPageButtonPressed);
            PreviousPageButton.interactable = false;
            NextPageButton.onClick.AddListener(OnNextPageButtonPressed);
            NextPageButton.interactable = false;
            
            inventoryItemStore = new InventoryItemStore();
            inventoryItemUIManager = new InventoryItemUIManager(InstantiateItemEntry, inventoryItemStore);
        }

        private void OnNextPageButtonPressed()
        {
            inventoryItemUIManager.NextPage();
            Refresh().Forget();
        }

        private void OnPreviousPageButtonPressed()
        {
            inventoryItemUIManager.PreviousPage();
            Refresh().Forget();
        }

        private GameObject InstantiateItemEntry() => Instantiate(itemEntryPrefab, contentGO.transform, false);
        
        public async UniTask Refresh()
        {
            var inventoryService = EmergenceServices.GetService<IInventoryService>();
            var updatedInventory = await inventoryService.InventoryByOwnerAsync(EmergenceSingleton.Instance.GetCachedAddress(), InventoryChain.AnyCompatible);
            inventoryItemStore.SetItems(updatedInventory.Result);
            if (updatedInventory.Success)
            {
                UpdateInventoryItemListeners();
            }
            Modal.Instance.Hide();
             
            var avatarService = EmergenceServices.GetService<IAvatarService>();
            var updatedAvatars = await avatarService.AvatarsByOwnerAsync(EmergenceSingleton.Instance.GetCachedAddress());
            if (updatedAvatars.Success)
            {
                avatars = updatedAvatars.Result;
            }
            Modal.Instance.Hide();
            
            NextPageButton.interactable = inventoryItemUIManager.IsNextPageAvailable;
            PreviousPageButton.interactable = inventoryItemUIManager.IsPreviousPageAvailable;
        }
        
        private void UpdateInventoryItemListeners()
        {
            Modal.Instance.Show("Retrieving inventory items...");
            
            inventoryItemUIManager.UpdateDisplayItems();

            foreach (var entry in inventoryItemUIManager.GetAllEntries())
            {
                Button entryButton = entry.GetComponent<Button>();
                InventoryItem item = entry.Item;
                entryButton.onClick.RemoveAllListeners();
                entryButton.onClick.AddListener(() => OnInventoryItemPressed(item));
                entryButton.onClick.AddListener(() => OnItemClicked?.Invoke(item));
            }
        }

        private void RefreshFilteredResults()
        {
            foreach (var itemEntry in inventoryItemUIManager.GetAllEntries())
            {
                var item = itemEntry.Item;
                // Search string filter
                string itemName = item.Meta?.Name.ToLower();
                bool searchStringResult = string.IsNullOrEmpty(itemName) || itemName.StartsWith(collectionScreenFilterParams.searchString.ToLower()) || string.IsNullOrEmpty(collectionScreenFilterParams.searchString);

                // Blockchain filter
                string itemBlockchain = item.Blockchain;
                bool blockchainResult = collectionScreenFilterParams.blockchain.Equals("ANY") || itemBlockchain.Equals(collectionScreenFilterParams.blockchain);
                
                //Avatar filter
                bool isAvatar = avatars.Any(a => $"{item.Blockchain.ToUpper()}:{item.Contract.ToUpper()}" == $"{a.chain.ToUpper()}:{a.contractAddress.ToUpper()}");
                bool avatarResult = collectionScreenFilterParams.avatars || !isAvatar;
                
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
            collectionScreenFilterParams.searchString = searchString;
            RefreshFilteredResults();
        }

        private void OnAvatarsToggleValueChanged(bool selected)
        {
            collectionScreenFilterParams.avatars = selected;
            RefreshFilteredResults();
        }
        
        private void OnPropsToggleValueChanged(bool selected)
        {
            EmergenceLogger.LogWarning("Prop filtering is currently not implemented");
            collectionScreenFilterParams.props = selected;
            RefreshFilteredResults();
        }
        
        private void OnClothingToggleValueChanged(bool selected)
        {
            EmergenceLogger.LogWarning("Clothing filtering is currently not implemented");
            collectionScreenFilterParams.clothing = selected;
            RefreshFilteredResults();
        }
        
        private void OnWeaponsToggleValueChanged(bool selected)
        {
            EmergenceLogger.LogWarning("Weapon filtering is currently not implemented");
            collectionScreenFilterParams.weapons = selected;
            RefreshFilteredResults();
        }

        private void OnBlockchainDropdownValueChanged(int selection)
        {
            EmergenceLogger.LogInfo(blockchainDropdown.options[selection].text);
            collectionScreenFilterParams.blockchain = blockchainDropdown.options[selection].text.ToUpper();
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
