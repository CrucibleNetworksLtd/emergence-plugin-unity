using System.Collections.Generic;
using EmergenceSDK.EmergenceDemo.DemoStations;
using EmergenceSDK.Integrations.Futureverse.Services;
using EmergenceSDK.Internal.UI.Inventory;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Inventory;
using Tweens;
using UnityEngine;

namespace EmergenceSDK.Samples.FutureverseTestScene.DemoStations
{
    public class FutureverseInventoryDemo : DemoStation<FutureverseInventoryDemo>, ILoggedInDemoStation
    {
        private IFutureverseService futureverseService;
        private ISessionService sessionService;
        [SerializeField] private GameObject itemEntryPrefab;
        [SerializeField] private GameObject contentGO;
        [SerializeField] private GameObject scrollView;

        private bool isInventoryVisible;
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
            futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
            sessionService = EmergenceServiceProvider.GetService<ISessionService>();
            inventoryService = EmergenceServiceProvider.GetService<IInventoryService>();
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
            if (HasBeenActivated() && IsReady && sessionService.HasLoginSetting(LoginSettings.EnableFuturepass))
            {
                ShowInventory();
            }
            else if (IsReady && !sessionService.HasLoginSetting(LoginSettings.EnableFuturepass))
            {
                InstructionsText.text = "You must connect with Futurepass";
            }
            else if (IsReady && sessionService.HasLoginSetting(LoginSettings.EnableFuturepass))
            {
                InstructionsText.text = ActiveInstructions;
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