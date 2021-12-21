using System.Collections;
using System.Collections.Generic;
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

        private void Awake()
        {
            Instance = this;
            disconnectButton.onClick.AddListener(OnDisconnectClick);
        }

        // TODO Update
        private void EmergenceState_OnBalanceRefreshed(string balance)
        {
            walletBalance.text = balance;
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