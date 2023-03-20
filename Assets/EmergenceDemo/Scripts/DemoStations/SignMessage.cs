using EmergenceSDK.Services;
using UnityEngine;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public class SignMessage : DemoStation<SignMessage>, IDemoStation
    {
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

        public bool IsReady { get; set; }
    }
}