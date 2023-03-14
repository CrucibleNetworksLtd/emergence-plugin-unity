using System;
using UnityEngine;
using EmergenceSDK;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Avatar = EmergenceSDK.Avatar;

namespace EmergenceDemo
{
    public class DemoSignMessage : MonoBehaviour
    {

        [SerializeField] private GameObject instructions;

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
                SignMessage();
            }
        }

        private void SignMessage() {
            EmergenceServices.Instance.RequestToSign("Test message", message => {
                Debug.Log("Message signed succesfully: " + message);
            }, (message, code) => {
                Debug.LogError("Error signing message: " + message);
            });
        }
    }
}