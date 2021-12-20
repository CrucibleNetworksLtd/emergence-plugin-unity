using UnityEngine;
using UnityEditor;
using System.Diagnostics;

namespace Emergence
{
    public class EmergenceEditor : Editor
    {
        [MenuItem("Emergence/Run EVM Server")]
        static void RunEVMServer()
        {
            UnityEngine.Debug.Log("Run EVM Server");
            Process.Start("run-server.bat");
        }

        [MenuItem("Emergence/Stop EVM Server")]
        static void StopEVMServer()
        {
            UnityEngine.Debug.Log("Stop EVM Server");
        }

        [MenuItem("Emergence/Restart EVM Server")]
        static void RestartEVMServer()
        {
            UnityEngine.Debug.Log("Restart EVM Server");
            Process.Start("run-server.bat");
        }
    }
}