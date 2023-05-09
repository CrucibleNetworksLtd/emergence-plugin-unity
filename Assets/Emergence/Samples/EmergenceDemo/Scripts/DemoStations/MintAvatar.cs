using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Services;
using EmergenceSDK.Types.Responses;
using UnityEngine;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public class MintAvatar : DemoStation<MintAvatar>, IDemoStation
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

        private IContractService ContractService => contractService ??= EmergenceServices.GetService<IContractService>();
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
                ContractService.LoadContract(deployedContract.contractAddress, deployedContract.contract.ABI, deployedContract.contract.name, OnMintSuccess, EmergenceLogger.LogError);
            }
        }
        
        private void OnMintSuccess()
        {
            var contractInfo = new ContractInfo(deployedContract.contractAddress, "mint", deployedContract.contract.name, deployedContract.chain.DefaultNodeURL);
            ContractService.WriteMethod<BaseResponse<string>, string[]>(contractInfo, "", "", "0", new string[] { }, OnWriteSuccess, EmergenceLogger.LogError);
        }

        private void OnWriteSuccess(BaseResponse<string> response)
        {
            Debug.Log("Mint response: " + response.message);
        }
    }
}
