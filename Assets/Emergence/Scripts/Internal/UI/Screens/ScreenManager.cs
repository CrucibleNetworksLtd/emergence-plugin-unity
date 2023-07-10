using System;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI.Screens
{
    public class ScreenManager : MonoBehaviour
    {
        [Header("Screen references")]
        [SerializeField]
        private GameObject welcomeScreen;

        [SerializeField]
        private GameObject screensRoot;

        [SerializeField]
        private GameObject logInScreen;

        [SerializeField]
        private GameObject dashboardScreen;

        [SerializeField]
        private GameObject editPersonaScreen;
        
        [SerializeField]
        private GameObject myCollectionScreen;

        [Header("UI Reference")]
        public Button escButton;
        public Button escButtonOnboarding;
        public Button escButtonLogin;
        public Button personasButton;
        public Button collectionButton;
        public Toggle personasToggle;
        public Toggle collectionToggle;

        [SerializeField]
        public GameObject disconnectModal;

        private enum ScreenStates
        {
            WaitForServer,
            Welcome,
            LogIn,
            Dashboard,
            EditPersona,
            Collection,
        }

        private ScreenStates state = ScreenStates.WaitForServer;

        public static ScreenManager Instance { get; private set; }

        public bool IsVisible => gameObject.activeSelf;

        private void Awake()
        {
            Instance = this;
            escButton.onClick.AddListener(OnEscButtonPressed);
            escButtonOnboarding.onClick.AddListener(OnEscButtonPressed);
            escButtonLogin.onClick.AddListener(OnEscButtonPressed);
            personasToggle.onValueChanged.AddListener(OnPersonaButtonPressed);
            collectionToggle.onValueChanged.AddListener(OnCollectionButtonPressed);
        }

        private void OnDestroy()
        {
            escButton.onClick.RemoveListener(OnEscButtonPressed);
            escButtonOnboarding.onClick.RemoveListener(OnEscButtonPressed);
            escButtonLogin.onClick.RemoveListener(OnEscButtonPressed);
            personasToggle.onValueChanged.RemoveListener(OnPersonaButtonPressed);
            collectionToggle.onValueChanged.RemoveListener(OnCollectionButtonPressed);
        }

        private void Start()
        {
            // Get all the content size fitters in scroll areas and enable them for runtime
            // Disabling them on edit time avoids dirtying the scene as soon as it loads
            ContentSizeFitter[] csf = gameObject.GetComponentsInChildren<ContentSizeFitter>(true);

            for (int i = 0; i < csf.Length; i++)
            {
                csf[i].enabled = true;
            }

            ChangeState(this.state);
        }

        private void Update()
        {
            switch (state)
            {
                case ScreenStates.WaitForServer:
                    ShowWelcome();
                    break;
            }
        }

        public void ResetToOnBoardingIfNeeded()
        {
            if (PlayerPrefs.GetInt(StaticConfig.HasLoggedInOnceKey, 0) == 0)
            {
                ChangeState(ScreenStates.Welcome);
            }
        }

        public delegate void ButtonEsc();
        public static event ButtonEsc OnButtonEsc;

        private void OnEscButtonPressed()
        {
            OnButtonEsc?.Invoke();
        }

        public delegate void ButtonPersona();

        public static event ButtonPersona OnButtonPersona;

        private void OnPersonaButtonPressed(bool selected)
        {
            if (!selected) return;
            ShowDashboard();
            OnButtonPersona?.Invoke();
        }
        
        public delegate void ButtonCollection();

        public static event ButtonCollection OnButtonCollection;

        private void OnCollectionButtonPressed(bool selected)
        {
            if (!selected) return;
            ShowCollection();
            OnButtonCollection?.Invoke();
        }

        private void ChangeState(ScreenStates state)
        {
            welcomeScreen.SetActive(false);
            logInScreen.SetActive(false);
            dashboardScreen.SetActive(false);
            editPersonaScreen.SetActive(false);
            disconnectModal.SetActive(false);
            myCollectionScreen.SetActive(false);

            this.state = state;

            switch (state)
            {
                case ScreenStates.WaitForServer:
                    // TODO modal
                    EmergenceLogger.LogInfo("Waiting for server");
                    break;
                case ScreenStates.Welcome:
                    welcomeScreen.SetActive(true);
                    screensRoot.SetActive(false);
                    break;
                case ScreenStates.LogIn:
                    logInScreen.SetActive(true);
                    screensRoot.SetActive(false);
                    break;
                case ScreenStates.Dashboard:
                    screensRoot.SetActive(true);
                    dashboardScreen.SetActive(true);
                    break;
                case ScreenStates.EditPersona:
                    editPersonaScreen.SetActive(true);
                    screensRoot.SetActive(true);
                    break;
                case ScreenStates.Collection:
                    myCollectionScreen.SetActive(true);
                    screensRoot.SetActive(true);
                    break;
            }
        }

        public void ShowWelcome()
        {
            if (PlayerPrefs.GetInt(StaticConfig.HasLoggedInOnceKey, 0) > 0)
            {
                ShowLogIn();
            }
            else
            {
                ChangeState(ScreenStates.Welcome);
            }
        }

        public void ShowLogIn()
        {
            ChangeState(ScreenStates.LogIn);
        }

        public void ShowDashboard()
        {
            ChangeState(ScreenStates.Dashboard);
            DashboardScreen.Instance.Refresh();
        }

        public void ShowEditPersona()
        {
            ChangeState(ScreenStates.EditPersona);
        }

        public void ShowCollection()
        {
            ChangeState(ScreenStates.Collection);
            CollectionScreen.Instance.Refresh();
        }

        public void Restart()
        {
            LogInScreen.Instance.Restart();
            state = ScreenStates.WaitForServer;
        }
    }
}