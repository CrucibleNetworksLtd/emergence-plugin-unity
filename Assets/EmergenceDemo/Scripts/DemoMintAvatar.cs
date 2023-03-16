using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EmergenceSDK.EmergenceDemo.Scripts
{
    public class DemoMintAvatar : MonoBehaviour
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
                ContractHelper.LoadContract(deployedContract.contractAddress, deployedContract.contract.ABI,
                                             deployedContract.contract.name, OnMintSuccess, OnMintError);
            }
        }
        
        private void OnMintSuccess()
        {
            var contractInfo = new ContractInfo(deployedContract.contractAddress, deployedContract.contract.ABI, deployedContract.contract.name, deployedContract.chain.DefaultNodeURL);
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
