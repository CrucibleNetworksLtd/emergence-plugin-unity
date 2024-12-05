using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.ScriptableObjects;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types.Responses;
using UnityEngine;

namespace EmergenceSDK.Samples.CoreSamples.DemoStations
{
    public class WriteMethod : DemoStation<WriteMethod>, ILoggedInDemoStation
    {
        public DeployedSmartContract deployedContract;
        private static string activeInstruction = "Press 'E' to Write";
        public bool IsReady
        {
            get => isReady;
            set
            {
                InstructionsText.text = value ? activeInstruction : InactiveInstructions;
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
                IncrementCurrentCount();
            }
        }

        private void IncrementCurrentCount()
        {
            var contractInfo = new ContractInfo(deployedContract, "IncrementCount");
            ContractService.WriteMethod(contractInfo, "0", new string[] { }, WriteMethodSuccess, EmergenceLogger.LogError);
        }

        private void WriteMethodSuccess(WriteContractResponse response)
        {
            EmergenceLogger.LogInfo("WriteMethod finished", true);
            //var transactionHash = response.transactionHash;
            ContractService.WriteMethodConfirmed += contractResponse =>
            {
                //if (contractResponse.transactionHash == transactionHash)
                {
                    EmergenceLogger.LogInfo("Transaction confirmed at least 3 times on the blockchain", true);
                }
            };
        }
    }
}