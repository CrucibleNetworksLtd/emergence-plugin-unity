using System;
using EmergenceSDK.Types;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK.InternalTesting
{
    public abstract class BaseTestWindow : EditorWindow
    {
        protected bool needsCleanUp;
        protected static bool IsLoggedIn() => EmergenceSingleton.Instance.GetCachedAddress() != null;

        protected bool ReadyToTest(out string message)
        {
            message = "";
            if (Application.isPlaying && IsLoggedIn()) 
                return true;
            
            
            message = "Hit play & sign in to test Emergence SDK";
            return false;
        }

        void Update()
        {
            if (needsCleanUp && !EditorApplication.isPlaying)
            {
                needsCleanUp = false;
                CleanUp();
            }
        }
        
        protected virtual void CleanUp() { }
    }
}