using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime;
using EmergenceSDK.Runtime.Internal.UI;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.ScriptableObjects;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types.Inventory;
using UnityEngine;

namespace EmergenceSDK.Samples.CoreSamples.DemoStations
{
    public class DynamicMetadataController : DemoStation<DynamicMetadataController>, ILoggedInDemoStation
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
            ScreenManager.Instance.ShowCollection();
            CollectionScreen.Instance.OnItemClicked += UpdateDynamicMetadata;
        }

        private class DynamicMetaData
        {
            public int statusCode;
            public string message;
        }
        
        private void UpdateDynamicMetadata(InventoryItem item)
        {
            if (dynamicMetaDataService == null)
            {
                dynamicMetaDataService = EmergenceServiceProvider.GetService<IDynamicMetadataService>();
            }
            EmergenceLogger.LogInfo("Updating Dynamic metadata", true);
            if (!string.IsNullOrEmpty(item.Meta.DynamicMetadata))
            {
                var curMetadata = int.Parse(item.Meta.DynamicMetadata);
                curMetadata++;
                dynamicMetaDataService.WriteDynamicMetadata(
                    item.Blockchain,
                    item.Contract,
                    item.TokenId,
                    curMetadata.ToString(),
                    "0iKoO1V2ZG98fPETreioOyEireDTYwby",
                    UpdateDynamicMetadataSuccess,
                    EmergenceLogger.LogError);
            }
            else
            {
                var curMetadata = 1;
                dynamicMetaDataService.WriteNewDynamicMetadata(
                    item.Blockchain,
                    item.Contract,
                    item.TokenId,
                    curMetadata.ToString(),
                    "0iKoO1V2ZG98fPETreioOyEireDTYwby",
                    UpdateDynamicMetadataSuccess,
                    EmergenceLogger.LogError);
            }

            void UpdateDynamicMetadataSuccess(string response)
            {
                var dynamicMetaData = JsonUtility.FromJson<DynamicMetaData>(response);
                EmergenceLogger.LogInfo($"Dynamic metadata updated: {dynamicMetaData?.message}", true);
                if (dynamicMetaData.statusCode == 0)
                {
                    CollectionScreen.Instance.Refresh().Forget();
                    item.Meta.DynamicMetadata = dynamicMetaData.message;
                    CollectionScreen.Instance.OpenSidebar(item);
                }
            }
        }
    }
}


