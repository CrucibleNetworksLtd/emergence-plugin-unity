using System.Collections;
using System.Collections.Generic;
using EmergenceSDK;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EmergenceDemo
{
    public class DemoDynamicMetadataController : MonoBehaviour
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
                ShowNFTPicker();
            }
        }

        private void ShowNFTPicker()
        {
            Services.Instance.OpenNFTPicker();
        }

        private void UpdateDynamicMetadata()
        {

        }
    }
}