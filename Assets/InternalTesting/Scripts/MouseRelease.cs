#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK.InternalTesting
{
    public class MouseRelease : EditorWindow
    {
        [MenuItem("Window/Emergence Internal/MouseRelease")]
        public static void ShowWindow()
        {
            GetWindow<MouseRelease>("Mouse Release");
        }

        private void Update()
        {
            // Check if the game is running
            if (EditorApplication.isPlaying)
            {
                // Check if the user presses Shift + 1
                if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Alpha1))
                {
                    // Release the mouse cursor
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        private void OnGUI()
        {
            // Any GUI code here
            GUILayout.Label("Press Shift + 1 to release mouse.");
        }
    }
}
#endif