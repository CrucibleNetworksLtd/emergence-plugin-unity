using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Implementations.Login;
using EmergenceSDK.Implementations.Login.Exceptions;
using EmergenceSDK.Implementations.Login.Types;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Integrations.Futureverse.Services;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI.Screens
{
    public class LogInScreen : MonoBehaviour
    {
        [Header("Log-in Manager")]
        public LoginManager loginManager;        
        [FormerlySerializedAs("rawQRImage")] [Header("UI References")]
        public RawImage rawQrImage;
        public Button backButton;
        public TextMeshProUGUI refreshCounterText;
        public TextMeshProUGUI refreshText;
        
        [Header("Sub Screens")]
        public GameObject qrScreen;
        public GameObject futureverseScreen;
        public GameObject startupScreen;
        
        [FormerlySerializedAs("LoginWithFV")] [Header("Futureverse")]
        public Button loginWithFv;
        [FormerlySerializedAs("LoginWithWC")] public Button loginWithWc;
        [FormerlySerializedAs("CreateFPass")] public Button createFPass;
        [FormerlySerializedAs("RetryFPassCheck")] public Button retryFPassCheck;

        private void SetTimeRemainingText(LoginManager _, EmergenceQrCode emergenceQrCode) => refreshCounterText.text = emergenceQrCode.TimeLeftInt.ToString("0");

        
        public static LogInScreen Instance;
        
        private static IWalletServiceInternal WalletServiceInternal => EmergenceServiceProvider.GetService<IWalletServiceInternal>();
        private void Awake()
        {
            Instance = this;
            
            loginWithFv.onClick.AddListener(LoginWithFvClicked);
            loginWithWc.onClick.AddListener(LoginWithWcClicked);
            backButton.onClick.AddListener(() => loginManager.CancelLogin());

            createFPass.onClick.AddListener(CreateFPassClicked);
            retryFPassCheck.onClick.AddListener(RetryFPassCheckClicked);
            
            loginManager.qrCodeTickEvent.AddListener(SetTimeRemainingText);
            loginManager.loginStartedEvent.AddListener(HandleLoginStarted);
            loginManager.loginCancelledEvent.AddListener((_) =>
            {
                Restart();
            });
            loginManager.loginEndedEvent.AddListener((_) =>
            {
                SetLoginButtonsInteractable(true);
            });
            
            loginManager.loginFailedEvent.AddListener((_, exceptionContainer) =>
            {
                HideAllScreens();
                if (exceptionContainer.Exception is FuturepassRequestFailedException or FuturepassInformationRequestFailedException)
                {
                    exceptionContainer.HandleException();
                    futureverseScreen.SetActive(true);
                }
                else
                {
                    startupScreen.SetActive(true);
                }
            });
            
            loginManager.loginStepUpdatedEvent.AddListener((_, loginStep, stepPhase) =>
            {
                if (stepPhase != StepPhase.Success) return;
                
                switch (loginStep)
                {
                    case LoginStep.QrCodeRequest:
                        var texture2D = loginManager.CurrentQrCode.Texture;
                        texture2D.filterMode = FilterMode.Point;
                        rawQrImage.texture = texture2D;
                        refreshText.text = "QR expires in:";
                        break;
                    case LoginStep.HandshakeRequest:
                        HeaderScreen.Instance.Refresh(((IWalletService)WalletServiceInternal).ChecksummedWalletAddress);
                        HeaderScreen.Instance.Show();
                        break;
                    case LoginStep.AccessTokenRequest:
                        // Nothing to do here in this case
                        break;
                    case LoginStep.FuturepassRequests:
                        // Nothing to do here in this case
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(loginStep), loginStep, null);
                }
            });
            
            loginManager.loginSuccessfulEvent.AddListener((_, _) =>
            {
                PlayerPrefs.SetInt(StaticConfig.HasLoggedInOnceKey, 1);
                ScreenManager.Instance.ShowDashboard().Forget();
            });
        }

        private void HandleLoginStarted(LoginManager _)
        {
            rawQrImage.texture = null;
            HideAllScreens();
            qrScreen.SetActive(true);
            refreshCounterText.text = "";
            refreshText.text = "Retrieving QR code...";
        }

        private void HideAllScreens()
        {
            qrScreen.SetActive(false);
            futureverseScreen.SetActive(false);
            startupScreen.SetActive(false);
        }
        
        private void LoginWithFvClicked()
        {
            SetLoginButtonsInteractable(false);
            UniTask.Void(async () =>
            {
                await loginManager.WaitUntilAvailable();
                await loginManager.StartLogin(LoginMode.Futurepass);
            });
        }

        private void SetLoginButtonsInteractable(bool interactable)
        {
            loginWithFv.interactable = interactable;
            loginWithWc.interactable = interactable;
        }

        private void LoginWithWcClicked()
        {
            SetLoginButtonsInteractable(false);
            UniTask.Void(async () =>
            {
                await loginManager.WaitUntilAvailable();
                await loginManager.StartLogin(LoginMode.WalletConnect);
            });
        }
        
        private void CreateFPassClicked()
        {
            Application.OpenURL("https://futurepass.futureverse.app/");
        }

        private void RetryFPassCheckClicked()
        {
            Restart();
        }

        public void Restart()
        {
            HideAllScreens();
            startupScreen.SetActive(true);
        }
    }
}
