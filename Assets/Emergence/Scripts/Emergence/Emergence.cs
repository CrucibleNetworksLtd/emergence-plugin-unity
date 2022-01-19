using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace EmergenceSDK
{
    public class Emergence : MonoBehaviour
    {
        [Header("Emergence Server")]
        [SerializeField]
        private string customEmergenceServerLocation;

        [SerializeField]
        private string nodeURL;

        [SerializeField]
        private string gameId;

        [Header("Keyboard shortcut to open Emergence")]
        [SerializeField]
        private KeyCode key = KeyCode.Tab;

        [SerializeField]
        private bool shift = true;

        [SerializeField]
        private bool ctrl = false;

        private GameObject ui;

        [Serializable]
        public class EmergenceUIStateChanged : UnityEvent<bool> { }

        [Serializable]
        public class EmergenceUIOpened: UnityEvent { }

        [Serializable]
        public class EmergenceUIClosed: UnityEvent { }

        [Header("Events")]
        public EmergenceUIOpened OnEmergenceUIOpened;
        public EmergenceUIClosed OnEmergenceUIClosed;

        // Not showing this event in the Inspector because the actual visibility 
        // parameter value would be overwritten by the value set in the inspector
        [HideInInspector]
        public EmergenceUIStateChanged OnEmergenceUIVisibilityChanged;

        public static Emergence Instance;

        public bool IsUIVisible
        {
            get
            {
                return ScreenManager.Instance != null && ScreenManager.Instance.IsVisible;
            }
        }

        private void Awake()
        {
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
        }

        private void EmergenceManager_OnButtonEsc()
        {
            CloseOverlay();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            ScreenManager.OnButtonEsc -= EmergenceManager_OnButtonEsc;
        }

        private void OnApplicationQuit()
        {
            Services.Instance.StopEVMServer();
        }

        private void Start()
        {
            if (transform.childCount < 1)
            {
                Debug.LogError("Missing children");
                return;
            }
            Services.Instance.SetupAndStartEVMServer(nodeURL, gameId);

            ui = transform.GetChild(0).gameObject;
            ui.SetActive(false);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && ScreenManager.Instance != null && ScreenManager.Instance.IsVisible)
            {
                UpdateCursor();
            }
        }

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

        private CursorLockMode previousCursorLockMode = CursorLockMode.None;
        private bool previousCursorVisible = false;
        private void Update()
        {
            bool shortcutPressed = Input.GetKeyDown(key)
                           && (shift && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) || !shift)
                           && (ctrl && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) || !ctrl);

            if (shortcutPressed)
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
                        SaveCursor();
                        UpdateCursor();
                        OnEmergenceUIOpened.Invoke();
                        OnEmergenceUIVisibilityChanged?.Invoke(true);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
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
        }

        private void CloseOverlay()
        {
            ScreenManager.Instance.gameObject.SetActive(false);
            RestoreCursor();
            OnEmergenceUIClosed.Invoke();
            OnEmergenceUIVisibilityChanged?.Invoke(false);
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name.Equals("Emergence"))
            {
                Debug.Log("Loaded");
                ui?.SetActive(false);
                ScreenManager.Instance.gameObject.SetActive(true);
                SaveCursor();
                UpdateCursor();
                OnEmergenceUIOpened.Invoke();
                OnEmergenceUIVisibilityChanged?.Invoke(true);
            }
        }
    }
}