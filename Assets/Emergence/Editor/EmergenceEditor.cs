using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace EmergenceSDK
{
    public class EmergenceEditor : Editor
    {
        [MenuItem("Emergence/Run EVM Server")]
        static void RunEVMServer()
        {
            Debug.Log("Run EVM Server");
            Process.Start("run-server.bat");
        }

        [MenuItem("Emergence/Stop EVM Server")]
        static void StopEVMServer()
        {
            Debug.Log("Stop EVM Server");
        }

        [MenuItem("Emergence/Restart EVM Server")]
        static void RestartEVMServer()
        {
            Debug.Log("Restart EVM Server");
            Process.Start("run-server.bat");
        }
    }
}