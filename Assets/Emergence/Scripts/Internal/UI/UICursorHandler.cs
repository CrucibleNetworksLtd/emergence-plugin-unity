using UnityEngine;

namespace EmergenceSDK.Internal.UI
{
    public class UICursorHandler
    {
        private CursorLockMode previousCursorLockMode = CursorLockMode.None;
        private bool previousCursorVisible = false;

        public void SaveCursor()
        {
            previousCursorLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
        }

        public void UpdateCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void RestoreCursor()
        {
            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
        }
    }
}