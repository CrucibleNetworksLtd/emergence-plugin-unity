using EmergenceSDK.Internal.UI.Screens;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Inventory;
using UnityEngine;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public class DynamicMetadataController : DemoStation<DynamicMetadataController>, IDemoStation
    {
        public DeployedSmartContract deployedContract;
        private IDynamicMetadataService dynamicMetaDataService;

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
            dynamicMetaDataService = EmergenceServices.GetService<IDynamicMetadataService>();
            
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
                ShowNFTPicker();
            }
        }

        private void ShowNFTPicker()
        {
            EmergenceSingleton.Instance.OpenEmergenceUI();
            ScreenManager.Instance.ShowCollection(UpdateDynamicMetadata);
        }

        private class DynamicMetaData
        {
            public int statusCode;
            public string message;
        }
        
        private void UpdateDynamicMetadata(InventoryItem item)
        {
            EmergenceLogger.LogInfo("Updating Dynamic metadata");
            if (string.IsNullOrEmpty(item.Meta.DynamicMetadata)) 
                return;
            
            var curMetadata = int.Parse(item.Meta.DynamicMetadata);
            curMetadata++;

            dynamicMetaDataService.WriteDynamicMetadata(item.Blockchain, item.Contract, item.TokenId,
                curMetadata.ToString(), UpdateDynamicMetadataSuccess, EmergenceLogger.LogError);
            
            void UpdateDynamicMetadataSuccess(string response)
            {
                var dynamicMetaData = JsonUtility.FromJson<DynamicMetaData>(response);
                if (dynamicMetaData.statusCode == 0)
                {
                    CollectionScreen.Instance.OpenSidebar(item);
                    CollectionScreen.Instance.Refresh(null);
                }
            
            }
        }
    }
}


