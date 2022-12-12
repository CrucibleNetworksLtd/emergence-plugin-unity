using System;
using StarterAssets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

namespace EmergenceSDK
{
    public class Emergence : MonoBehaviour
    {
        [Header("Emergence Configuration")]
        //[SerializeField]
        //private string customEmergenceServerLocation;


        [SerializeField]
        private string nodeURL;


        [SerializeField]
        private string tokenSymbol = "MATIC";

        [Header("EVM Server")]
        [SerializeField]
        private bool launchEVMServerOnAwake = false;

        [SerializeField]
        private bool launchEVMServerOnStart = true;

        // [Header("Keyboard shortcut to open Emergence")]
        // [SerializeField]
        // private KeyCode key = KeyCode.Tab;
        //
        // [SerializeField] 
        // private bool shift = true;
        //
        // [SerializeField]
        // private bool ctrl = false;

        private GameObject ui;

        [Serializable]
        public class EmergenceUIStateChanged : UnityEvent<bool> { }

        [Serializable]
        public class EmergenceUIOpened : UnityEvent { }

        [Serializable]
        public class EmergenceUIClosed : UnityEvent { }

        [Header("Events")]
        public EmergenceUIOpened OnEmergenceUIOpened;
        public EmergenceUIClosed OnEmergenceUIClosed;

        // Not showing this event in the Inspector because the actual visibility 
        // parameter value would be overwritten by the value set in the inspector
        [HideInInspector]
        public EmergenceUIStateChanged OnEmergenceUIVisibilityChanged;

        public static Emergence Instance;

        public const string HAS_LOGGED_IN_ONCE_KEY = "HasLoggedInOnce";

        public bool IsUIVisible
        {
            get
            {

                return ScreenManager.Instance != null && ScreenManager.Instance.IsVisible;
            }
        }

        public string TokenSymbol
        {
            get
            {
                return tokenSymbol;
            }
        }

        #region Overlay

        public void OpenOverlay()
        {
            if (ScreenManager.Instance == null)
            {
                ui.SetActive(true);
                SceneManager.LoadSceneAsync("Emergence", LoadSceneMode.Additive);
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
            
            // Disable player controls if found
            ThirdPersonController charController = FindObjectOfType<ThirdPersonController>();
            if (charController != null)
            {
                charController.enabled = false;
            }
        }

        public void CloseOverlay()
        {
            if (ScreenManager.Instance == null)
            {
                return;
            }

            ScreenManager.Instance.gameObject.SetActive(false);
            RestoreCursor();
            OnEmergenceUIClosed.Invoke();
            OnEmergenceUIVisibilityChanged?.Invoke(false);
            
            // Enable player controls
            ThirdPersonController charController = FindObjectOfType<ThirdPersonController>();
            if (charController != null)
            {
                charController.enabled = true;
            }
        }

        #endregion Overlay

        #region Monobehaviour

        private void Awake()
        {
            if (transform.childCount < 1)
            {
                Debug.LogError("Missing children");
                return;
            }

            if (Instance != null)
            {
                Debug.LogError($"Emergence prefab instance already exists, removing this GameObject from the scene [{gameObject.name}]");
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            ScreenManager.OnButtonEsc += EmergenceManager_OnButtonEsc;
            DontDestroyOnLoad(gameObject);

            if (launchEVMServerOnAwake)
            {
                LocalEmergenceServer.Instance.LaunchLocalServerProcess(EmergenceSingleton.Instance.LaunchHidden);
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            ScreenManager.OnButtonEsc -= EmergenceManager_OnButtonEsc;
        }

        private void OnApplicationQuit()
        {
            LocalEmergenceServer.Instance.KillLocalServerProcess();
        }

        private void Start()
        {
            if (transform.childCount < 1)
            {
                Debug.LogError("Missing children");
                return;
            }

            if (launchEVMServerOnStart && !launchEVMServerOnAwake)
            {
                LocalEmergenceServer.Instance.LaunchLocalServerProcess(EmergenceSingleton.Instance.LaunchHidden);
            }

            ui = transform.GetChild(0).gameObject;
            ui.SetActive(false);
        }

        private void Update()
        {
            // bool shortcutPressed = Input.GetKeyDown(key)
            //                && (shift && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) || !shift)
            //                && (ctrl && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) || !ctrl);

            bool shortcutPressed = Keyboard.current.tabKey.wasPressedThisFrame;

            if (shortcutPressed)
            {
                OpenOverlay(); 
            }

            //if (Input.GetKeyDown(KeyCode.Escape))
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (ScreenManager.Instance != null)
                {
                    if (ScreenManager.Instance.IsVisible)
                    {
                        ScreenManager.Instance.gameObject.SetActive(false);
                        RestoreCursor();
                        OnEmergenceUIClosed.Invoke();
                        OnEmergenceUIVisibilityChanged?.Invoke(false);
                    }
                }
            }

            #region Tests
            /*
            if (Input.GetKeyDown(KeyCode.C))
            {
                string path = "C:\\dev\\wallet.json";
                string password = "password";

                Services.Instance.CreateWallet(path, password, () =>
                {
                    Debug.Log("Success CreateWallet");
                },
                (error, code) =>
                {
                    Debug.LogError("[" + code + "] " + error);
                });
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                string privateKey = "0cb5384dadcc8ed56d09ade8f87949ab3b7c237c4378621b57dfd3cd7c5046c6";
                string password = "password";
                string publicKey = "0xb674c35ca4607EB1CF1c58c36eb69972818770Fd";
                string path = "C:\\dev\\wallet.json";

                Services.Instance.CreateKeyStore(privateKey, password, publicKey, path, () =>
                {
                    Debug.Log("Success CreateKeyStore");
                },
                (error, code) =>
                {
                    Debug.LogError("[" + code + "] " + error);
                });
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                string name = string.Empty;
                string password = "password";
                string path = "C:\\dev\\wallet.json";
                string nodeURL = "https://polygon-mainnet.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134";

                Services.Instance.LoadAccount(name, password, path, nodeURL, () =>
                {
                    Debug.Log("Success LoadAccount");
                },
                (error, code) =>
                {
                    Debug.LogError("[" + code + "] " + error);
                });
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                Services.Instance.GetAccessToken((accessToken) =>
                {
                    Debug.Log($"Success accesstoken {accessToken}");
                    ScreenManager.Instance.ShowDashboard();
                },
                (error, code) =>
                {
                    Debug.LogError("[" + code + "] " + error);
                });
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                Services.Instance.GetBalance((balance) =>
                {
                    Debug.Log($"Success balance {balance}");
                    ScreenManager.Instance.ShowDashboard();
                },
                (error, code) =>
                {
                    Debug.LogError("[" + code + "] " + error);
                });
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                Services.Instance.ValidateAccessToken((valid) =>
                {
                    Debug.Log($"Valid Access token: {valid}");
                },
                (error, code) =>
                {
                    Debug.LogError("[" + code + "] " + error);
                });
            }*/

            #endregion Tests
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

        #region Events

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name.Equals("Emergence"))
            {
                Debug.Log("Emergence overlay scene Loaded");
                ui?.SetActive(false);
                ScreenManager.Instance.gameObject.SetActive(true);
                SaveCursor();
                UpdateCursor();
                OnEmergenceUIOpened.Invoke();
                OnEmergenceUIVisibilityChanged?.Invoke(true);
            }
        }

        private void EmergenceManager_OnButtonEsc()
        {
            CloseOverlay();
        }

        #endregion Events
    }
}