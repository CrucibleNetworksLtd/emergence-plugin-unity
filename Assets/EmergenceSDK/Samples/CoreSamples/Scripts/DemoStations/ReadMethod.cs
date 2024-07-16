using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.ScriptableObjects;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types.Responses;
using UnityEngine;

namespace EmergenceSDK.Samples.CoreSamples.DemoStations
{
    public class ReadMethod : DemoStation<ReadMethod>, ILoggedInDemoStation
    {
        public DeployedSmartContract deployedContract;
        private static string activeInstruction = "Press 'E' to Read";

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
                ReadCurrentCount();
            }
        }

        private void ReadCurrentCount()
        {
            ContractInfo contractInfo = new ContractInfo(deployedContract, "GetCurrentCount");
            ContractService.ReadMethod<string[]>(contractInfo, new string[] { EmergenceServiceProvider.GetService<IWalletService>().WalletAddress }, ReadMethodSuccess, EmergenceLogger.LogError);
        }

        public class ContractResponse
        {
            public List<string> response { get; set; }

            public override string ToString()
            {
                 return string.Join(", ", response);
            }
        }
        
        private void ReadMethodSuccess(ReadContractResponse response)
        {
            EmergenceLogger.LogInfo($"ReadContract finished: {response.response.First()}", true);
        }
    }
}