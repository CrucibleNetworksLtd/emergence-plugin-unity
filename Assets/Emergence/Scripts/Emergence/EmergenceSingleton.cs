using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.UI;
using EmergenceSDK.Internal.UI.Screens;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Types;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Environment = EmergenceSDK.ScriptableObjects.Environment;

namespace EmergenceSDK
{
    public class EmergenceSingleton : SingletonComponent<EmergenceSingleton>
    {

        public EmergenceConfiguration Configuration;

        public string CurrentDeviceId { get; set; }

        public event Action OnGameClosing;

        public Environment Environment = new Environment();
        public UICursorHandler CursorHandler => cursorHandler ??= cursorHandler = new UICursorHandler();
        
        private UICursorHandler cursorHandler;
        private GameObject ui;
        private string accessToken;
        private string address;
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
                    CursorHandler.SaveCursor();
                    CursorHandler.UpdateCursor();
                    EmergenceUIOpened.Invoke();
                    EmergenceUIVisibilityChanged?.Invoke(true);
                }
            }
        }

        private void OpenOverlayFirstTime()
        {
            ui.SetActive(true);
            GameObject UIRoot = Instantiate(Resources.Load<GameObject>("Emergence Root"));
            UIRoot.name = "Emergence UI Overlay";
            UIRoot.GetComponentInChildren<EventSystem>().enabled = true;
            ui.SetActive(false);
            ScreenManager.Instance.gameObject.SetActive(true);
            ScreenManager.Instance.ShowWelcome().Forget();
            CursorHandler.SaveCursor();
            CursorHandler.UpdateCursor();
            EmergenceUIOpened.Invoke();
            EmergenceUIVisibilityChanged?.Invoke(true);
        }

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

        public string GetCachedAddress()
        {
            return address;
        }

        public void SetCachedAddress(string _address)
        {
            EmergenceLogger.LogInfo("Setting cached address to: " + _address);
            address = _address;
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

        private void OnApplicationQuit()
        {
            OnGameClosing?.Invoke();
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
    }
}
