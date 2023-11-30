using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
        [Header("UI References")]
        public RawImage rawQRImage;
        public Button backButton;
        public TextMeshProUGUI refreshCounterText;
        
        [Header("Sub Screens")]
        public GameObject qrScreen;
        public GameObject futureverseScreen;
        public GameObject startupScreen;
        
        [Header("Futureverse")]
        public Button LoginWithFV;
        public Button LoginWithWC;
        public Button CreateFPass;
        public Button RetryFPassCheck;
        
        public void SetTimeRemainingText() => refreshCounterText.text = timeRemaining.ToString("0");

        private readonly int qrRefreshTimeOut = 60;
        private int timeRemaining;
        
        private bool usingFV = false;
        
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
            
            LoginWithFV.onClick.AddListener(LoginWithFVClicked);
            LoginWithWC.onClick.AddListener(LoginWithWCClicked);
            
            CreateFPass.onClick.AddListener(CreateFPassClicked);
            RetryFPassCheck.onClick.AddListener(RetryFPassCheckClicked);
        }

        private void HideAllScreens()
        {
            qrScreen.SetActive(false);
            futureverseScreen.SetActive(false);
            startupScreen.SetActive(false);
        }
        
        private void LoginWithFVClicked()
        {
            HideAllScreens();
            usingFV = true;
            qrScreen.SetActive(true);
        }
        
        private void LoginWithWCClicked()
        {
            HideAllScreens();
            usingFV = false;
            qrScreen.SetActive(true);
        }
        
        private void CreateFPassClicked()
        {
            Application.OpenURL("https://futurepass.futureverse.app/");
        }

        private void RetryFPassCheckClicked()
        {
            FullRestart();
        }


        private void OnDestroy()
        {
            LoginWithFV.onClick.RemoveListener(LoginWithFVClicked);
            LoginWithWC.onClick.RemoveListener(LoginWithWCClicked);
            
            CreateFPass.onClick.RemoveListener(CreateFPassClicked);
            RetryFPassCheck.onClick.RemoveListener(RetryFPassCheckClicked);
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

            if (usingFV)
            {
                var loggedInWithFV = await AttemptFVLogin();
                if (!loggedInWithFV)
                { 
                    HideAllScreens();
                    futureverseScreen.SetActive(true);
                    return true; //Bit of a hack to prevent the screen from refreshing.
                }
            }

            PlayerPrefs.SetInt(StaticConfig.HasLoggedInOnceKey, 1);
            ScreenManager.Instance.ShowDashboard().Forget();
            return true;
        }

        private async UniTask<bool> AttemptFVLogin()
        {
            var fvService = EmergenceServices.GetService<IFutureverseService>();
            var linkedPassInfo = await fvService.GetLinkedFuturepassInformation();
            if (!linkedPassInfo.Success)
                return false;
            var fpass = await fvService.GetFuturePassInformation(linkedPassInfo.Result.ownedFuturepass);
            if (!fpass.Success)
                return false;
            EmergenceLogger.LogInfo("Logged in with Futureverse.");
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
            
            HideAllScreens();
            startupScreen.SetActive(true);
            
            timeRemaining = qrRefreshTimeOut;
            qrCancellationToken.Cancel();
            qrCancellationToken = new CancellationTokenSource();
            qrCancellationToken.Token.ThrowIfCancellationRequested();
            HandleQR(qrCancellationToken).Forget();
        }
    }
}
