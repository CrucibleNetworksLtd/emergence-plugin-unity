using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Implementations.Login;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
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
        
        private IWalletServiceInternal walletServiceInternal => EmergenceServiceProvider.GetService<IWalletServiceInternal>();
        private ISessionServiceInternal sessionServiceInternal => EmergenceServiceProvider.GetService<ISessionServiceInternal>();
        
        private CancellationTokenSource qrCancellationToken = new CancellationTokenSource();
#pragma warning disable CS0414 // Field is assigned but its value is never used
        private bool hasStarted = false;
#pragma warning restore CS0414 // Field is assigned but its value is never used
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
            instance.gameObject.SetActive(false);
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
            var qrResponse = await sessionServiceInternal.GetQrCodeAsync();
            if (!qrResponse.Successful)
            {
                EmergenceLogger.LogError("Error retrieving QR code.");
                return false;
            }

            rawQRImage.texture = qrResponse.Result1;
            return true;
        }
        
        private async UniTask<string> Handshake()
        {
            var handshakeResponse = await walletServiceInternal.HandshakeAsync();
            if (!handshakeResponse.Successful)
            {
                EmergenceLogger.LogError("Error during handshake.");
                return "";
            }
            return handshakeResponse.Result1;
        }

        private async UniTask<bool> HandleRefreshAccessToken()
        {
            var tokenResponse = await sessionServiceInternal.GetAccessTokenAsync();
            if (!tokenResponse.Successful)
                return false;

            LoginManager.SetFirstLoginFlag();
            ScreenManager.Instance.ShowDashboard().Forget();
            return true;
        }
        
        
        private void Restart()
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