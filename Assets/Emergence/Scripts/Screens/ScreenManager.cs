using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EmergenceSDK
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

        private bool checkingForServer = false;

        public static ScreenManager Instance { get; private set; }

        public bool IsVisible
        {
            get
            {
                return gameObject.activeSelf;
            }
        }

        private void Awake()
        {
            Instance = this;
            escButton.onClick.AddListener(OnEscButtonPressed);
            escButtonOnboarding.onClick.AddListener(OnEscButtonPressed);
            escButtonLogin.onClick.AddListener(OnEscButtonPressed);
            personasButton.onClick.AddListener(OnPersonaButtonPressed);
            collectionButton.onClick.AddListener(OnCollectionButtonPressed);

            GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                EventSystem[] ess = roots[i].GetComponentsInChildren<EventSystem>();

                if (ess.Length > 0)
                {
                    EventSystem es = gameObject.GetComponentInChildren<EventSystem>();
                    es.gameObject.SetActive(false);
                    break;
                }
            }
        }

        private void OnDestroy()
        {
            escButton.onClick.RemoveListener(OnEscButtonPressed);
            escButtonOnboarding.onClick.RemoveListener(OnEscButtonPressed);
            escButtonLogin.onClick.RemoveListener(OnEscButtonPressed);
            personasButton.onClick.RemoveListener(OnPersonaButtonPressed);
            collectionButton.onClick.RemoveListener(OnCollectionButtonPressed);
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
                    Modal.Instance.Show("Waiting for server...");
                    if (!checkingForServer)
                    {
                        checkingForServer = true;
                        Services.Instance.IsConnected((connected) =>
                            {
                                Modal.Instance.Hide();
                                Debug.Log("EVM server found");
                                checkingForServer = false;

                                if (connected && Services.Instance.HasAccessToken)
                                {
                                    ShowDashboard();
                                }
                                else
                                {
                                    ShowWelcome();
                                }
                            },
                            (error, code) =>
                            {
                                Modal.Instance.Show("Server not found, trying to launch");
                                Debug.LogWarning("EVM code not running, trying to launch");

                                LocalEmergenceServer.Instance.LaunchLocalServerProcess();
                                Modal.Instance.Hide();
                                checkingForServer = false;
                                ShowWelcome();

                                //if (LocalEmergenceServer.Instance.LaunchLocalServerProcess())
                                //{
                                //}
                                //else
                                //{
                                //    Debug.LogWarning("Couldn't launch EVM Server");
                                //    ModalPromptOK.Instance.Show("Error running server");
                                //    checkingForServer = false;
                                //}
                            });
                    }
                    break;
            }
        }

        public void ResetToOnBoardingIfNeeded()
        {
            if (PlayerPrefs.GetInt(Emergence.HAS_LOGGED_IN_ONCE_KEY, 0) == 0)
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

        private void OnPersonaButtonPressed()
        {
            ShowDashboard();
            OnButtonPersona?.Invoke();
        }
        
        public delegate void ButtonCollection();

        public static event ButtonCollection OnButtonCollection;

        private void OnCollectionButtonPressed()
        {
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
                    Debug.Log("Waiting for server");
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
            if (PlayerPrefs.GetInt(Emergence.HAS_LOGGED_IN_ONCE_KEY, 0) > 0)
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