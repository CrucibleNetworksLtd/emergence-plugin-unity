using UnityEngine;
using UnityEngine.UI;

namespace Emergence
{
    public class WelcomeScreen : MonoBehaviour
    {
        public Button connectWalletButton;

        private void Awake()
        {
            connectWalletButton.onClick.AddListener(OnConnectWallet);
        }

        private void OnConnectWallet()
        {
            EmergenceManager.Instance.ShowLogIn();
        }
    }
}
