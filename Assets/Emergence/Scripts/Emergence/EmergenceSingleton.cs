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

        [FormerlySerializedAs("_environment")] public Environment Environment = new Environment();
        public UICursorHandler CursorHandler => cursorHandler ??= cursorHandler = new UICursorHandler();
        
        private UICursorHandler cursorHandler;
        private GameObject ui;
        private string accessToken;
        private string address;
        private InputAction uiToggleAction;
        private InputAction closeAction;

        [FormerlySerializedAs("key")] [Header("Keyboard shortcut to open Emergence")] [SerializeField]
        private Key Key = Key.Tab;

        [FormerlySerializedAs("shift")] [SerializeField] private bool Shift = true;

        [FormerlySerializedAs("ctrl")] [SerializeField] private bool Ctrl = false;

        [FormerlySerializedAs("OnEmergenceUIOpened")] [Header("Events")] public EmergenceUIEvents.EmergenceUIOpened EmergenceUIOpened;
        [FormerlySerializedAs("OnEmergenceUIClosed")] public EmergenceUIEvents.EmergenceUIClosed EmergenceUIClosed;

        // Not showing this event in the Inspector because the actual visibility 
        // parameter value would be overwritten by the value set in the inspector
        [FormerlySerializedAs("OnEmergenceUIVisibilityChanged")] [HideInInspector] public EmergenceUIEvents.EmergenceUIStateChanged EmergenceUIVisibilityChanged;
        public EmergencePersona CurrentCachedPersona { get; set; }

        [Header("Set the emergence SDK log level")]
        public EmergenceLogger.LogLevel LogLevel;

        public void OpenEmergenceUI()
        {
            if (ScreenManager.Instance == null)
            {
                ui.SetActive(true);
                GameObject UIRoot = Instantiate(Resources.Load<GameObject>("Emergence Root"));
                UIRoot.name = "Emergence UI Overlay";
                UIRoot.GetComponentInChildren<EventSystem>().enabled = true;
                ui.SetActive(false);
                ScreenManager.Instance.gameObject.SetActive(true);
                CursorHandler.SaveCursor();
                CursorHandler.UpdateCursor();
                EmergenceUIOpened.Invoke();
                EmergenceUIVisibilityChanged?.Invoke(true);
                
            }
            else
            {
                if (!ScreenManager.Instance.IsVisible)
                {
                    ScreenManager.Instance.gameObject.SetActive(true);
                    ScreenManager.Instance.ShowWelcome().Forget();
                    CursorHandler.SaveCursor();
                    CursorHandler.UpdateCursor();
                    EmergenceUIOpened.Invoke();
                    EmergenceUIVisibilityChanged?.Invoke(true);
                }
            }
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
            uiToggleAction.Enable();
            closeAction.Enable();
        }

        private void OnDisable()
        {
            uiToggleAction.Disable();
            closeAction.Disable();
        }

        private new void Awake() 
        {
            uiToggleAction = new InputAction("UIToggle", binding: "<Keyboard>/tab");
            uiToggleAction.performed += _ => ToggleUI();

            closeAction = new InputAction("CloseAction", binding: "<Keyboard>/escape");
            closeAction.performed += _ => CloseUI();
            
            if (transform.childCount < 1)
            {
                EmergenceLogger.LogError("Missing children");
                return;
            }
            
            ScreenManager.Instance.ClosingUI += CloseEmergenceUI;
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
        
        private void ToggleUI()
        {
            if (ScreenManager.Instance != null)
            {
                if (ScreenManager.Instance.IsVisible)
                {
                    CloseEmergenceUI();
                }
                else
                {
                    OpenEmergenceUI();
                }
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
