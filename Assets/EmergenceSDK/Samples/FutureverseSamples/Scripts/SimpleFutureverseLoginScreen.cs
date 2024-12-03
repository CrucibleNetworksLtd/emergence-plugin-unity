using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Samples.FutureverseSamples
{
    /// <summary>
    /// A sample login implementation showcasing both Futureverse QR-code and custodial logins.
    /// </summary>
    public class FutureverseLoginSample : MonoBehaviour
    {
        [SerializeField]
        private LoginManager loginManager;

        [SerializeField] 
        private GameObject buttonsPanel;
        
        [Header("QR Code Login")]
        // The DisableEmergenceAccessToken login setting will remove the Emergence specific authentication from the login flow.
        [SerializeField]
        private LoginSettings qrLoginSettings = LoginSettings.EnableFuturepass | LoginSettings.DisableEmergenceAccessToken; 
        
        [SerializeField]
        private Button qrLoginButton; // Button to start QR login.

        [SerializeField] 
        private GameObject qrCodePanel; // Container for QR code UI
        
        [SerializeField]
        private RawImage qrCodeImage; // UI element for QR code.

        [SerializeField]
        private TMP_Text qrCodeMessageText; // UI element for custodial login messages.
        
        [Header("Custodial Web Login")]
        [SerializeField]
        private LoginSettings custodialWebLoginSettings = LoginSettings.EnableCustodialLogin | LoginSettings.EnableFuturepass | LoginSettings.DisableEmergenceAccessToken;
        
        [SerializeField]
        private Button custodialWebLoginButton; // Button to start custodial login.
        
        [SerializeField] 
        private GameObject custodialWebLoginPanel; // Container for CustodialWebLogin UI
                
        [SerializeField]
        private TMP_Text custodialWebLoginMessageText; // UI element for custodial login messages.

        private bool isQrCodeLogin = false;
        

        private void Awake()
        {
            // Load the Futureverse service provider. This enables all teh relevant Emergence Services
            EmergenceServiceProvider.Load(ServiceProfile.Futureverse);

            // Set up event listeners.
            loginManager.loginStepUpdatedEvent.AddListener(OnLoginStepUpdated);
            loginManager.loginSuccessfulEvent.AddListener(OnLoginSuccessful);
            loginManager.loginCancelledEvent.AddListener(OnLoginCancelled);
            loginManager.qrCodeTickEvent.AddListener(OnQrCodeTick);

            // Bind button actions.
            qrLoginButton.onClick.AddListener(StartQrCodeLogin);
            custodialWebLoginButton.onClick.AddListener(StartCustodialLogin);
        }

        /// <summary>
        /// Starts the QR-code login process.
        /// </summary>
        private void StartQrCodeLogin()
        {
            isQrCodeLogin = true;
            buttonsPanel.SetActive(false);
            qrCodePanel.SetActive(true);
            UniTask.Void(async () =>
            {
                await loginManager.WaitUntilAvailable(); // Ensure the LoginManager is ready.
                await loginManager.StartLogin(qrLoginSettings); // Start the Futureverse QR login.
            });
        }

        /// <summary>
        /// Starts the custodial login process.
        /// </summary>
        private void StartCustodialLogin()
        {
            isQrCodeLogin = false;
            buttonsPanel.SetActive(false);
            custodialWebLoginPanel.SetActive(true);
            custodialWebLoginMessageText.text = "Please follow instructions in your default browser for authentication...";
            UniTask.Void(async () =>
            {
                await loginManager.WaitUntilAvailable(); // Ensure the LoginManager is ready.
                await loginManager.StartLogin(custodialWebLoginSettings); // Start custodial login.
            });
        }

        /// <summary>
        /// Handles updates during the login process.
        /// </summary>
        private void OnLoginStepUpdated(LoginManager _, LoginStep step, StepPhase phase)
        {
            if (phase != StepPhase.Success) return;

            switch (step)
            {
                case LoginStep.QrCodeRequest:
                    // Update the QR code display.
                    Texture2D qrCodeTexture = loginManager.CurrentQrCode.Texture;
                    qrCodeTexture.filterMode = FilterMode.Point; // Crisp rendering.
                    qrCodeImage.texture = qrCodeTexture;

                    Debug.Log("QR code displayed. User can scan to authenticate.");
                    break;

                case LoginStep.CustodialRequests:
                    custodialWebLoginMessageText.text = "Successfully authenticated.";
                    Debug.Log("Custodial login in progress...");
                    break;
            }
        }

        /// <summary>
        /// Called when the login process is successfully completed.
        /// </summary>
        private void OnLoginSuccessful(LoginManager _, string walletAddress)
        {
            LoginManager.SetFirstLoginFlag();
            if (isQrCodeLogin)
            {
                qrCodeImage.gameObject.SetActive(false);
                qrCodeMessageText.text =($"Login successful. Wallet address: {walletAddress}");
            }
            else
            {
                custodialWebLoginMessageText.text =($"Login successful. Wallet address: {walletAddress}");
            }
        }

        /// <summary>
        /// Called if the login process is cancelled.
        /// </summary>
        private void OnLoginCancelled(LoginManager _)
        {
            Debug.LogWarning("Login process was cancelled.");
            custodialWebLoginMessageText.text = "Login cancelled. Please try again.";
        }

        /// <summary>
        /// Updates the remaining time for the QR code's validity.
        /// </summary>
        private void OnQrCodeTick(LoginManager _, EmergenceQrCode qrCode)
        {
            qrCodeMessageText.text = $"QR code expires in: {qrCode.TimeLeftInt} seconds.";
        }
    }
}
