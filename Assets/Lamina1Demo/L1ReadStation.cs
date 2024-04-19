using EmergenceSDK.EmergenceDemo.DemoStations;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Services;
using EmergenceSDK.Types.Responses;
using UnityEngine;

namespace EmergenceSDK.Lamina1Demo
{
    public class L1ReadStation : DemoStation<L1ReadStation>, IDemoStation
    {
        public DeployedSmartContract deployedContract;
        public GameObject trophy;

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
                CheckForMintedL1Token();
            }
        }

        private void CheckForMintedL1Token() 
        {
            
            var contractInfo = new ContractInfo(deployedContract, "addressMints");
            ContractService.ReadMethod(contractInfo, new string[] { EmergenceServiceProvider.GetService<IWalletService>().WalletAddress }, OnReadSuccess, EmergenceLogger.LogError);
        }
        
        private void OnReadSuccess(BaseResponse<string> response)
        {
            EmergenceLogger.LogInfo("Token confirmed as minted", true);
            trophy.SetActive(true);
        }
    }
}