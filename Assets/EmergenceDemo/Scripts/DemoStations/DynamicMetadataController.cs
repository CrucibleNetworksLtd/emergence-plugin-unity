using EmergenceSDK.Internal.UI.Screens;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public class DynamicMetadataController : DemoStation<DynamicMetadataController>, IDemoStation
    {
        public DeployedSmartContract deployedContract;

        private void Start()
        {
            instructions.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            instructions.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            instructions.SetActive(false);
        }

        private void Update()
        {
            if (HasBeenActivated() && IsReady)
            {
                ShowNFTPicker();
            }
        }

        private void ShowNFTPicker()
        {
            EmergenceSingleton.Instance.OpenEmergenceUI();
            ScreenManager.Instance.ShowCollection(UpdateDynamicMetadata);
        }

        private void UpdateDynamicMetadata(InventoryItem item)
        {
            Debug.Log("Updating Dynamic metadata");
            if (string.IsNullOrEmpty(item.meta.dynamicMetadata)) return;
            var curMetadata = int.Parse(item.meta.dynamicMetadata);
            curMetadata++;

            EmergenceServices.Instance.WriteDynamicMetadata(item.blockchain, item.contract, item.tokenId, curMetadata.ToString(), (string response) => {}, (string error, long code) => {});
            EmergenceSingleton.Instance.CloseEmergeneUI();
        }

        public bool IsReady { get; set; }
    }
}


