using EmergenceSDK.Types;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK
{
    public abstract class BaseTestWindow : EditorWindow
    {
        protected static bool IsLoggedIn() => EmergenceSingleton.Instance.GetCachedAddress() != null;

        
        protected bool ReadyToTest(out string message)
        {
            message = "";
            if (Application.isPlaying && IsLoggedIn()) 
                return true;
            
            
            message = "Hit play & sign in to test Emergence SDK";
            CleanUp();
            return false;
        }

        protected virtual void CleanUp() { }
    }
}