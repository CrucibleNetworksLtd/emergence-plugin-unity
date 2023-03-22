using EmergenceSDK.Services;
using UnityEngine;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public class SignMessage : DemoStation<SignMessage>, IDemoStation
    {
        public bool IsReady
        {
            get => isReady;
            set => InstructionsText.text = value ? ActiveInstructions : InactiveInstructions;
        }
        
        private void Start()
        {
            instructionsGO.SetActive(false);
            IsReady = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            instructionsGO.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            instructionsGO.SetActive(false);
        }

        private void Update()
        {
            if (HasBeenActivated() && IsReady)
            {
                EmergenceServices.Instance.RequestToSign("Test message", SignSuccess, SignErrorCallback);
            }
        }

        private void SignErrorCallback(string message, long code)
        {
            Debug.LogError("Error signing message: " + message);
        }

        private void SignSuccess(string message)
        {
            Debug.Log("Message signed succesfully: " + message);
        }
    }
}