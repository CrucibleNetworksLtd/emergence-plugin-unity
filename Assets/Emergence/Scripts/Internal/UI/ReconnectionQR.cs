using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.UI.Screens;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI
{
    public class ReconnectionQR : MonoBehaviour
    {
        [Header("UI References")]
        public RawImage rawQRImage;
        public Button closeButton;
        public TextMeshProUGUI refreshCounterText;
        public void SetTimeRemainingText() => refreshCounterText.text = timeRemaining.ToString("0");
        

        private readonly int qrRefreshTimeOut = 60;
        private int timeRemaining;
        
        private IPersonaService personaService => EmergenceServices.GetService<IPersonaService>();
        private IWalletService walletService => EmergenceServices.GetService<IWalletService>();
        private ISessionService sessionService => EmergenceServices.GetService<ISessionService>();
        
        private CancellationTokenSource qrCancellationToken = new CancellationTokenSource();
        private bool hasStarted = false;
        private bool timerIsRunning = false;
        private bool loginComplete = false;
        
        private static ReconnectionQR instance;

        private List<Action> reconnectionEvents = new List<Action>();

        public static async UniTask<bool> FireEventOnReconnection(Action action)
        {
            if (instance == null)
                instance = EmergenceSingleton.Instance.ReconnectionQR;
            instance.gameObject.SetActive(true);
            instance.reconnectionEvents.Add(action);
            instance.closeButton.onClick.RemoveAllListeners();
            instance.closeButton.onClick.AddListener(() =>
            {
                instance.reconnectionEvents.Clear();
                instance.gameObject.SetActive(false);
            });
            return await instance.HandleReconnection();
        }

        private async UniTask<bool> HandleReconnection()
        {
            await HandleQR(qrCancellationToken);
            instance.gameObject.SetActive(false);
            return loginComplete;
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
            foreach (var reconnectionEvent in reconnectionEvents)
            {
                reconnectionEvent.Invoke();
            }
            reconnectionEvents.Clear();
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
        
        
        private void Restart()
        {
            timeRemaining = qrRefreshTimeOut;
            qrCancellationToken.Cancel();
            qrCancellationToken = new CancellationTokenSource();
            qrCancellationToken.Token.ThrowIfCancellationRequested();
            HandleQR(qrCancellationToken).Forget();
        }
    }
}