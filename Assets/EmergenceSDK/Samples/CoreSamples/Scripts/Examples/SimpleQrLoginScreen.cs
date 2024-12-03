using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Samples.CoreSamples
{
    /// <summary>
    /// A simple implementation of QR-code-only login for demonstration purposes.
    /// </summary>
    public class SimpleQrLoginScreen : MonoBehaviour
    {
        [SerializeField]
        private LoginManager loginManager;
        
        [SerializeField]
        private LoginSettings loginSettings = LoginSettings.Default;

        [SerializeField]
        private RawImage qrCodeDisplay; // UI element to display the QR code.

        [SerializeField] private TMP_Text qrCodeText = null;

        private void Awake()
        {
            // Load the default service provider for the login process.
            EmergenceServiceProvider.Load(ServiceProfile.Default);

            // Set up event listeners for login process steps.
            loginManager.loginStepUpdatedEvent.AddListener(OnLoginStepUpdated);
            loginManager.loginSuccessfulEvent.AddListener(OnLoginSuccessful);
            loginManager.loginCancelledEvent.AddListener(OnLoginCancelled);
            loginManager.qrCodeTickEvent.AddListener(OnQrCodeTick);
            StartLogin();
        }

        /// <summary>
        /// Starts the login process for QR-code authentication.
        /// </summary>
        public void StartLogin()
        {
            UniTask.Void(async () =>
            {
                await loginManager.WaitUntilAvailable(); // Ensure LoginManager is ready.
                await loginManager.StartLogin(loginSettings); // Start login with specified settings.
            });
        }

        /// <summary>
        /// Handles updates during the login process.
        /// </summary>
        private void OnLoginStepUpdated(LoginManager _, LoginStep step, StepPhase phase)
        {
            if (phase != StepPhase.Success) return;

            if (step == LoginStep.QrCodeRequest)
            {
                // Display the QR code texture in the assigned UI RawImage.
                Texture2D qrCodeTexture = loginManager.CurrentQrCode.Texture;
                qrCodeTexture.filterMode = FilterMode.Point; // Ensure a crisp QR display.
                qrCodeDisplay.texture = qrCodeTexture;

                Debug.Log("QR code displayed. User can scan to authenticate.");
            }
        }

        /// <summary>
        /// Called when the login is successfully completed.
        /// </summary>
        private void OnLoginSuccessful(LoginManager _, string walletAddress)
        {
            LoginManager.SetFirstLoginFlag();
            qrCodeText.text = ($"Login successful. Wallet address: {walletAddress}");
            qrCodeDisplay.gameObject.SetActive(false); // Hide the login screen.
        }

        /// <summary>
        /// Called if the login process is cancelled.
        /// </summary>
        private void OnLoginCancelled(LoginManager _)
        {
            Debug.LogWarning("Login process was cancelled.");
        }

        /// <summary>
        /// Updates the remaining time for the QR code's validity.
        /// </summary>
        private void OnQrCodeTick(LoginManager _, EmergenceQrCode qrCode)
        {
            qrCodeText.text = ($"QR code expires in: {qrCode.TimeLeftInt} seconds.");
        }
    }
}
