using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug; // Stops the clash with System.

namespace EmergenceSDK.Internal.Utils
{
    /// <summary>
    /// Class for running some external Shell commands to pull Git info for the current repo. Used in displaying build info in the Debug Overlay.
    /// </summary>
    public static class BuildInfoGenerator
    {
        private static string gitInfo = "";

        /// <summary>
        /// Runs the appropriate shell script to generate Git information.
        /// </summary>
        private static bool RunGitInfoScript()
        {
            string scriptPath = Application.dataPath + "/../";
            
            string shellFileName = "";
#if UNITY_EDITOR_WIN
            shellFileName = "generate_git_info.bat";
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            shellFileName = "generate_git_info.sh";
#else
            Debug.LogError("Unsupported OS, Unable to Parse Git Info");
#endif
            string scriptFullPath = Path.Combine(scriptPath, shellFileName);

            if (!File.Exists(scriptFullPath))
            {
                Debug.LogError("Script file not found: " + scriptFullPath);
                return false;
            }

            ProcessStartInfo processStartInfo = GetProcessStartInfo(scriptFullPath);
            using (Process process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                process.WaitForExit();
            }
            return true;
        }

        /// <summary>
        /// Logs the Git information, Unity version, and current UTC time to the Unity console.
        /// </summary>
        public static string GetBuildInfo()
        {
            string output = "";

            string gitInfoPath = "Assets/Emergence/Resources/git_info.txt";
            
            string utcNow = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            string unityVersion = Application.unityVersion;
            
            // If we haven't cached git info we'll attempt to read it and then check if that's been successful.
            if (string.IsNullOrEmpty(gitInfo) && !File.Exists(gitInfoPath) && RunGitInfoScript())
            {
                gitInfo = File.ReadAllText(gitInfoPath);
            }
            else if (string.IsNullOrEmpty(gitInfo) && File.Exists(gitInfoPath))
            {
                gitInfo = File.ReadAllText(gitInfoPath);
            }
            else if (!File.Exists(gitInfoPath) && !RunGitInfoScript())
            {
                Debug.LogError("Failed To Run Shell commands for parsing GitInfo");
            }
            else if(!File.Exists(gitInfoPath)) // If we can't parse the info we exit out cleanly
            {
                Debug.LogError("Git info file not found: " + gitInfoPath);
                gitInfo = "Unable to Parse Git Info";
            }

            output = string.Format("Build Info | UTC Date/Time: {0} | Unity{1}\n{2}", utcNow, unityVersion, gitInfo);
            return output;
        }

        /// <summary>
        /// Gets the process start information of the shell command that stores the Git Branch and Commit.
        /// </summary>
        private static ProcessStartInfo GetProcessStartInfo(string scriptFullPath)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
#if UNITY_EDITOR_WIN
            processStartInfo.FileName = "cmd.exe";
            processStartInfo.Arguments = "/c " + scriptFullPath;
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            processStartInfo.FileName = "/bin/bash";
            processStartInfo.Arguments = scriptFullPath;
#endif
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;

            return processStartInfo;
        }
    }
}