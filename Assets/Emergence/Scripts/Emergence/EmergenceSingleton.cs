using EmergenceSDK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace EmergenceSDK
{
    public class EmergenceSingleton : SingletonComponent<EmergenceSingleton>
    {
        private GameObject ui;
        private string accessToken;
        private string address;

        public static string HAS_LOGGED_IN_ONCE_KEY = "HasLoggedInOnce";

        public EmergenceConfiguration Configuration;

        public string CurrentDeviceId { get; set; }
        
        public enum Environment
        {
            Staging,
            Production
        }

        public Environment _environment = new Environment();
        

        [Header("Keyboard shortcut to open Emergence")] [SerializeField]
        private KeyCode key = KeyCode.Tab;

        [SerializeField] private bool shift = true;

        [SerializeField] private bool ctrl = false;

        [Serializable]
        public class EmergenceUIStateChanged : UnityEvent<bool>
        {
        }

        [Serializable]
        public class EmergenceCachedPersonaUpdated : UnityEvent
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
        public EmergenceCachedPersonaUpdated CachedPersonaUpdated;

        // Not showing this event in the Inspector because the actual visibility 
        // parameter value would be overwritten by the value set in the inspector
        [HideInInspector] public EmergenceUIStateChanged OnEmergenceUIVisibilityChanged;
        public EmergencePersona CurrentCachedPersona { get; set; }

        public void OpenEmergenceUI()
        {
            if (ScreenManager.Instance == null)
            {
                ui.SetActive(true);
                Instantiate(Resources.Load<GameObject>("Emergence Root")).name = "Emergence UI Overlay";
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
        
        public void CloseEmergeneUI()
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

        public string GetCurrentAccessToken()
            {
                return accessToken;
            }

            public bool HasAccessToken()
            {
                return !string.IsNullOrEmpty(accessToken);
            }

            public string GetCachedAddress()
            {
                return address;
            }

            public void SetCachedAddress(string _address)
            {
                Debug.Log("Setting cached address to: " + _address);
                address = _address;
            }

            public bool HasCachedAddress()
            {
                return !string.IsNullOrEmpty(address);
            }
            
            #region Monobehaviour
            
            private new void Awake() 
            {
                if (transform.childCount < 1)
                {
                    Debug.LogError("Missing children");
                    return;
                }
                
                ScreenManager.OnButtonEsc += EmergenceManager_OnButtonEsc;
                DontDestroyOnLoad(gameObject);
            }
            
            private void Start()
            {
                if (transform.childCount < 1)
                {
                    Debug.LogError("Missing children");
                    return;
                }

                ui = transform.GetChild(0).gameObject;
                ui.SetActive(false);
            }

            private void Update()
            {


                    bool shortcutPressed = Input.GetKeyDown(key)
                                           && (shift && (Input.GetKey(KeyCode.LeftShift) ||
                                                         Input.GetKey(KeyCode.RightShift)) || !shift)
                                           && (ctrl && (Input.GetKey(KeyCode.LeftControl) ||
                                                        Input.GetKey(KeyCode.RightControl)) || !ctrl);

                    if (shortcutPressed)
                    {
                        OpenEmergenceUI();
                    }

                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        Debug.Log("Esc");
                        if (ScreenManager.Instance != null)
                        {
                            if (ScreenManager.Instance.IsVisible)
                            {
                                CloseEmergeneUI();
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
            CloseEmergeneUI();
        }

    }
}
