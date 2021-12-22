using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Emergence
{
    public class HeaderScreen : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject headerInformation;
        public TextMeshProUGUI walletBalance;
        public TextMeshProUGUI walletAddress;
        public Button disconnectButton;
        public RawImage avatar;

        public static HeaderScreen Instance;

        private readonly float refreshTimeOut = 30.0f;
        private float remainingTime = 0.0f;
        private void Awake()
        {
            Instance = this;
            disconnectButton.onClick.AddListener(OnDisconnectClick);
        }

        private void Update()
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0.0f)
            {
                remainingTime += refreshTimeOut;

                NetworkManager.Instance.GetBalance((balance) =>
                {
                    walletBalance.text = balance;
                },
                (error, code) =>
                {
                    Debug.LogError("[" + code + "] " + error);
                });
            }
        }

        private void Start()
        {
            Hide();
        }

        public void Hide()
        {
            headerInformation.SetActive(false);
        }

        public void Show()
        {
            headerInformation.SetActive(true);
            remainingTime = 0.0f;
        }

        public void Refresh(string address)
        {
            walletAddress.text = address;
            // TODO add the circle avatar image data
        }

        public delegate void Disconnect();
        public static event Disconnect OnDisconnect;
        private void OnDisconnectClick()
        {
            OnDisconnect?.Invoke();
        }
    }
}