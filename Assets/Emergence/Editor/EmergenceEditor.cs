using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.IO;

using UnityEngine;
using System.IO.Compression;

namespace EmergenceSDK
{
    public class EmergenceEditor : Editor
    {
        private const string basePath = "Emergence/";

        [MenuItem(basePath + "Run EVM Server", priority = 1)]
        private static void RunEVMServer()
        {
            Debug.Log("Run EVM Server");
            Process.Start("run-server.bat");
        }

        [MenuItem(basePath + "Stop EVM Server", priority = 2)]
        private static void StopEVMServer()
        {
            Debug.Log("Stop EVM Server");
        }

        [MenuItem(basePath + "Restart EVM Server", priority = 3)]
        private static void RestartEVMServer()
        {
            Debug.Log("Restart EVM Server");
            Process.Start("run-server.bat");
        }

        [MenuItem(basePath + "Unzip local EVM server", priority = 20)]
        private static void UnzipLocalServer()
        {
            string compressedFile = Path.Combine(Application.dataPath, "Emergence/Server Installation/Server.zip");
            string decompressionPath = Directory.GetParent(Application.dataPath).FullName;
            ZipFile.ExtractToDirectory(compressedFile, decompressionPath);
        }
    }
}