using System.Collections.Generic;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public class ReadMethod : DemoStation<ReadMethod>, IDemoStation
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
                ReadCurrentCount();
            }
        }

        private void ReadCurrentCount()
        {
            ContractService.LoadContract(deployedContract.contractAddress, deployedContract.contract.ABI,
                deployedContract.chain.networkName, OnLoadSuccess, (message, id) => { Debug.LogError("Error while loading contract: " + message); });
        }

        public class ContractResponse
        {
            public List<string> response { get; set; }

            public override string ToString()
            {
                 return string.Join(", ", response);
            }
        }
        
        private void OnLoadSuccess()
        {
            ContractInfo contractInfo = new ContractInfo(deployedContract.contractAddress, "GetCurrentCount", deployedContract.chain.networkName, deployedContract.chain.DefaultNodeURL);
            ContractService.ReadMethod<ContractResponse, string[]>(contractInfo, new string[] { EmergenceSingleton.Instance.GetCachedAddress() },
                ReadMethodSuccess, EmergenceLogger.LogError);
        }

        private void ReadMethodSuccess(ContractResponse response)
        {
            Debug.Log($"ReadContract finished: {response}");
        }
    }
}