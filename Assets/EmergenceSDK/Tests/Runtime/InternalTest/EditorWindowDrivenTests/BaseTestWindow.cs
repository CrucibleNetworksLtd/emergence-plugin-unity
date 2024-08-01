#if UNITY_EDITOR

using EmergenceSDK.Runtime.Services;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK.Tests.Internal.EditorWindowDrivenTests
{
    public abstract class BaseTestWindow : EditorWindow
    {
        protected bool needsCleanUp;
        protected static bool IsLoggedIn() => EmergenceServiceProvider.GetService<IWalletService>().IsValidWallet;

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

#endif