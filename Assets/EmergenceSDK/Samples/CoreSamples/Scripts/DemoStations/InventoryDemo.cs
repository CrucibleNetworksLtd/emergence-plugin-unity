using System.Collections.Generic;
using EmergenceSDK.Runtime.Internal.UI;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types.Inventory;
using Tweens;
using UnityEngine;

namespace EmergenceSDK.Samples.CoreSamples.DemoStations
{
    public class InventoryDemo : DemoStation<InventoryDemo>, ILoggedInDemoStation
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
            EmergenceServiceProvider.OnServicesLoaded += _ => inventoryService = EmergenceServiceProvider.GetService<IInventoryService>();
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
                scrollView.AddTween(new AnchoredPositionTween() {
                    to = Vector2.zero,
                    duration = .25f
                });
                isInventoryVisible = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                scrollView.AddTween(new AnchoredPositionTween() {
                    to = new Vector2(0, -200f),
                    duration = .25f
                });
                isInventoryVisible = false;
                Cursor.visible = false;
            }

            inventoryService.InventoryByOwner(EmergenceServiceProvider.GetService<IWalletService>().WalletAddress, InventoryChain.AnyCompatible, SuccessInventoryByOwner, EmergenceLogger.LogError);
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