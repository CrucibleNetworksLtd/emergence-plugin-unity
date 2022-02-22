using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK
{
    public class LogInScreen : MonoBehaviour
    {
        [Header("UI References")]
        public RawImage rawQRImage;
        public TextMeshProUGUI refreshCounterText;

        private float timeRemaining = 0.0f;
        private readonly float QRRefreshTimeOut = 60.0f;

        public static LogInScreen Instance;

        private enum States
        {
            Handshake,
            QR,
            RefreshAccessToken,
            RefreshingAccessToken,
            LoginFinished,
        }

        private States state = States.Handshake;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            switch (state)
            {
                case States.Handshake:
                    Services.Instance.Handshake((walletAddress) =>
                    {
                        state = States.RefreshAccessToken;
                        HeaderScreen.Instance.Refresh(walletAddress);
                    },
                    (error, code) =>
                    {
                        Debug.LogError("[" + code + "] " + error);
                        Reinitialize();
                    });
                    state = States.QR;
                    break;
                case States.QR:
                    timeRemaining -= Time.deltaTime;
                    if (timeRemaining <= 0.0f)
                    {
                        timeRemaining += QRRefreshTimeOut;
                        Services.Instance.ReinitializeWalletConnect((disconnected) =>
                        {
                            Services.Instance.GetQRCode((texture) =>
                            {
                                rawQRImage.texture = texture;
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
                case States.RefreshAccessToken:
                    state = States.RefreshingAccessToken;
                    Services.Instance.GetAccessToken((token) =>
                    {
                        state = States.LoginFinished;
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

        public void Restart()
        {
            state = States.Handshake;
            timeRemaining = 0.0f;
        }

        private void Reinitialize()
        {
            ModalPromptOK.Instance.Show("Sorry, there was a problem with your request", () =>
            {
                Services.Instance.ReinitializeWalletConnect((disconnected) =>
                {
                    state = States.Handshake;
                    timeRemaining = 0.0f;
                },
                (error, code) =>
                {
                    Debug.LogError("[" + code + "] " + error);
                    ModalPromptOK.Instance.Show("Error initializing wallet connection", () =>
                    {
                        Reinitialize();
                    });
                });
            });
        }

    }
}
