using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EmergenceSDK.EmergenceDemo.Scripts
{
    public class DemoReadMethod : MonoBehaviour
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
                ReadCurrentCount();
            }
        }

        private void ReadCurrentCount()
        {
            ContractHelper.LoadContract(deployedContract.contractAddress, deployedContract.contract.ABI,
                deployedContract.chain.networkName, () =>
                {
                    ContractHelper.ReadMethod<BaseResponse<string>, string[]>(deployedContract.contractAddress,
                        "GetCurrentCount", deployedContract.chain.networkName, deployedContract.chain.DefaultNodeURL, new string[] { EmergenceSingleton.Instance.GetCachedAddress() },
                        (response) => { Debug.Log("ReadContract finished"); },
                        (message, id) => { Debug.LogError("Error while getting current count: " + message); });
                }, (message, id) => { Debug.LogError("Error while loading contract: " + message); });
        }
    }
}