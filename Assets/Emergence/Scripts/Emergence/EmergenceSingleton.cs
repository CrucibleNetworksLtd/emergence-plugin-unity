using EmergenceSDK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EmergenceSingleton : SingletonComponent<EmergenceSingleton>
{
    private GameObject ui;
    private string accessToken;
    private string address;


    [Header("Keyboard shortcut to open Emergence")]
    [SerializeField]
    private KeyCode key = KeyCode.Tab;

    [SerializeField]
    private bool shift = true;

    [SerializeField]
    private bool ctrl = false;

    [Serializable]
    public class EmergenceUIStateChanged : UnityEvent<bool> { }

    [Serializable]
    public class EmergenceCachedPersonaUpdated : UnityEvent { }
    [Serializable]
    public class EmergenceUIOpened : UnityEvent { }

    [Serializable]
    public class EmergenceUIClosed : UnityEvent { }

    [Header("Events")]
    public EmergenceUIOpened OnEmergenceUIOpened;
    public EmergenceUIClosed OnEmergenceUIClosed;
    public EmergenceCachedPersonaUpdated CachedPersonaUpdated;

    // Not showing this event in the Inspector because the actual visibility 
    // parameter value would be overwritten by the value set in the inspector
    [HideInInspector]
    public EmergenceUIStateChanged OnEmergenceUIVisibilityChanged;
    public EmergencePersona CurrentCachedPersona { get; set; }

    /// <summary>
    /// Shortcut to .Instance. Use .Instance instead.
    /// </summary>
    /// <returns></returns>
    [Obsolete("Use .Instance instead.")]
    public static EmergenceSingleton GetEmergenceManager()
    {
        return Instance;
    }
    public void OpenEmergenceUI()
    {
        Emergence.Instance.OpenOverlay();
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

    public bool HasCachedAddress()
    {
        return !string.IsNullOrEmpty(address);
    }

    private void Update()
    {
        bool shortcutPressed = Input.GetKeyDown(key)
                       && (shift && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) || !shift)
                       && (ctrl && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) || !ctrl);

        if (shortcutPressed)
        {
            OpenEmergenceUI();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ScreenManager.Instance != null)
            {
                if (ScreenManager.Instance.IsVisible)
                {
                    Emergence.Instance.CloseOverlay();
                }
            }
        }
    }


}
