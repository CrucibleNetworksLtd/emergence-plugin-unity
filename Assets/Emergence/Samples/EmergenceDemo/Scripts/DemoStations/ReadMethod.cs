using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
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

        private void OnLoadSuccess()
        {
            ContractInfo contractInfo = new ContractInfo(deployedContract.contractAddress, "GetCurrentCount", deployedContract.chain.networkName, deployedContract.chain.DefaultNodeURL);
            ContractService.ReadMethod<BaseResponse<string>, string[]>(contractInfo, new string[] { EmergenceSingleton.Instance.GetCachedAddress() },
                (response) => Debug.Log($"ReadContract finished: {response.message}"), EmergenceLogger.LogError);
        }
    }
}