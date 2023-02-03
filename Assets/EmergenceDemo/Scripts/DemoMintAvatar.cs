using System.Collections;
using System.Collections.Generic;
using EmergenceSDK;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EmergenceDemo
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
                MintAvatar();
            }
        }

        private void MintAvatar()
        {
            ContractHelper.LoadContract(deployedContract.contractAddress, deployedContract.contract.ABI,
                deployedContract.contract.name, () =>
                {
                    ContractHelper.WriteMethod<BaseResponse<string>, string[]>(deployedContract.contractAddress, "mint",
                        "", "", deployedContract.chain.networkName, deployedContract.chain.DefaultNodeURL, new string[] { },
                        (response) => { Debug.Log("Mint response: " + response.message); },
                        (message, id) => { Debug.LogError("Error while minting avatar: " + message); });
                }, (message, id) => { Debug.LogError("Error while loading contract: " + message); });
        }
    }
}
