using System.Collections;
using EmergenceSDK.Internal.Services;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI.Screens
{
    public class HeaderScreen : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject headerInformation;
        public TextMeshProUGUI walletBalance;
        public TextMeshProUGUI walletAddress;
        public Button menuButton;
        public Button disconnectModalButton;
        public Button disconnectButton;

        public static HeaderScreen Instance;

        private readonly float refreshTimeOut = 30.0f;
        private IWalletService walletService;
        private IAccountService accountService;
        
        private Coroutine refreshCoroutine;

        private void Awake()
        {
            Instance = this;
            menuButton.onClick.AddListener(OnMenuOpenClick);
            disconnectButton.onClick.AddListener(OnDisconnectClick);
            disconnectModalButton.onClick.AddListener(OnMenuCloseClick);
        }
        
        private void Start()
        {
            walletService = EmergenceServices.GetService<WalletService>();
            accountService = EmergenceServices.GetService<AccountService>();
            Hide();

            refreshCoroutine = StartCoroutine(RefreshWalletBalance());
        }

        private void OnDestroy()
        {
            menuButton.onClick.RemoveListener(OnMenuOpenClick);
            disconnectButton.onClick.RemoveListener(OnDisconnectClick);
            disconnectModalButton.onClick.RemoveListener(OnMenuCloseClick);
        }
        
        IEnumerator RefreshWalletBalance()
        {
            while (gameObject.activeSelf && headerInformation.activeSelf)
            {
                walletService.GetBalance((balance) =>
                    {
                        string converted = UnitConverter.Convert(balance, UnitConverter.EtherUnitType.WEI, UnitConverter.EtherUnitType.ETHER, ",");
                        string[] splitted = converted.Split(new string[] { "," }, System.StringSplitOptions.None);

                        string result = splitted[0];

                        if (splitted.Length == 2)
                        {
                            result += "." + splitted[1].Substring(0, UnitConverter.SIGNIFICANT_DIGITS);
                        }

                        walletBalance.text = result;// + " " + Emergence.Instance.TokenSymbol;
                    },
                    (error, code) =>
                    {
                        Debug.LogError("[" + code + "] " + error);
                        ModalPromptOK.Instance.Show("Sorry, there was a problem getting your balance, will retry in " + refreshTimeOut.ToString("0") + " seconds");
                    });
                yield return new WaitForSeconds(refreshTimeOut);
            }
        }

        public void Hide()
        {
            headerInformation.SetActive(false);
            if (refreshCoroutine != null) 
                StopCoroutine(refreshCoroutine);
        }

        public void Show()
        {
            headerInformation.SetActive(true);
            refreshCoroutine = StartCoroutine(RefreshWalletBalance());
        }

        public void Refresh(string address)
        {
            walletAddress.text = address.Substring(0, 6) + "..." + address.Substring(address.Length - 4, 4);
        }

        private void OnMenuOpenClick()
        {
            disconnectModalButton.gameObject.SetActive(true);
        }

        private void OnMenuCloseClick()
        {
            disconnectModalButton.gameObject.SetActive(false);
        }

        private void OnDisconnectClick()
        {
            Modal.Instance.Show("Disconnecting wallet...");
            accountService.Disconnect(() =>
            {
                Modal.Instance.Hide();
                Hide();
                ScreenManager.Instance.Restart();
            },
            (error, code) =>
            {
                Debug.LogError("[" + code + "] " + error);
            });
        }
    }
}