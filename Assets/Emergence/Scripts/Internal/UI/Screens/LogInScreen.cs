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

        private float timeRemaining = 0.0f;
        private readonly float QRRefreshTimeOut = 60.0f;

        public static LogInScreen Instance;
        
        private IAccountService accountService;
        private IWalletService walletService;
        private IQRCodeService qrService;
        

        private enum States
        {
            QR,
            RefreshAccessToken,
            RefreshingAccessToken,
            LoginFinished,
        }

        private States state = States.QR;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            accountService = EmergenceServices.GetService<IAccountService>();
            walletService = EmergenceServices.GetService<IWalletService>();
            qrService = EmergenceServices.GetService<IQRCodeService>();
        }

        private void Update()
        {
            switch (state)
            {
                case States.QR:
                {
                    timeRemaining -= Time.deltaTime;
                    if (timeRemaining <= 0.0f)
                    {
                        timeRemaining += QRRefreshTimeOut;

                        qrService.GetQRCode((texture, deviceId) =>
                            {
                                EmergenceSingleton.Instance.CurrentDeviceId = deviceId;
                                rawQRImage.texture = texture;

                                walletService.Handshake((walletAddress) =>
                                    {
                                        state = States.RefreshAccessToken;
                                        HeaderScreen.Instance.Refresh(walletAddress);
                                    },
                                    (error, code) =>
                                    {
                                        Debug.LogError("[" + code + "] " + error);
                                        Reinitialize();
                                    });
                            },
                            (error, code) =>
                            {
                                Debug.LogError("[" + code + "] " + error);
                                Reinitialize();
                            });
                    }

                    refreshCounterText.text = timeRemaining.ToString("0");
                    break;
                }
                case States.RefreshAccessToken:
                {
                    state = States.RefreshingAccessToken;
                    accountService.GetAccessToken((token) =>
                        {
                            state = States.LoginFinished;
                            PlayerPrefs.SetInt(EmergenceSingleton.HAS_LOGGED_IN_ONCE_KEY, 1);
                            ScreenManager.Instance.ShowDashboard();
                        },
                        (error, code) =>
                        {
                            Debug.LogError("[" + code + "] " + error);
                            Reinitialize();
                        });
                    break;
                }
            }
        }

        public void Restart()
        {
            state = States.QR;
            timeRemaining = 0.0f;
        }

        private void Reinitialize()
        {
            ModalPromptOK.Instance.Show("Sorry, there was a problem with your request", () =>
            {
                state = States.QR;
                timeRemaining = 0.0f;
            });
        }

    }
}
