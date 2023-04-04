using EmergenceSDK.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI.Screens
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
        
        private IWalletService walletService;
        
        private void Awake()
        {
            skipButton.onClick.AddListener(OnConnectWallet);
            space1Button.onClick.AddListener(OnNext);
            space2Button.onClick.AddListener(OnNext);
            space3Button.onClick.AddListener(OnNext);

            Reset();
        }

        private void Start()
        {
            walletService = EmergenceServices.GetService<IWalletService>();
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

        private void Update()
        {
            if (state != States.Splash)
            {
                if (Keyboard.current[Key.Space].wasPressedThisFrame)
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
        }

        private void OnConnectWallet()
        {
            ScreenManager.Instance.ShowLogIn();
        }
    }
}
