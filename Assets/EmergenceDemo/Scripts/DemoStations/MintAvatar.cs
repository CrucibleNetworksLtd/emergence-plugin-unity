using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
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
            set => InstructionsText.text = value ? ActiveInstructions : InactiveInstructions;
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
                ContractHelper.LoadContract(deployedContract.contractAddress, deployedContract.contract.ABI,
                                             deployedContract.contract.name, OnMintSuccess, OnMintError);
            }
        }
        
        private void OnMintSuccess()
        {
            var contractInfo = new ContractInfo(deployedContract.contractAddress, "mint", deployedContract.contract.name, deployedContract.chain.DefaultNodeURL);
            ContractHelper.WriteMethod<BaseResponse<string>, string[]>(contractInfo, "", "", new string[] { }, OnWriteSuccess, OnWriteError);
        }

        private void OnMintError(string message, long id)
        {
            Debug.LogError("Error while loading contract: " + message);
        }

        private void OnWriteSuccess(BaseResponse<string> response)
        {
            Debug.Log("Mint response: " + response.message);
        }

        private void OnWriteError(string message, long id)
        {
            Debug.LogError("Error while minting avatar: " + message);
        }
    }
}
