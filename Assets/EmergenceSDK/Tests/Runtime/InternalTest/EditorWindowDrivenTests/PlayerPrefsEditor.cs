#if UNITY_EDITOR
using EmergenceSDK.Runtime.Internal.Utils;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK.Tests.Internal.EditorWindowDrivenTests
{
    public class PlayerPrefsEditor : EditorWindow
    {
        [MenuItem("Window/Emergence Internal/PlayerPrefsEditor")]
        public static void ShowWindow()
        {
            GetWindow<PlayerPrefsEditor>("Player Prefs Editor");
        }

        private void OnGUI()
        {
            GUILayout.Label("PlayerPrefs Editor", EditorStyles.boldLabel);

            // Display PlayerPrefs keys and values
            int hasLoggedInOnceValue = PlayerPrefs.GetInt(StaticConfig.HasLoggedInOnceKey, 0);
            GUILayout.Label($"{StaticConfig.HasLoggedInOnceKey}: {hasLoggedInOnceValue}");

            // Add a button to reset the HasLoggedInOnceKey value to 0
            if (GUILayout.Button("Reset HasLoggedInOnceKey"))
            {
                PlayerPrefs.SetInt(StaticConfig.HasLoggedInOnceKey, 0);
                PlayerPrefs.Save();
            }
        }
    }
}
#endif