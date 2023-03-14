using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK
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

        private void Update()
        {
            switch (state)
            {
                case States.QR:
                    timeRemaining -= Time.deltaTime;
                    if (timeRemaining <= 0.0f)
                    {
                        timeRemaining += QRRefreshTimeOut;

                        EmergenceServices.Instance.GetQRCode((texture, deviceId) =>
                            {
                                EmergenceSingleton.Instance.CurrentDeviceId = deviceId;
                                rawQRImage.texture = texture;

                                EmergenceServices.Instance.Handshake((walletAddress) =>
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
                        //
                        // Services.Instance.ReinitializeWalletConnect((disconnected) =>
                        // {
                        //     Services.Instance.Handshake((walletAddress) =>
                        //     {
                        //         state = States.RefreshAccessToken;
                        //         HeaderScreen.Instance.Refresh(walletAddress);
                        //     },
                        //     (error, code) =>
                        //     {
                        //         Debug.LogError("[" + code + "] " + error);
                        //         Reinitialize();
                        //     });
                        //
                        //     Services.Instance.GetQRCode((texture) =>
                        //     {
                        //         rawQRImage.texture = texture;
                        //     },
                        //     (error, code) =>
                        //     {
                        //         Debug.LogError("[" + code + "] " + error);
                        //         Reinitialize();
                        //     });
                        // },
                        // (error, code) =>
                        // {
                        //     Debug.LogError("[" + code + "] " + error);
                        //     Reinitialize();
                        // });
                    }

                    refreshCounterText.text = timeRemaining.ToString("0");
                    break;
                case States.RefreshAccessToken:
                    state = States.RefreshingAccessToken;
                    EmergenceServices.Instance.GetAccessToken((token) =>
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

            #region Test
            /*
            if (Input.GetKeyDown(KeyCode.B))
            {
                Debug.Log("CHEAT ACTIVATED!");

                string accessTokenJson = System.IO.File.ReadAllText("accessToken.json");

                Services.Instance.SkipWallet(true, accessTokenJson);
                ScreenManager.Instance.ShowDashboard();
            }*/
            #endregion Test
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
