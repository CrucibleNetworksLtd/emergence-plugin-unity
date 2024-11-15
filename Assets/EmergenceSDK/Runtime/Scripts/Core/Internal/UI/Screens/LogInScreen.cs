using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Futureverse;
using EmergenceSDK.Runtime.Internal.Services;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Exceptions.Login;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Runtime.Internal.UI
{
    public class LogInScreen : MonoBehaviour
    {
        [Header("Log-in Manager")]
        public LoginManager loginManager;

        [Header("UI References")]
        public RawImage rawQrImage;

        public Button backButton;
        public GameObject urlContainer;
        public TMP_InputField urlInputField;
        public Button copyUrlButton;
        public TextMeshProUGUI refreshCounterText;
        public TextMeshProUGUI refreshText;

        [Header("Sub Screens")]
        public GameObject qrScreen;
        public GameObject futureverseScreen;

        public Button createFPass;
        public Button retryFPassCheck;

        [Header("Futureverse Login UI")] 
        [SerializeField]
        private GameObject fVLoginScreen = null;
        
        [SerializeField]
        private TMP_Text custodialLoginMessage = null;

        [SerializeField] 
        private Button fVWalletConnectLoginBeginButton = null;
        
        [SerializeField] 
        private Button fVCustodialLoginBeginButton = null;

        private void SetTimeRemainingText(LoginManager _, EmergenceQrCode emergenceQrCode) => refreshCounterText.text = emergenceQrCode.TimeLeftInt.ToString("0");

        public static LogInScreen Instance;

        private static IWalletServiceInternal WalletServiceInternal => EmergenceServiceProvider.GetService<IWalletServiceInternal>();

        private void Awake()
        {
            Instance = this;

            backButton.onClick.AddListener(() => loginManager.CancelLogin());

            createFPass.onClick.AddListener(CreateFPassClicked);
            retryFPassCheck.onClick.AddListener(RetryFPassCheckClicked);

            loginManager.qrCodeTickEvent.AddListener(SetTimeRemainingText);
            loginManager.loginStartedEvent.AddListener(HandleLoginStarted);
            loginManager.loginCancelledEvent.AddListener((_) => { loginManager.CancelLogin(); EmergenceSingleton.Instance.CloseEmergenceUI(); });
            loginManager.loginFailedEvent.AddListener(HandleLoginErrors);

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
                        urlInputField.text = loginManager.CurrentQrCode.WalletConnectUrl;
                        copyUrlButton.interactable = true;
                        break;
                    case LoginStep.HandshakeRequest:
                        HeaderScreen.Instance.Refresh(((IWalletService)WalletServiceInternal).ChecksummedWalletAddress);
                        HeaderScreen.Instance.Show();
                        break;
                    case LoginStep.CustodialRequests:
                        custodialLoginMessage.text = "Successfully generated Custodial Bearer token, Connecting to Emergence Services";
                        break;
                    case LoginStep.AccessTokenRequest:
                    case LoginStep.FuturepassRequests:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(loginStep), loginStep, null);
                }
            });

            loginManager.loginSuccessfulEvent.AddListener((_, _) =>
            {
                LoginManager.SetFirstLoginFlag();
                ScreenManager.Instance.ShowDashboard().Forget();
            });
        }

        private void HandleLoginErrors(LoginManager _, LoginExceptionContainer exceptionContainer)
        {
            HideAllScreens();
            var e = exceptionContainer.Exception;
            switch (e)
            {
                case FuturepassRequestFailedException or FuturepassInformationRequestFailedException:
                    exceptionContainer.HandleException();
                    EmergenceLogger.LogWarning(e);
                    futureverseScreen.SetActive(true);
                    break;
                case FuturepassRequestFailedException
                    or FuturepassInformationRequestFailedException
                    or TokenRequestFailedException
                    or HandshakeRequestFailedException
                    or QrCodeRequestFailedException
                    or TokenRequestFailedException
                    or OperationCanceledException:
                    exceptionContainer.HandleException();
                    EmergenceLogger.LogWarning(e);
                    SetupLogin().Forget();
                    break;
            }
        }

        /// <summary>
        /// Event bound to the LoginManager.loginStarted event, called after we call LoginManager.startLogin on enable
        /// </summary>
        /// <param name="_"></param>
        private void HandleLoginStarted(LoginManager _)
        {
            
            switch (EmergenceSingleton.Instance.DefaultLoginFlow)
            {
                case LoginFlow.WalletConnect:
                    StartQRCodeDrivenLogin();
                    break;
            }

        }

        /// <summary>
        /// Once login event is started we display the UI to power it, in this case we display the QR code for login methods that us it
        /// </summary>
        private void StartQRCodeDrivenLogin()
        {
            urlContainer.SetActive(true);
            copyUrlButton.interactable = false;
            urlInputField.text = "";
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
            fVLoginScreen.SetActive(false);
        }

        private void ShowLoginWithFv()
        {
            EmergenceServiceProvider.Load(ServiceProfile.Futureverse);
            fVLoginScreen.SetActive(true);
            fVCustodialLoginBeginButton.interactable = true;
            fVWalletConnectLoginBeginButton.gameObject.SetActive(true);
            custodialLoginMessage.text = "LOGIN WITH BROWSER";
            fVCustodialLoginBeginButton.onClick.AddListener(StartFVCustodialLogin);
            fVWalletConnectLoginBeginButton.onClick.AddListener(StartFVWalletConnectLogin);
        }

        public void StartFVWalletConnectLogin()
        {
            fVLoginScreen.SetActive(false);
            StartQRCodeDrivenLogin();
            UniTask.Void(async () =>
            {
                await loginManager.StartLogin(LoginSettings.EnableFuturepass);
            });
        }
        
        /// <summary>
        /// Called by the custodial login UI, will tell login manager to begin login process and create authentication request with FV
        /// </summary>
        public void StartFVCustodialLogin()
        {
            custodialLoginMessage.text = "Please refer to your default browser to authenticate";
            fVCustodialLoginBeginButton.interactable = false;
            fVWalletConnectLoginBeginButton.gameObject.SetActive(false);
            try
            {
                UniTask.Void(async () =>
                {
                    await loginManager.StartLogin(LoginSettings.EnableCustodialLogin | LoginSettings.EnableFuturepass);
                });
            }
            catch (Exception e)
            {
                HideAllScreens();
            }
        }

        private void ShowLoginWithWc()
        {
            EmergenceServiceProvider.Load(ServiceProfile.Default);
            UniTask.Void(async () =>
            {
                await loginManager.StartLogin(LoginSettings.Default);
            });
        }

        private static void CreateFPassClicked()
        {
            Application.OpenURL(FutureverseSingleton.Instance.Environment == EmergenceEnvironment.Production
                ? "https://futurepass.futureverse.app/"
                : "https://identity-dashboard.futureverse.cloud/");
        }

        private void RetryFPassCheckClicked()
        {
            SetupLogin().Forget();
        }

        private async UniTask SetupLogin()
        {
            loginManager.CancelLogin();
            await loginManager.WaitUntilAvailable();
            HideAllScreens();
            switch (EmergenceSingleton.Instance.DefaultLoginFlow)
            {
                case LoginFlow.Futurepass:
                    ShowLoginWithFv();
                    break;
                case LoginFlow.WalletConnect:
                    ShowLoginWithWc();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(EmergenceSingleton.Instance.DefaultLoginFlow));
            }
        }

        private void OnEnable()
        {
            if (!loginManager.IsBusy)
            {
                SetupLogin().Forget();
            }
        }
    }
}