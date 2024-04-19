using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using UnityEngine;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public class SignMessage : DemoStation<SignMessage>, ILoggedInDemoStation
    {
        private IWalletService walletService;

        public bool IsReady
        {
            get => isReady;
            set
            {
                InstructionsText.text = value ? ActiveInstructions : InactiveInstructions;
                isReady = value;
            }
        }
        
        private void Start()
        {
            walletService = EmergenceServiceProvider.GetService<IWalletService>();
            
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
                var message = "Test message";
                walletService.RequestToSign(message, (signedMessage) =>
                {
                    SignSuccess(message, signedMessage);
                }, SignErrorCallback);
            }
        }

        private void SignErrorCallback(string message, long code)
        {
            EmergenceLogger.LogError("Error signing message: " + message, true);
        }

        private void SignSuccess(string message, string signedMessage)
        {
            EmergenceLogger.LogInfo("Message signed succesfully: " + signedMessage, true);
            EmergenceLogger.LogInfo("Validating message...", true);

            walletService.ValidateSignedMessage(message, signedMessage, walletService.WalletAddress, isValid =>
            {
                if (isValid)
                {
                    EmergenceLogger.LogInfo("Message is valid", true);
                }
                else
                {
                    EmergenceLogger.LogWarning("Message is not valid", true);
                }
            }, (_, _) =>
            {
                EmergenceLogger.LogError("Error validating message", true);
            });
        }
    }
}