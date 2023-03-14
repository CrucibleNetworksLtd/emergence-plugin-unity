using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK
{
    public class WelcomeScreen : MonoBehaviour
    {
        [Header("UI Screen references")]
        public GameObject splashScreen;
        public GameObject headerScreen;
        public GameObject step1Screen;
        public GameObject step2Screen;
        public GameObject step3Screen;

        [Header("UI Button references")]
        public Button skipButton;
        public Button space1Button;
        public Button space2Button;
        public Button space3Button;

        private enum States
        {
            Splash,
            Step1,
            Step2,
            Step3,
        }

        private States state = States.Splash;
        private const float splashDuration = 3.0f;
        private void Awake()
        {
            skipButton.onClick.AddListener(OnConnectWallet);
            space1Button.onClick.AddListener(OnNext);
            space2Button.onClick.AddListener(OnNext);
            space3Button.onClick.AddListener(OnNext);

            Reset();
        }

        private void OnDestroy()
        {
            skipButton.onClick.RemoveListener(OnConnectWallet);
            space1Button.onClick.RemoveListener(OnNext);
            space2Button.onClick.RemoveListener(OnNext);
            space3Button.onClick.RemoveListener(OnNext);
        }

        private void OnEnable()
        {
            Reset();
        }

        private void Reset()
        {
            splashTimer = 0.0f;
            state = States.Splash;
            headerScreen.SetActive(false);
            ShowScreen(splashScreen);
        }

        private void ShowScreen(GameObject screen)
        {
            splashScreen.SetActive(false);
            step1Screen.SetActive(false);
            step2Screen.SetActive(false);
            step3Screen.SetActive(false);

            screen.SetActive(true);
        }

        private float splashTimer = 0.0f;
        private void OnNext()
        {
            switch (state)
            {
                case States.Splash:
                    state = States.Step1;
                    headerScreen.SetActive(true);
                    ShowScreen(step1Screen);
                    break;
                case States.Step1:
                    state = States.Step2;
                    ShowScreen(step2Screen);
                    break;
                case States.Step2:
                    state = States.Step3;
                    ShowScreen(step3Screen);
                    break;
                case States.Step3:
                    OnConnectWallet();
                    break;
            }
        }

        private float timeCounter = 0.0f;
        private const string cheatcode = "nowallet";
        private int codeIndex = 0;
        private float cheatTimeOut = 10.0f;

        private bool cheatActive = false;

        private void Update()
        {
            if (state != States.Splash)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    OnNext();
                }
            }

            splashTimer += Time.deltaTime / splashDuration;
            if (splashTimer >= 1.0f && state == States.Splash)
            {
                splashTimer = 0.0f;
                OnNext();
            }

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

                        EmergenceServices.Instance.SkipWallet(cheatActive, accessTokenJson);
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
