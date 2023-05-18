using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Services;
using EmergenceSDK.Types.Responses;
using UnityEngine;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public class WriteMethod : DemoStation<WriteMethod>, IDemoStation
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
                IncrementCurrentCount();
            }
        }

        private void IncrementCurrentCount()
        {
            var contractInfo = new ContractInfo(deployedContract.contractAddress, "IncrementCount", deployedContract.chain.networkName,
                deployedContract.chain.DefaultNodeURL, deployedContract.contract.ABI);
            ContractService.WriteMethod<BaseResponse<string>>(contractInfo, "", "", "0", "", WriteMethodSuccess, EmergenceLogger.LogError);
        }

        private void WriteMethodSuccess(BaseResponse<string> response)
        {
            Debug.Log("WriteMethod finished");
        }
    }
}