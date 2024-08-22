using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using UnityEngine;

namespace EmergenceSDK.Samples.CoreSamples
{
    /// <summary>
    /// A minimalist implementation of a login widget. Here to support documentation and guidance on implementing your own LoginManager.<see cref="LoginManager"/><para/>
    /// </summary>
    public class SimpleLoginScreen : MonoBehaviour
    {
        [SerializeField]
        private LoginManager loginManager;
        
        [SerializeField]
        private LoginSettings loginSettings = LoginSettings.Default;
        
        private void Awake()
        {
            // We need to load a service provider profile
            EmergenceServiceProvider.Load(ServiceProfile.Default);
            
            // Add a listener to the loginStepUpdatedEvent. This is triggered as a log in progresses
            loginManager.loginStepUpdatedEvent.AddListener(OnLoginStepUpdated);
            
            // Handle successful login by simply hiding the gameObject and setting the first-login flag to true
            loginManager.loginSuccessfulEvent.AddListener(OnLoginSuccessful);
            
            loginManager.loginStartedEvent.AddListener(OnLoginStarted);
            loginManager.qrCodeTickEvent.AddListener(OnQrCodeTick);
            loginManager.loginCancelledEvent.AddListener(OnLoginCancelled);
        }

        private void StartLogin()
        {
            UniTask.Void(async () =>
            {
                await loginManager.WaitUntilAvailable(); // Wait until the login manager is available
                await loginManager.StartLogin(loginSettings); // Start login
            });
        }

        private void OnLoginStepUpdated(LoginManager _, LoginStep loginStep, StepPhase stepPhase)
        {
            if (stepPhase != StepPhase.Success) return;

            switch (loginStep)
            {
                case LoginStep.QrCodeRequest: 
                    Texture2D qrCodeTexture2D = loginManager.CurrentQrCode.Texture;
                    //Display QR code to User
                    break;
            }
        }

        private void OnLoginSuccessful(LoginManager _,string walletAddress)
        {
            LoginManager.SetFirstLoginFlag();
            Debug.Log($"Wallet address: {walletAddress}");
        }

        private void OnLoginStarted(LoginManager _)
        {

        }

        private void OnLoginCancelled(LoginManager _)
        {
            
        }

        private void OnQrCodeTick(LoginManager _, EmergenceQrCode qrCode)
        {
            
        }
    }
}