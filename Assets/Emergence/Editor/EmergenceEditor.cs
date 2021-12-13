using UnityEngine;
using UnityEditor;

public class EmergenceEditor : Editor
{
    [MenuItem("Emergence/Run EVM Server")]
    static void RunEVMServer()
    {
        Debug.Log("Run EVM Server");
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
    }
}
