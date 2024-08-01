using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.ScriptableObjects;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types.Responses;
using UnityEngine;

namespace EmergenceSDK.Samples.CoreSamples.DemoStations
{
    public class MintAvatar : DemoStation<MintAvatar>, ILoggedInDemoStation
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
                var contractInfo = new ContractInfo(deployedContract, "mint");
                ContractService.WriteMethod(contractInfo, "0", new string[] { }, OnWriteSuccess, EmergenceLogger.LogError);
            }
        }
        
        private void OnWriteSuccess(BaseResponse<string> response)
        {
            EmergenceLogger.LogInfo("Mint response: " + response.message, true);
        }
    }
}
