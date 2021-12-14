using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

        private bool serverRunning = false;

        private GameObject ui;

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

        private void Start()
        {
            if (transform.childCount < 1)
            {
                UnityEngine.Debug.LogError("Missing children");
                return;
            }

            ui = transform.GetChild(0).gameObject;
            ui.SetActive(false);

            if (!serverRunning)
            {
                StartServer();
                serverRunning = true;
            }
        }

        private void Update()
        {
            bool shortcutPressed = Input.GetKeyDown(key)
                           && (shift && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) || !shift)
                           && (ctrl && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) || !ctrl);
            if (shortcutPressed)
            {
                if (!ui.activeSelf)
                {
                    ui.SetActive(true);
                    SceneManager.LoadSceneAsync("Emergence", LoadSceneMode.Additive);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseOverlay();
            }

        }

        private void CloseOverlay()
        {
            SceneManager.UnloadSceneAsync("Emergence");
            ui.SetActive(false);
        }


        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name.Equals("Emergence"))
            {
                UnityEngine.Debug.Log("Loaded");
                ui.SetActive(false);
            }
        }


        private static void StartServer()
        {
            try
            {
                Process.Start("run-server.bat");
                UnityEngine.Debug.Log("Running Emergence Server");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Server error: " + e.Message);
            }
        }
    }
}