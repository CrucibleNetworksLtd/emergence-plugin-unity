using EmergenceSDK.Internal.UI.Screens;
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
            Debug.Log("Updating Dynamic metadata");
            if (string.IsNullOrEmpty(item.Meta.DynamicMetadata)) 
                return;
            
            var curMetadata = int.Parse(item.Meta.DynamicMetadata);
            curMetadata++;

            EmergenceServices.Instance.WriteDynamicMetadata(item.Blockchain, item.Contract, item.TokenId,
                curMetadata.ToString(), UpdateDynamicMetadataSuccess, (string error, long code) => {});
            
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


