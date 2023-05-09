using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Services;
using EmergenceSDK.Types.Responses;
using UnityEngine;

namespace EmergenceSDK.Samples.Examples
{
    public class WritingContracts : MonoBehaviour
    {
        [Header("Contract information")]
        // This must be set in the inspector
        public DeployedSmartContract deployedContract;
        // Public string array that is used as input data for the smart contract method
        public string[] body = new string[] { };
        // Public string that is used as input data for the smart contract method
        public string value = "0";

        private IContractService contractService;
        
        public void Awake()
        {
            contractService = EmergenceServices.GetService<IContractService>();
        }

        public void Start()
        {
            WriteContract();
        }
        
        public void WriteContract()
        {
            // Loads the contract using the deployedContract variable and calls the Load method once it's loaded
            contractService.LoadContract(deployedContract.contractAddress, deployedContract.contract.ABI, 
                deployedContract.contract.name, OnLoadSuccess, EmergenceLogger.LogError);
        }

        // This method is called once the contract is loaded
        private void OnLoadSuccess()
        {
            // Creates a ContractInfo object with the smart contract address, method name, network name, and default node URL
            var contractInfo = new ContractInfo(deployedContract.contractAddress, "[METHOD NAME]",
                deployedContract.chain.networkName, deployedContract.chain.DefaultNodeURL);

            // Calls the ReadMethod method to execute the smart contract method defined in the ABI with an empty input parameter
            contractService.WriteMethod<BaseResponse<string>, string[]>(contractInfo, "", "", value, body,
                OnWriteSuccess, EmergenceLogger.LogError);
        }

        private void OnWriteSuccess(BaseResponse<string> response)
        {
            // Logs the response to the console
            Debug.Log($"{response}");
        }
    }
}