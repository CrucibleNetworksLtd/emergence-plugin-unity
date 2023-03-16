using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EmergenceSDK.EmergenceDemo.Scripts
{
    public class DemoWriteMethod : MonoBehaviour
    {
        public GameObject instructions;
        public DeployedSmartContract deployedContract;

        private void Start()
        {
            instructions.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            instructions.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            instructions.SetActive(false);
        }

        private void Update()
        {
            if (Keyboard.current.eKey.wasPressedThisFrame && instructions.activeSelf)
            {
                IncrementCurrentCount();
            }
        }

        private void IncrementCurrentCount()
        {
            ContractHelper.LoadContract(deployedContract.contractAddress, deployedContract.contract.ABI, deployedContract.chain.networkName, 
                OnLoadContractSuccess, (message, id) => Debug.LogError("Error while loading contract: " + message));
        }

        private void OnLoadContractSuccess()
        {
            var contractInfo = new ContractInfo(deployedContract.contractAddress, "IncrementCount", deployedContract.chain.networkName, deployedContract.chain.DefaultNodeURL);
            ContractHelper.WriteMethod<BaseResponse<string>, string[]>(contractInfo, "", "", new string[] { },
                (response) => Debug.Log("WriteMethod finished"), (message, id) => Debug.LogError("Error while incrementing current count: " + message));
        }
    }
}