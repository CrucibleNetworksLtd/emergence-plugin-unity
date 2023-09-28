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
        public void SetTimeRemainingText() =>  refreshCounterText.text = refreshCounterText.text = timeRemaining.ToString("0");

        private readonly int qrRefreshTimeOut = 10;
        private int timeRemaining;
        
        public static LogInScreen Instance;
        
        private IPersonaService personaService => EmergenceServices.GetService<IPersonaService>();
        private IWalletService walletService => EmergenceServices.GetService<IWalletService>();
        private ISessionService sessionService => EmergenceServices.GetService<ISessionService>();
        
        private CancellationTokenSource qrCancellationToken = new CancellationTokenSource();

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            timeRemaining = qrRefreshTimeOut;
            refreshCounterText.text = "";
            HandleQR(qrCancellationToken).Forget();
        }

        private void OnDisable()
        {
        }

        private async UniTask HandleQR(CancellationTokenSource cts)
        {
            var refreshQR = await RefreshQR();
            if(refreshQR == false)
            {
                HandleQR(qrCancellationToken).Forget();
                UniTask.WaitForEndOfFrame(this);
                return;
            }
            StartCountdown().Forget();
            var handshake = await Handshake();
            if(handshake == String.Empty)
            {
                HandleQR(qrCancellationToken).Forget();
                UniTask.WaitForEndOfFrame(this);
                return;
            }
            HeaderScreen.Instance.Refresh(handshake);
            var refreshAccessToken = await HandleRefreshAccessToken();
            if(refreshAccessToken == false)
            {
                HandleQR(qrCancellationToken).Forget();
                UniTask.WaitForEndOfFrame(this);
            }
        }

        private async UniTask StartCountdown()
        {
            while (timeRemaining > 0)
            {
                SetTimeRemainingText();
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                timeRemaining--;
            }
            qrCancellationToken.Cancel();
            qrCancellationToken = new CancellationTokenSource();
            HandleQR(qrCancellationToken).Forget();
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
                EmergenceLogger.LogError("Error retrieving QR code.");
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
            ScreenManager.Instance.ShowDashboard();
            return true;
        }
    }
}
