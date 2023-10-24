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

        public void SetTimeRemainingText() => refreshCounterText.text = timeRemaining.ToString("0");

        private readonly int qrRefreshTimeOut = 60;
        private int timeRemaining;
        
        public static LogInScreen Instance;
        
        private IPersonaService personaService => EmergenceServices.GetService<IPersonaService>();
        private IWalletService walletService => EmergenceServices.GetService<IWalletService>();
        private ISessionService sessionService => EmergenceServices.GetService<ISessionService>();
        
        private CancellationTokenSource qrCancellationToken = new CancellationTokenSource();
        private bool hasStarted = false;
        private bool loginComplete = false;
        private bool timerIsRunning = false;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            if (!hasStarted)
            {
                timeRemaining = qrRefreshTimeOut;
                refreshCounterText.text = "";
                HandleQR(qrCancellationToken).Forget();
                hasStarted = true;
            }
        }

        private async UniTask HandleQR(CancellationTokenSource cts)
        {
            try
            {
                var token = cts.Token;

                var refreshQR = await RefreshQR();
                if (!refreshQR)
                {
                    Restart();
                    return;
                }
                
                StartCountdown(token).Forget();
                
                var handshake = await Handshake();
                if (string.IsNullOrEmpty(handshake))
                {
                    Restart();
                    return;
                }

                HeaderScreen.Instance.Refresh(handshake);
                HeaderScreen.Instance.Show();
                
                var refreshAccessToken = await HandleRefreshAccessToken();
                if (!refreshAccessToken)
                {
                    Restart();
                    return;
                }
            }
            catch (OperationCanceledException e)
            {
                EmergenceLogger.LogError(e.Message, e.HResult);
                Restart();
            }
            loginComplete = true;
        }

        private async UniTask StartCountdown(CancellationToken cancellationToken)
        {
            if (timerIsRunning)
                return;
            try
            {
                timerIsRunning = true;
                while (timeRemaining > 0 && !loginComplete)
                {
                    SetTimeRemainingText();
                    await UniTask.Delay(TimeSpan.FromSeconds(1));
                    timeRemaining--;
                }
            }
            catch (Exception e)
            {
                EmergenceLogger.LogError(e.Message, e.HResult);
                timerIsRunning = false;
                return;
            }
            Restart();
            timerIsRunning = false;
        }
        
        private async UniTask<bool> RefreshQR()
        {
            var qrResponse = await sessionService.GetQRCodeAsync();
            if (!qrResponse.Success)
            {
                EmergenceLogger.LogError("Error retrieving QR code.");
                return false;
            }

            rawQRImage.texture = qrResponse.Result;
            return true;
        }
        
        private async UniTask<string> Handshake()
        {
            var handshakeResponse = await walletService.HandshakeAsync();
            if (!handshakeResponse.Success)
            {
                EmergenceLogger.LogError("Error during handshake.");
                return "";
            }
            return handshakeResponse.Result;
        }

        private async UniTask<bool> HandleRefreshAccessToken()
        {
            var tokenResponse = await personaService.GetAccessTokenAsync();
            if (!tokenResponse.Success)
                return false;
            PlayerPrefs.SetInt(StaticConfig.HasLoggedInOnceKey, 1);
            ScreenManager.Instance.ShowDashboard().Forget();
            return true;
        }

        public void FullRestart()
        {
            loginComplete = false;
            Restart();
        }
        
        public void Restart()
        {
            if(loginComplete)
                return;
            timeRemaining = qrRefreshTimeOut;
            qrCancellationToken.Cancel();
            qrCancellationToken = new CancellationTokenSource();
            qrCancellationToken.Token.ThrowIfCancellationRequested();
            HandleQR(qrCancellationToken).Forget();
        }
    }
}
