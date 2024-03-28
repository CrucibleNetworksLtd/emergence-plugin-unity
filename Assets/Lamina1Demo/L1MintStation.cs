using EmergenceSDK.EmergenceDemo.DemoStations;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Services;
using EmergenceSDK.Types.Responses;
using UnityEngine;

namespace EmergenceSDK.Lamina1Demo
{
    public class L1MintStation : DemoStation<L1MintStation>, IDemoStation
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
        
        private IContractService ContractService => contractService ??= EmergenceServiceProvider.GetService<IContractService>();
        private IContractService contractService;

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
                MintL1Token();
            }
        }

        private void MintL1Token()
        {
            
            var contractInfo = new ContractInfo(deployedContract, "mint");
            
            ContractService.WriteMethod(contractInfo, "", "", "1000000000000000000", new string[] { }, OnWriteSuccess, EmergenceLogger.LogError);
        }
        
        private void OnWriteSuccess(BaseResponse<string> response)
        {
            EmergenceLogger.LogInfo("Mint called successfully", true);
        }
    }
}
