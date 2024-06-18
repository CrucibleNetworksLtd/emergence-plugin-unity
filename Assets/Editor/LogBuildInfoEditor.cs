using UnityEditor;
using UnityEngine;
using EmergenceSDK.Internal.Utils;

namespace EmergenceSDK.Editor
{
    /// <summary>
    /// Class for enabling Emergence Log Build info editor tool.
    /// </summary>
    internal static class LogBuildInfoEditor
    {
        /// <summary>
        /// Editor Ui button that logs the Build info for the emergence SDK to unity.
        /// </summary>
        [MenuItem("Tools/Emergence/Log build Info")]
        private static void LogBuildInfo()
        {
            Debug.Log(BuildInfoGenerator.GetBuildInfo());
        }
    }
}
