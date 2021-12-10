using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
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
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
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
            if (!ui.activeSelf)
            {
                ui.SetActive(true);
                SceneManager.LoadSceneAsync("Emergence", LoadSceneMode.Additive);
            }
        }
    }
    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.name.Equals("Emergence"))
        {
            Debug.Log("Loaded");
            ui.SetActive(false);
        }
    }
}
