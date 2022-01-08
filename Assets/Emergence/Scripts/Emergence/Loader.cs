using UnityEngine;
using UnityEngine.SceneManagement;

namespace Emergence
{
    public class Loader : MonoBehaviour
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

        public delegate void EmergenceUI(bool visible);
        public static event EmergenceUI OnEmergenceUI;

        private void Awake()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            EmergenceManager.OnButtonEsc += EmergenceManager_OnButtonEsc;
            DontDestroyOnLoad(gameObject);
        }

        private void EmergenceManager_OnButtonEsc()
        {
            CloseOverlay();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            EmergenceManager.OnButtonEsc -= EmergenceManager_OnButtonEsc;
        }

        private void OnApplicationQuit()
        {
            NetworkManager.Instance.StopEVMServer();
        }

        private void Start()
        {
            if (transform.childCount < 1)
            {
                Debug.LogError("Missing children");
                return;
            }
            NetworkManager.Instance.SetupAndStartEVMServer(nodeURL, gameId);

            ui = transform.GetChild(0).gameObject;
            ui.SetActive(false);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && EmergenceManager.Instance != null && EmergenceManager.Instance.IsVisible)
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
                if (EmergenceManager.Instance == null)
                {
                    ui.SetActive(true);
                    SceneManager.LoadSceneAsync("Emergence", LoadSceneMode.Additive);
                }
                else
                {
                    if (!EmergenceManager.Instance.IsVisible)
                    {
                        EmergenceManager.Instance.gameObject.SetActive(true);
                        SaveCursor();
                        UpdateCursor();
                        OnEmergenceUI?.Invoke(true);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (EmergenceManager.Instance != null)
                {
                    if (EmergenceManager.Instance.IsVisible)
                    {
                        EmergenceManager.Instance.gameObject.SetActive(false);
                        RestoreCursor();
                        OnEmergenceUI?.Invoke(false);
                    }
                    else
                    {
                        EmergenceManager.Instance.gameObject.SetActive(true);
                        OnEmergenceUI?.Invoke(true);
                        UpdateCursor();
                        ModalPromptYESNO.Instance.Show("Quit", "Are you sure?", () =>
                        {
                            Application.Quit();
                        },
                        () =>
                        {
                            EmergenceManager.Instance.gameObject.SetActive(false);
                            OnEmergenceUI?.Invoke(false);
                            RestoreCursor();
                        });
                    }
                }
            }
        }

        private void CloseOverlay()
        {
            EmergenceManager.Instance.gameObject.SetActive(false);
            RestoreCursor();
            OnEmergenceUI?.Invoke(false);
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name.Equals("Emergence"))
            {
                Debug.Log("Loaded");
                ui?.SetActive(false);
                EmergenceManager.Instance.gameObject.SetActive(true);
                SaveCursor();
                UpdateCursor();
                OnEmergenceUI?.Invoke(true);
            }
        }
    }
}