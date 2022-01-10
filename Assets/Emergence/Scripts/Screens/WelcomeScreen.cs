using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK
{
    public class WelcomeScreen : MonoBehaviour
    {
        public Button connectWalletButton;

        private void Awake()
        {
            connectWalletButton.onClick.AddListener(OnConnectWallet);
        }

        private float timeCounter = 0.0f;
        private const string cheatcode = "nowallet";
        private int codeIndex = 0;
        private float cheatTimeOut = 10.0f;

        private bool cheatActive = false;

        private void Update()
        {
            timeCounter -= Time.deltaTime;
            if (timeCounter <= 0.0f)
            {
                timeCounter += cheatTimeOut;
                codeIndex = 0;
            }

            string currentKey = KeyToString.GetCurrentAlphaKey();

            if (currentKey.Trim().Length == 1)
            {
                if (currentKey.Equals(cheatcode[codeIndex].ToString()))
                {
                    codeIndex++;
                    timeCounter += cheatTimeOut;

                    if (codeIndex >= cheatcode.Length)
                    {
                        cheatActive = true;
                        Debug.Log("CHEAT ACTIVATED!");
                        codeIndex = 0;

                        string accessTokenJson = System.IO.File.ReadAllText("accessToken.json");

                        Services.Instance.SkipWallet(cheatActive, accessTokenJson);
                        ScreenManager.Instance.ShowDashboard();
                    }
                }
                else
                {
                    codeIndex = 0;
                    timeCounter += cheatTimeOut;
                    Debug.LogWarning("CHEAT SEQUENCE RESET!");
                }
            }
        }

        private void OnConnectWallet()
        {
            ScreenManager.Instance.ShowLogIn();
        }
    }
}
