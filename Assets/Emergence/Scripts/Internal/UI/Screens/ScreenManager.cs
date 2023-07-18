using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI.Screens
{
    public class ScreenManager : MonoBehaviour
    {
        [FormerlySerializedAs("welcomeScreen")]
        [Header("Screen references")]
        [SerializeField]
        private GameObject WelcomeScreen;

        [FormerlySerializedAs("screensRoot")] [SerializeField]
        private GameObject ScreensRoot;

        [FormerlySerializedAs("logInScreen")] [SerializeField]
        private GameObject LOGInScreen;

        [FormerlySerializedAs("dashboardScreen")] [SerializeField]
        private GameObject DashboardScreen;

        [FormerlySerializedAs("editPersonaScreen")] [SerializeField]
        private GameObject EditPersonaScreen;
        
        [FormerlySerializedAs("myCollectionScreen")] [SerializeField]
        private GameObject MyCollectionScreen;

        [FormerlySerializedAs("escButton")] [Header("UI Reference")]
        public Button EscButton;
        [FormerlySerializedAs("escButtonOnboarding")] public Button EscButtonOnboarding;
        [FormerlySerializedAs("escButtonLogin")] public Button EscButtonLogin;
        [FormerlySerializedAs("personasButton")] public Button PersonasButton;
        [FormerlySerializedAs("collectionButton")] public Button CollectionButton;
        [FormerlySerializedAs("personasToggle")] public Toggle PersonasToggle;
        [FormerlySerializedAs("collectionToggle")] public Toggle CollectionToggle;

        [FormerlySerializedAs("disconnectModal")] [SerializeField]
        public GameObject DisconnectModal;

        public Action ClosingUI;
        
        private InputAction escAction;

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

            EscButton.onClick.AddListener(OnEscButtonPressed);
            EscButtonOnboarding.onClick.AddListener(OnEscButtonPressed);
            EscButtonLogin.onClick.AddListener(OnEscButtonPressed);

            PersonasToggle.onValueChanged.AddListener(OnPersonaButtonPressed);
            CollectionToggle.onValueChanged.AddListener(OnCollectionButtonPressed);
            
            escAction = new InputAction("Esc", binding: "<Keyboard>/escape");
            escAction.performed += _ => OnEscButtonPressed();
        }

        private void OnEnable()
        {
            escAction.Enable();
        }

        private void OnDisable()
        {
            escAction.Disable();
        }
        
        private void OnDestroy()
        {
            EscButton.onClick.RemoveListener(OnEscButtonPressed);
            EscButtonOnboarding.onClick.RemoveListener(OnEscButtonPressed);
            EscButtonLogin.onClick.RemoveListener(OnEscButtonPressed);

            PersonasToggle.onValueChanged.RemoveListener(OnPersonaButtonPressed);
            CollectionToggle.onValueChanged.RemoveListener(OnCollectionButtonPressed);
        }

        private async void Start()
        {
            // Get all the content size fitters in scroll areas and enable them for runtime
            // Disabling them on edit time avoids dirtying the scene as soon as it loads
            ContentSizeFitter[] csf = gameObject.GetComponentsInChildren<ContentSizeFitter>(true);

            for (int i = 0; i < csf.Length; i++)
            {
                csf[i].enabled = true;
            }

            await ChangeState(this.state);
        }

        private async UniTask ChangeState(ScreenStates newState)
        {
            WelcomeScreen.SetActive(false);
            LOGInScreen.SetActive(false);
            DashboardScreen.SetActive(false);
            EditPersonaScreen.SetActive(false);
            DisconnectModal.SetActive(false);
            MyCollectionScreen.SetActive(false);

            state = newState;

            switch (state)
            {
                case ScreenStates.WaitForServer:
                    // TODO modal
                    EmergenceLogger.LogInfo("Waiting for server");
                    break;
                case ScreenStates.Welcome:
                    WelcomeScreen.SetActive(true);
                    ScreensRoot.SetActive(false);
                    break;
                case ScreenStates.LogIn:
                    LOGInScreen.SetActive(true);
                    ScreensRoot.SetActive(false);
                    break;
                case ScreenStates.Dashboard:
                    ScreensRoot.SetActive(true);
                    DashboardScreen.SetActive(true);
                    await Screens.DashboardScreen.Instance.Refresh();
                    break;
                case ScreenStates.EditPersona:
                    EditPersonaScreen.SetActive(true);
                    ScreensRoot.SetActive(true);
                    break;
                case ScreenStates.Collection:
                    MyCollectionScreen.SetActive(true);
                    ScreensRoot.SetActive(true);
                    await CollectionScreen.Instance.Refresh();
                    break;
            }
        }

        private void OnEscButtonPressed() => ClosingUI?.Invoke();

        private void OnPersonaButtonPressed(bool selected)
        {
            if (selected)
            {
                ShowDashboard().Forget();
            }
        }

        private void OnCollectionButtonPressed(bool selected)
        {
            if (selected)
            {
                ShowCollection().Forget();
            }
        }

        public async UniTask ShowWelcome()
        {
            if (PlayerPrefs.GetInt(StaticConfig.HasLoggedInOnceKey, 0) > 0)
            {
                await ShowLogIn();
            }
            else
            {
                await ChangeState(ScreenStates.Welcome);
            }
        }

        public UniTask ShowLogIn()
        {
            return ChangeState(ScreenStates.LogIn);
        }

        public UniTask ShowDashboard()
        {
            return ChangeState(ScreenStates.Dashboard);
        }

        public UniTask ShowEditPersona()
        {
            return ChangeState(ScreenStates.EditPersona);
        }

        public UniTask ShowCollection()
        {
            return ChangeState(ScreenStates.Collection);
        }

        public UniTask Restart()
        {
            LogInScreen.Instance.Restart();
            return ChangeState(ScreenStates.WaitForServer);
        }
    }
}
