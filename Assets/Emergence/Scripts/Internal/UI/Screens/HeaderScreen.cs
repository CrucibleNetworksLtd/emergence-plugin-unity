using System.Threading;
using Cysharp.Threading.Tasks;
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

        private readonly float refreshTimeOut = 90.0f;
        private IWalletService walletService => EmergenceServices.GetService<WalletService>();
        private ISessionService sessionService => EmergenceServices.GetService<SessionService>();
        
        private CancellationTokenSource refreshCancellationToken;

        private void Awake()
        {
            Instance = this;
            menuButton.onClick.AddListener(OnMenuOpenClick);
            disconnectButton.onClick.AddListener(OnDisconnectClick);
            disconnectModalButton.onClick.AddListener(OnMenuCloseClick);
        }
        
        private void Start()
        {
            refreshCancellationToken = new CancellationTokenSource();
            _ = RefreshWalletBalanceAsync(refreshCancellationToken.Token);
        }

        private void OnDestroy()
        {
            menuButton.onClick.RemoveListener(OnMenuOpenClick);
            disconnectButton.onClick.RemoveListener(OnDisconnectClick);
            disconnectModalButton.onClick.RemoveListener(OnMenuCloseClick);
        }
        
        private async UniTask RefreshWalletBalanceAsync(CancellationToken cancellationToken)
        {
            while (gameObject.activeSelf && headerInformation.activeSelf)
            {
                var balance = await walletService.GetBalanceAsync();
                if(balance.Success)
                {
                    string converted = UnitConverter.Convert(balance.Result, UnitConverter.EtherUnitType.WEI, UnitConverter.EtherUnitType.ETHER, ",");
                    string[] splitted = converted.Split(new string[] { "," }, System.StringSplitOptions.None);
                    string result = splitted[0];
                    if (splitted.Length == 2)
                    {
                        result += "." + splitted[1].Substring(0, Mathf.Min(UnitConverter.SIGNIFICANT_DIGITS, splitted.Length));
                    }
                    walletBalance.text = result + " " + EmergenceSingleton.Instance.Configuration.Chain.CurrencySymbol;
                }
                else
                {
                    ModalPromptOK.Instance.Show("Sorry, there was a problem getting your balance, will retry in " + refreshTimeOut.ToString("0") + " seconds");
                }
                await UniTask.Delay((int)(refreshTimeOut * 1000), cancellationToken: cancellationToken);
            }
        }

        public void Hide()
        {
            headerInformation.SetActive(false);
            refreshCancellationToken?.Cancel();
        }

        public void Show()
        {
            headerInformation.SetActive(true);
            refreshCancellationToken = new CancellationTokenSource();
            RefreshWalletBalanceAsync(refreshCancellationToken.Token).Forget();
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

        private async void OnDisconnectClick()
        {
            Modal.Instance.Show("Disconnecting wallet...");
            refreshCancellationToken?.Cancel();
            var result = await sessionService.DisconnectAsync();
            Modal.Instance.Hide();
            if (result.Success)
            {
                Hide();
                ScreenManager.Instance.Restart().Forget();
            }
        }
    }
}
