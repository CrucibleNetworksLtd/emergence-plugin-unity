using System;
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
            EmergenceSingleton.Instance.OpenEmergenceUI();
            ScreenManager.Instance.ShowCollection(UpdateDynamicMetadata);
        }

        private void UpdateDynamicMetadata(InventoryItem item)
        {
            Debug.Log("Updating Dynamic metadata");
            if (string.IsNullOrEmpty(item.meta.dynamicMetadata)) return;
            var curMetadata = int.Parse(item.meta.dynamicMetadata);
            curMetadata++;

            Services.Instance.WriteDynamicMetadata(item.blockchain, item.contract, item.tokenId, curMetadata.ToString(), (string response) => {}, (string error, long code) => {});
            EmergenceSingleton.Instance.CloseEmergeneUI();
        }
    }
}


