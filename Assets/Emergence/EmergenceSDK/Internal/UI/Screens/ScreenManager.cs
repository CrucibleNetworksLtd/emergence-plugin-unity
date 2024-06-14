using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Implementations.Login;
using EmergenceSDK.Internal.UI.Personas;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI.Screens
{
    public class ScreenManager : MonoBehaviour
    {
        [Header("Screen references")]
        [SerializeField]
        private GameObject WelcomeScreen;

        [SerializeField]
        private GameObject ScreensRoot;

        [SerializeField]
        private GameObject LogInScreen;

        [SerializeField]
        private GameObject DashboardScreen;

        [SerializeField]
        private GameObject EditPersonaScreen;
        
        [SerializeField]
        private GameObject MyCollectionScreen;

        [Header("UI Reference")]
        public Button EscButton;
        public Button EscButtonOnboarding;
        public Button EscButtonLogin;
        public Button BackButtonLogin;
        public Button PersonasButton;
        public Button CollectionButton;
        public Toggle PersonasToggle;
        public Toggle CollectionToggle;

        
        [Header("PersonaUI")]
        [SerializeField]
        private Transform PersonaScrollContents;
        [SerializeField]
        private Pool PersonaButtonPool;
        [SerializeField]
        private PersonaCarousel PersonaCarousel;
        private PersonaUIManager personaUIManager;
        
        [SerializeField]
        public GameObject DisconnectModal;

        public static Action ClosingUI;
        
        private InputAction escAction;

        internal enum ScreenStates
        {
            WaitForServer,
            Welcome,
            LogIn,
            Dashboard,
            EditPersona,
            Collection,
        }

        internal ScreenStates ScreenState { get; set; } = ScreenStates.WaitForServer;
        private ISessionService sessionService;

        public static ScreenManager Instance { get; private set; }

        public bool IsVisible => gameObject.activeSelf;

        private void Awake()
        {
            Instance = this;

            sessionService = EmergenceServiceProvider.GetService<ISessionService>();
            EscButton.onClick.AddListener(OnEscButtonPressed);
            EscButtonOnboarding.onClick.AddListener(OnEscButtonPressed);
            EscButtonLogin.onClick.AddListener(OnEscButtonPressed);
            BackButtonLogin.onClick.AddListener(() =>
            {
                if (BackButtonLogin.GetComponentInParent<LoginManager>()?.IsBusy != true)
                {
                    OnEscButtonPressed();
                }
            });

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
            BackButtonLogin.onClick.RemoveListener(OnEscButtonPressed);

            PersonasToggle.onValueChanged.RemoveListener(OnPersonaButtonPressed);
            CollectionToggle.onValueChanged.RemoveListener(OnCollectionButtonPressed);
        }

        private async void Start()
        {
            // Get all the content size fitters in scroll areas and enable them for runtime
            // Disabling them on edit time avoids dirtying the scene as soon as it loads
            ContentSizeFitter[] csf = gameObject.GetComponentsInChildren<ContentSizeFitter>(true);

            foreach (var fitter in csf)
            {
                fitter.enabled = true;
            }
            
            personaUIManager = new PersonaUIManager(DashboardScreen.GetComponent<DashboardScreen>(), PersonaButtonPool, PersonaCarousel, PersonaScrollContents);
        }

        private async UniTask ChangeState(ScreenStates newState)
        {
            WelcomeScreen.SetActive(false);
            LogInScreen.SetActive(false);
            DashboardScreen.SetActive(false);
            EditPersonaScreen.SetActive(false);
            DisconnectModal.SetActive(false);
            MyCollectionScreen.SetActive(false);

            ScreenState = newState;

            switch (ScreenState)
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
                    LogInScreen.SetActive(true);
                    ScreensRoot.SetActive(false);
                    LogInScreen.GetComponent<LogInScreen>().SetupLogin().Forget();
                    break;
                case ScreenStates.Dashboard:
                    ScreensRoot.SetActive(true);
                    DashboardScreen.SetActive(true);
                    personaUIManager.Refresh().Forget();
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
            if (sessionService is { IsLoggedIn: true })
            {
                await ShowDashboard();
                return;
            }
            
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
            return ChangeState(ScreenStates.LogIn);
        }
    }
}
