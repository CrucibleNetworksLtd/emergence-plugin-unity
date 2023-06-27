using System;
using EmergenceSDK.Internal.UI.Screens;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace EmergenceSDK.Types
{
    public class EmergenceSingleton : SingletonComponent<EmergenceSingleton>
    {
        private GameObject ui;
        private string accessToken;
        private string address;

        public static string HAS_LOGGED_IN_ONCE_KEY = "HasLoggedInOnce";

        public EmergenceConfiguration Configuration;

        public string CurrentDeviceId { get; set; }

        public event Action OnGameClosing;
        
        public enum Environment
        {
            Staging,
            Production
        }

        public Environment _environment = new Environment();
        

        [Header("Keyboard shortcut to open Emergence")] [SerializeField]
        private Key key = Key.Tab;

        [SerializeField] private bool shift = true;

        [SerializeField] private bool ctrl = false;

        [Serializable]
        public class EmergenceUIStateChanged : UnityEvent<bool>
        {
        }

        [Serializable]
        public class EmergenceUIOpened : UnityEvent
        {
        }

        [Serializable]
        public class EmergenceUIClosed : UnityEvent
        {
        }

        [Header("Events")] public EmergenceUIOpened OnEmergenceUIOpened;
        public EmergenceUIClosed OnEmergenceUIClosed;

        // Not showing this event in the Inspector because the actual visibility 
        // parameter value would be overwritten by the value set in the inspector
        [HideInInspector] public EmergenceUIStateChanged OnEmergenceUIVisibilityChanged;
        public EmergencePersona CurrentCachedPersona { get; set; }

        public void OpenEmergenceUI()
        {
            if (ScreenManager.Instance == null)
            {
                ui.SetActive(true);
                GameObject UIRoot = Instantiate(Resources.Load<GameObject>("Emergence Root"));
                UIRoot.name = "Emergence UI Overlay";
                UIRoot.GetComponentInChildren<EventSystem>().enabled = true;
                ui?.SetActive(false);
                ScreenManager.Instance.gameObject.SetActive(true);
                SaveCursor();
                UpdateCursor();
                OnEmergenceUIOpened.Invoke();
                OnEmergenceUIVisibilityChanged?.Invoke(true);
                
            }
            else
            {
                if (!ScreenManager.Instance.IsVisible)
                {
                    ScreenManager.Instance.gameObject.SetActive(true);
                    ScreenManager.Instance.ResetToOnBoardingIfNeeded();
                    SaveCursor();
                    UpdateCursor();
                    OnEmergenceUIOpened.Invoke();
                    OnEmergenceUIVisibilityChanged?.Invoke(true);
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
            RestoreCursor();
            OnEmergenceUIClosed.Invoke();
            OnEmergenceUIVisibilityChanged?.Invoke(false);
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

        #region Monobehaviour
        
        private new void Awake() 
        {
            if (transform.childCount < 1)
            {
                EmergenceLogger.LogError("Missing children");
                return;
            }
            
            ScreenManager.OnButtonEsc += EmergenceManager_OnButtonEsc;
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

        private void Update()
        {
            bool shortcutPressed = Keyboard.current[key].wasPressedThisFrame
                                   && (shift && (Keyboard.current[Key.LeftShift].isPressed ||
                                                 Keyboard.current[Key.RightShift].isPressed) || !shift)
                                   && (ctrl && (Keyboard.current[Key.LeftCtrl].isPressed ||
                                                Keyboard.current[Key.RightCtrl].isPressed) || !ctrl);


            if (shortcutPressed)
            {
                OpenEmergenceUI();
            }

            if (Keyboard.current[Key.Escape].wasPressedThisFrame)
            {
                if (ScreenManager.Instance != null)
                {
                    if (ScreenManager.Instance.IsVisible)
                    {
                        CloseEmergenceUI();
                    }
                }
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && ScreenManager.Instance != null && ScreenManager.Instance.IsVisible)
            {
                UpdateCursor();
            }
        }

        #endregion Monobehaviour

        #region Cursor Handling

        private CursorLockMode previousCursorLockMode = CursorLockMode.None;
        private bool previousCursorVisible = false;

        private void SaveCursor()
        {
            previousCursorLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
        }

        private void UpdateCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void RestoreCursor()
        {
            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
        }

        #endregion Cursor Handling

        private void EmergenceManager_OnButtonEsc()
        {
            CloseEmergenceUI();
        }
        
        private void OnApplicationQuit()
        {
            OnGameClosing?.Invoke();
        }

        private void OnApplicationPlaymodeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.ExitingEditMode)
            {
                OnGameClosing?.Invoke();
            }
        }

    }
}
