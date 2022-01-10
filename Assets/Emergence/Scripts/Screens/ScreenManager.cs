using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Emergence
{
    public class ScreenManager : MonoBehaviour
    {
        [Header("Screen references")]
        [SerializeField]
        private GameObject welcomeScreen;

        [SerializeField]
        private GameObject logInScreen;

        [SerializeField]
        private GameObject dashboardScreen;

        [SerializeField]
        private GameObject editPersonaScreen;

        [Header("UI Reference")]
        public Button escButton;

        private enum ScreenStates
        {
            WaitForServer,
            Welcome,
            LogIn,
            Dashboard,
            EditPersona,
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
            ChangeState(this.state);
            escButton.onClick.AddListener(OnEscButtonPressed);

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

        private void Start()
        {
            // Get all the content size fitters in scroll areas and enable them for runtime
            // Disabling them on edit time avoids dirtying the scene as soon as it loads
            ContentSizeFitter[] csf = gameObject.GetComponentsInChildren<ContentSizeFitter>(true);

            for (int i = 0; i < csf.Length; i++)
            {
                csf[i].enabled = true;
            }
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
                        NetworkManager.Instance.IsConnected((connected) =>
                            {
                                Modal.Instance.Hide();
                                Debug.Log("EVM server found");
                                checkingForServer = false;

                                if (connected && NetworkManager.Instance.HasAccessToken)
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

                                if (NetworkManager.Instance.StartEVMServer())
                                {
                                    Modal.Instance.Hide();
                                    checkingForServer = false;
                                    ShowWelcome();
                                }
                                else
                                {
                                    Debug.LogWarning("Couldn't launch EVM Server");
                                    ModalPromptOK.Instance.Show("Error running server");
                                    checkingForServer = false;
                                }
                            });
                    }
                    break;
            }
        }

        public delegate void ButtonEsc();

        public static event ButtonEsc OnButtonEsc;

        private void OnEscButtonPressed()
        {
            OnButtonEsc?.Invoke();
        }

        private void ChangeState(ScreenStates state)
        {
            welcomeScreen.SetActive(false);
            logInScreen.SetActive(false);
            dashboardScreen.SetActive(false);
            editPersonaScreen.SetActive(false);

            this.state = state;

            switch (state)
            {
                case ScreenStates.WaitForServer:
                    // TODO modal
                    Debug.Log("Waiting for server");
                    break;
                case ScreenStates.Welcome:
                    welcomeScreen.SetActive(true);
                    break;
                case ScreenStates.LogIn:
                    logInScreen.SetActive(true);
                    break;
                case ScreenStates.Dashboard:
                    dashboardScreen.SetActive(true);
                    break;
                case ScreenStates.EditPersona:
                    editPersonaScreen.SetActive(true);
                    break;
            }
        }

        public void ShowWelcome()
        {
            ChangeState(ScreenStates.Welcome);
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

        public void Restart()
        {
            LogInScreen.Instance.Restart();
            state = ScreenStates.WaitForServer;
        }
    }
}