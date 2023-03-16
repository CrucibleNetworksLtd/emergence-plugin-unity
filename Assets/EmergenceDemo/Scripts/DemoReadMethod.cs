using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EmergenceSDK.EmergenceDemo.Scripts
{
    public class DemoReadMethod : DemoStation<DemoReadMethod>, IDemoStation
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
                deployedContract.chain.networkName, OnLoadSuccess, (message, id) => { Debug.LogError("Error while loading contract: " + message); });
        }

        private void OnLoadSuccess()
        {
            ContractInfo contractInfo = new ContractInfo(deployedContract.contractAddress, "GetCurrentCount", deployedContract.chain.networkName, deployedContract.chain.DefaultNodeURL);
            ContractHelper.ReadMethod<BaseResponse<string>, string[]>(contractInfo, new string[] { EmergenceSingleton.Instance.GetCachedAddress() },
                (response) => Debug.Log($"ReadContract finished: {response.message}"), (message, id) => Debug.LogError("Error while getting current count: " + message));
        }

        public bool IsReady { get; set; }
    }
}