using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI.Screens
{
    public class LogInScreen : MonoBehaviour
    {
        [Header("UI References")]
        public RawImage rawQRImage;
        public Button backButton;
        public TextMeshProUGUI refreshCounterText;

        private readonly float QRRefreshTimeOut = 60.0f;
        public static LogInScreen Instance;
        
        private IPersonaService personaService;
        private IWalletService walletService;
        private ISessionService sessionService;

        private enum States
        {
            QR,
            RefreshAccessToken,
            RefreshingAccessToken,
            LoginFinished,
        }

        private States state = States.QR;

        private CancellationTokenSource cts;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            cts = new CancellationTokenSource();
            HandleStates().Forget();
        }

        private void OnDisable()
        {
            cts.Cancel();
        }

        private async UniTask HandleStates()
        {
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    switch (state)
                    {
                        case States.QR:
                            await HandleQR();
                            break;
                        case States.RefreshAccessToken:
                            await HandleRefreshAccessToken();
                            break;
                        case States.RefreshingAccessToken:
                            break; // Not handled here since this state is intermediary
                        case States.LoginFinished:
                            break; // Not handled here since this state is final
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore the exception caused by cancellation
            }
        }

        private async UniTask HandleQR()
        {
            float timeRemaining = QRRefreshTimeOut;

            while (state == States.QR && timeRemaining > 0)
            {
                await UniTask.Delay(1000); // update every second
                timeRemaining -= 1.0f;
                refreshCounterText.text = timeRemaining.ToString("0");
            }

            if (state == States.QR)
            {
                state = States.RefreshAccessToken;
                await RefreshQRCodeAndHandshake();
            }
        }

        private async UniTask RefreshQRCodeAndHandshake()
        {
            await sessionService.GetQRCode((texture) =>
                {
                    rawQRImage.texture = texture;

                    walletService.Handshake((walletAddress) =>
                        {
                            state = States.RefreshAccessToken;
                            HeaderScreen.Instance.Refresh(walletAddress);
                        },
                        (error, code) =>
                        {
                            EmergenceLogger.LogError("[" + code + "] " + error);
                            Reinitialize();
                        });
                },
                (error, code) =>
                {
                    EmergenceLogger.LogError(error, code);
                    Reinitialize();
                });
        }

        private async UniTask HandleRefreshAccessToken()
        {
            if (state == States.RefreshAccessToken)
            {
                state = States.RefreshingAccessToken;
                await personaService.GetAccessToken((token) =>
                    {
                        state = States.LoginFinished;
                        PlayerPrefs.SetInt(StaticConfig.HasLoggedInOnceKey, 1);
                        ScreenManager.Instance.ShowDashboard();
                    },
                    (error, code) =>
                    {
                        EmergenceLogger.LogError("[" + code + "] " + error);
                        Reinitialize();
                    });
            }
        }

        public void Restart()
        {
            state = States.QR;
            HandleStates().Forget();
        }

        private void Reinitialize()
        {
            ModalPromptOK.Instance.Show("Sorry, there was a problem with your request", () =>
            {
                state = States.QR;
                HandleStates().Forget();
            });
        }
    }
}
