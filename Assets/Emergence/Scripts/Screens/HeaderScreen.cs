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

        private void Awake()
        {
            disconnectButton.onClick.AddListener(OnDisconnectClick);
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

        public void Refresh(string balance, string address)
        {
            walletBalance.text = balance;
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