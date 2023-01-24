using System.Collections;
using System.Collections.Generic;
using EmergenceSDK;
using UnityEngine;
using UnityEngine.InputSystem;

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
        ContractHelper.LoadContract(deployedContract.contractAddress, deployedContract.contract.ABI, () =>
        {
            ContractHelper.ReadMethod<ReadContractTokenURIResponse, string[]>(deployedContract.contractAddress, "GetCurrentCount", new string[] { EmergenceSingleton.Instance.GetCachedAddress() },
                (response) =>
                {
                    Debug.Log("Current count: " + response.response);
                }, (message, id) =>
                {
                    Debug.LogError("Error while getting current count: " + message);
                });
        }, (message, id) =>
        {
            Debug.LogError("Error while loading contract: " + message);
        });
    }
}
