using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Emergence
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
                    NetworkManager.Instance.Handshake((walletAddress) => 
                    {
                        state = States.RefreshAccessToken;
                        HeaderScreen.Instance.Refresh(walletAddress);
                    }, 
                    (error, code) => 
                    {
                        Debug.LogError("[" + code + "] " + error);
                    });

                    state = States.QR;
                    break;
                case States.QR:
                    timeRemaining -= Time.deltaTime;
                    if (timeRemaining <= 0.0f)
                    {
                        timeRemaining += QRRefreshTimeOut;
                        NetworkManager.Instance.GetQRCode((texture) =>
                        {
                            rawQRImage.texture = texture;
                        },
                        (error, code) =>
                        {
                            Debug.LogError("[" + code + "] " + error);
                        });
                    }

                    refreshCounterText.text = Convert.ToInt32(timeRemaining).ToString();
                    break;
                case States.RefreshAccessToken:
                    state = States.RefreshingAccessToken;
                    NetworkManager.Instance.GetAccessToken((token) =>
                    {
                        state = States.LoginFinished;
                        EmergenceManager.Instance.ShowDashboard();
                    },
                    (error, code) =>
                    {
                        Debug.LogError("[" + code + "] " + error);
                    });
                    break;
            }
        }

        public void Restart()
        {
            state = States.Handshake;
            timeRemaining = 0.0f;
        }
    }
}
