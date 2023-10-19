using System.Collections.Generic;
using EmergenceSDK.Internal.UI;
using EmergenceSDK.Internal.UI.Inventory;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Inventory;
using UnityEngine;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public class InventoryDemo : DemoStation<InventoryDemo>, IDemoStation
    {
        [SerializeField] private GameObject itemEntryPrefab;
        [SerializeField] private GameObject contentGO;
        [SerializeField] private GameObject scrollView;

        private bool isInventoryVisible = false;
        private IInventoryService inventoryService;
        
        private InventoryItemStore inventoryItemStore;

        public bool IsReady
        {
            get => isReady;
            set
            {
                InstructionsText.text = value ? ActiveInstructions : InactiveInstructions;
                isReady = value;
            }
        }

        private void Start()
        {
            inventoryService = EmergenceServices.GetService<IInventoryService>();
            inventoryItemStore = new InventoryItemStore();
            
            instructionsGO.SetActive(false);
            IsReady = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            instructionsGO.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            instructionsGO.SetActive(false);
        }

        private void Update()
        {
            if (HasBeenActivated() && IsReady)
            {
                ShowInventory();
            }
        }

        private GameObject CreateEntry() => Instantiate(itemEntryPrefab, contentGO.transform, false);

        public void ShowInventory()
        {
            if (!isInventoryVisible)
            {
                DG.Tweening.DOTween.To(() => scrollView.GetComponent<RectTransform>().anchoredPosition,
                    x => scrollView.GetComponent<RectTransform>().anchoredPosition = x, new Vector2(0, 0), 0.25f);
                isInventoryVisible = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                DG.Tweening.DOTween.To(() => scrollView.GetComponent<RectTransform>().anchoredPosition,
                    x => scrollView.GetComponent<RectTransform>().anchoredPosition = x, new Vector2(0, -200f), 0.25f);
                isInventoryVisible = false;
                Cursor.visible = false;
            }

            inventoryService.InventoryByOwner(EmergenceSingleton.Instance.GetCachedAddress(), InventoryChain.AnyCompatible, SuccessInventoryByOwner, EmergenceLogger.LogError);
        }
        
        private void SuccessInventoryByOwner(List<InventoryItem> inventoryItems)
        {
            inventoryItemStore.SetItems(inventoryItems);
            foreach (var inventoryItem in inventoryItems)
            {
                var entry = CreateEntry();
                entry.GetComponent<InventoryItemEntry>().SetItem(inventoryItem);
            }
        }
    }
}