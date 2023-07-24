using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
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
        
        private IPersonaService personaService => EmergenceServices.GetService<IPersonaService>();
        private IWalletService walletService => EmergenceServices.GetService<IWalletService>();
        private ISessionService sessionService => EmergenceServices.GetService<ISessionService>();

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
                            await UniTask.WaitForEndOfFrame(this);
                            break;
                        case States.LoginFinished:
                            cts.Cancel();
                            break;
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

            await RefreshQRCodeAndHandshake();
            
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
            state = States.RefreshingAccessToken;
            var qrResponse = await sessionService.GetQRCodeAsync();
            if (!qrResponse.Success)
            {
                EmergenceLogger.LogError("Error retrieving QR code.");
                Reinitialize();
                return;
            }

            rawQRImage.texture = qrResponse.Result;
            var handshakeResponse = await walletService.HandshakeAsync();
            if (!handshakeResponse.Success)
            {
                EmergenceLogger.LogError("Error retrieving QR code.");
                Reinitialize();
                return;
            }
            state = States.RefreshAccessToken;
            HeaderScreen.Instance.Refresh(handshakeResponse.Result);
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
