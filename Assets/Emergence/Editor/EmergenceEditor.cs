#if UNITY_EDITOR
using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.IO;

using UnityEngine;
using System.IO.Compression;
using System;

namespace EmergenceSDK
{
    public class EmergenceEditor : Editor
    {
        private const string basePath = "Emergence/";

        // TODO Refactor current EVM server runtime methods to share parameters with this editor script
        /*
        [MenuItem(basePath + "Run EVM Server", priority = 1)]
        private static void RunEVMServer()
        {

        }

        [MenuItem(basePath + "Stop EVM Server", priority = 2)]
        private static void StopEVMServer()
        {

        }

        [MenuItem(basePath + "Restart EVM Server", priority = 3)]
        private static void RestartEVMServer()
        {

        }
        */

        [MenuItem(basePath + "Unzip local EVM server", priority = 20)]
        private static void UnzipLocalServer()
        {
            try
            {
                string compressedFile = Path.Combine(Application.dataPath, "Emergence/Server Installation/Server.zip");
                string decompressionPath = Directory.GetParent(Application.dataPath).FullName;
                ZipFile.ExtractToDirectory(compressedFile, decompressionPath);
                Debug.Log("Server files extracted");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}
#endif