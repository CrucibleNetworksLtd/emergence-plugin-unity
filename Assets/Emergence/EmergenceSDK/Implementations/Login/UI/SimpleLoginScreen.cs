using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Implementations.Login.Exceptions;
using EmergenceSDK.Implementations.Login.Types;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Internal.UI.Screens;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Implementations.Login.UI
{
    public class SimpleLoginScreen : MonoBehaviour
    {
        public LoginManager loginManager;
        public LoginMode loginMode;
        public RawImage rawImage;
        public TextMeshProUGUI countdownLabel;
        public Button cancelButton;
        private void SetTimeRemainingText(LoginManager _, EmergenceQrCode qrCode) => countdownLabel.text = "Expires in: " + qrCode.TimeLeftInt.ToString("0");
        private static IWalletServiceInternal WalletServiceInternal => EmergenceServiceProvider.GetService<IWalletServiceInternal>();

        private void Awake()
        {
            cancelButton.onClick.AddListener(loginManager.CancelLogin);
            loginManager.qrCodeTickEvent.AddListener(SetTimeRemainingText);
            loginManager.loginStartedEvent.AddListener(HandleLoginStarted);
            loginManager.loginCancelledEvent.AddListener((_) => { gameObject.SetActive(false); });
            loginManager.loginFailedEvent.AddListener((manager, container) =>
            {
                HandleLoginErrors(manager, container);
                gameObject.SetActive(false);
                gameObject.SetActive(true);
            });

            loginManager.loginStepUpdatedEvent.AddListener((_, loginStep, stepPhase) =>
            {
                if (stepPhase != StepPhase.Success) return;

                switch (loginStep)
                {
                    case LoginStep.QrCodeRequest:
                        var texture2D = loginManager.CurrentQrCode.Texture;
                        texture2D.filterMode = FilterMode.Point;
                        rawImage.texture = texture2D;
                        break;
                    case LoginStep.HandshakeRequest:
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
                gameObject.SetActive(false);
                PlayerPrefs.SetInt(StaticConfig.HasLoggedInOnceKey, 1);
            });
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HandleLoginErrors(LoginManager _, LoginExceptionContainer exceptionContainer)
        {
            var e = exceptionContainer.Exception;
            if (e is FuturepassRequestFailedException or FuturepassInformationRequestFailedException)
            {
                exceptionContainer.HandleException();
                EmergenceLogger.LogWarning(e.GetType().FullName + ": " + e.Message);
            }
            else if (e
                     is FuturepassRequestFailedException
                     or FuturepassInformationRequestFailedException
                     or TokenRequestFailedException
                     or HandshakeRequestFailedException
                     or QrCodeRequestFailedException)
            {
                exceptionContainer.HandleException();
                EmergenceLogger.LogWarning(e.GetType().FullName + ": " + e.Message);
                if (e.InnerException != null)
                {
                    EmergenceLogger.LogWarning("\t" + e.InnerException.GetType().FullName + ": " + e.InnerException.Message);
                }
            }
        }
        
        private void HandleLoginStarted(LoginManager _)
        {
            rawImage.texture = null;
            countdownLabel.text = "Retrieving...";
        }

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            UniTask.Void(async () =>
            {
                await loginManager.WaitUntilAvailable();
                await loginManager.StartLogin(loginMode);
            });
        }
    }
}