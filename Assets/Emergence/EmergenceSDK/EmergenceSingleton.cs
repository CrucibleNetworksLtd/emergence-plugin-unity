using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal;
using EmergenceSDK.Internal.UI;
using EmergenceSDK.Internal.UI.Screens;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EmergenceSDK
{
    public sealed class EmergenceSingleton : InternalEmergenceSingleton
    {
        public string CurrentDeviceId { get; set; }
        
        public GameEvents OnGameClosing = new();

        public ReconnectionQR ReconnectionQr => reconnectionQR ??= GetComponentInChildren<ReconnectionQR>(true);
        public EmergenceEnvironment Environment => CurrentForcedEnvironment ?? environment;
        public UICursorHandler CursorHandler => cursorHandler ??= cursorHandler = new UICursorHandler();
        
        public EmergenceConfiguration Configuration;
        [SerializeField] private EmergenceEnvironment environment;
        private UICursorHandler cursorHandler;
        private GameObject ui;
        private string accessToken;
        private InputAction closeAction;

        [Header("Keyboard shortcut to open Emergence")] 
        [SerializeField]
        private KeyCode Key = KeyCode.Z;

        [SerializeField] 
        private bool Shift = false;

        [SerializeField] 
        private bool Ctrl = false;

        [Header("Events")] 
        public EmergenceUIEvents.EmergenceUIOpened EmergenceUIOpened;
        public EmergenceUIEvents.EmergenceUIClosed EmergenceUIClosed;

        // Not showing this event in the Inspector because the actual visibility 
        // parameter value would be overwritten by the value set in the inspector
        [HideInInspector] 
        public EmergenceUIEvents.EmergenceUIStateChanged EmergenceUIVisibilityChanged;
        public EmergencePersona CurrentCachedPersona { get; set; }

        [Header("Set the emergence SDK log level")]
        public EmergenceLogger.LogLevel LogLevel;
        
        private ReconnectionQR reconnectionQR;
        private ISessionService sessionService;

        /// <summary>
        /// Opens the Emergence Overlay
        /// </summary>
        public void OpenEmergenceUI()
        {
            if (ScreenManager.Instance == null)
            {
                OpenOverlayFirstTime();
            }
            else
            {
                if (!ScreenManager.Instance.IsVisible)
                {
                    ScreenManager.Instance.gameObject.SetActive(true);
                    
                    if (sessionService.IsLoggedIn && ScreenManager.Instance.ScreenState < ScreenManager.ScreenStates.Dashboard)
                    {
                        ScreenManager.Instance.ShowDashboard().Forget();
                    }
                    
                    CursorHandler.SaveCursor();
                    CursorHandler.UpdateCursor();
                    EmergenceUIOpened.Invoke();
                    EmergenceUIVisibilityChanged?.Invoke(true);
                }
            }
        }

        public static Dictionary<string, string> DeviceIdHeader => new() { { "deviceId", Instance.CurrentDeviceId } };
        
        private void OpenOverlayFirstTime()
        {
            ui.SetActive(true);
            GameObject UIRoot = Instantiate(Resources.Load<GameObject>("Emergence Root"));
            UIRoot.name = "Emergence UI Overlay";
            ui.SetActive(false);
            ScreenManager.Instance.gameObject.SetActive(true);
            ScreenManager.Instance.ShowWelcome().Forget();
            CursorHandler.SaveCursor();
            CursorHandler.UpdateCursor();
            EmergenceUIOpened.Invoke();
            EmergenceUIVisibilityChanged?.Invoke(true);
        }

        /// <summary>
        /// Closes the Emergence Overlay
        /// </summary>
        public void CloseEmergenceUI()
        {
            if (ScreenManager.Instance == null)
            {
                return;
            }

            ScreenManager.Instance.gameObject.SetActive(false);
            CursorHandler.RestoreCursor();
            EmergenceUIClosed.Invoke();
            EmergenceUIVisibilityChanged?.Invoke(false);
        }
        
        private void OnEnable()
        {
            closeAction.Enable();
        }

        private void OnDisable()
        {
            closeAction.Disable();
        }

        private new void Awake()
        {
            EmergenceServiceProvider.OnServicesLoaded += _ => sessionService = EmergenceServiceProvider.GetService<ISessionService>();
            
            closeAction = new InputAction("CloseAction", binding: "<Keyboard>/escape");
            closeAction.performed += _ => CloseUI();
            
            if (transform.childCount < 1)
            {
                EmergenceLogger.LogError("Missing children");
                return;
            }
            
            ScreenManager.ClosingUI += CloseEmergenceUI;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            if (transform.childCount < 1)
            {
                EmergenceLogger.LogError("Missing children");
                return;
            }

            ui = transform.GetChild(0).gameObject;
            ui.SetActive(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(Key))
            {
                var shiftCheck = !Shift || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                var ctrlCheck = !Ctrl || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                var necessaryModifiersPressed = shiftCheck && ctrlCheck;
                if (!necessaryModifiersPressed)
                {
                    return;
                }
                ToggleUI();
            }
        }
        
        private void ToggleUI()
        {
            if (ScreenManager.Instance == null)
            {
                OpenOverlayFirstTime();
            }
            else if (ScreenManager.Instance.IsVisible)
            {
                CloseEmergenceUI();
            }
            else
            {
                OpenEmergenceUI();
            }
        }

        private void CloseUI()
        {
            if (ScreenManager.Instance != null)
            {
                if (ScreenManager.Instance.IsVisible)
                {
                    CloseEmergenceUI();
                }
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && ScreenManager.Instance != null && ScreenManager.Instance.IsVisible)
            {
                CursorHandler.UpdateCursor();
            }
        }

        private async void OnApplicationQuit()
        {
            await OnGameClosing.Invoke();
        }
        
#if UNITY_EDITOR
        private void OnApplicationPlaymodeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.ExitingEditMode)
            {
                OnGameClosing?.Invoke();
            }
        }
#endif
        protected override void InitializeDefault()
        {
            base.InitializeDefault();
            Configuration = Resources.Load<EmergenceConfiguration>("Configuration");
        }
    }
}
