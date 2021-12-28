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

        private void Awake()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            EmergenceManager.OnButtonEsc += EmergenceManager_OnButtonEsc;
            NetworkManager.Instance.SetupAndStartEVMServer(nodeURL, gameId);
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

            ui = transform.GetChild(0).gameObject;
            ui.SetActive(false);
        }

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
                    EmergenceManager.Instance.gameObject.SetActive(true);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (EmergenceManager.Instance != null)
                {
                    EmergenceManager.Instance.gameObject.SetActive(false);
                }
            }
        }

        private void CloseOverlay()
        {
            EmergenceManager.Instance.gameObject.SetActive(false);
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name.Equals("Emergence"))
            {
                Debug.Log("Loaded");
                ui?.SetActive(false);
            }
        }
    }
}