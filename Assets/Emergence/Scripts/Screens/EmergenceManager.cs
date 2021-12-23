using System;
using UnityEngine;
using UnityEngine.UI;

namespace Emergence
{
    public class EmergenceManager : MonoBehaviour
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

        public static EmergenceManager Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
            ChangeState(this.state);
            escButton.onClick.AddListener(OnEscButtonPressed);
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
                        NetworkManager.Instance.Ping(() =>
                            {
                                Modal.Instance.Hide();
                                Debug.Log("EVM server found");
                                checkingForServer = false;
                                ShowWelcome();
                            },
                            (error, code) =>
                            {
                                Modal.Instance.Show("Server not found, trying to launch");
                                Debug.LogWarning("EVM code not running, trying to launch");

                                try
                                {
                                    // TODO send process id set a reference to the current process and use System.Diagnostics's Process.Id property:int nProcessID = Process.GetCurrentProcess().Id;
                                    System.Diagnostics.Process.Start("run-server.bat");
                                    Debug.Log("Running Emergence Server");
                                    Modal.Instance.Hide();
                                    checkingForServer = false;
                                    ShowWelcome();
                                }
                                catch (Exception e)
                                {
                                    Debug.Log("Server error: " + e.Message);
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