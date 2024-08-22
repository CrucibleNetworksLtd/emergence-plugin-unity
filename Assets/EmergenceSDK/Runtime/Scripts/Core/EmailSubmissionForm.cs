#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Internal.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Runtime
{
    [InitializeOnLoad]
    public static class WindowOpener
    {
        static WindowOpener()
        {
            EditorApplication.delayCall += ShowWindow;
        }

        static void ShowWindow()
        {
            EmailSubmissionForm.Init();
        }
    }

    /// <summary>
    /// UI for email submission.
    /// <remarks>We will use your email to send you updates about Emergence.</remarks>
    /// </summary>
    internal class EmailSubmissionForm : EditorWindow
    {
        private string email;
        private const string baseUrl = "https://api.emailjs.com/api/v1.0/email/send";
        private Vector2 scrollPosition;
        private const string EmailSubmissionFormHasOpened = "EmailSubmissionForm_hasOpened";

        public static void Init()
        {
            if (!EditorPrefs.GetBool(EmailSubmissionFormHasOpened, false))
            {
                ShowWindow();
                // Automatically send email when the form is initialized for the first time
                SendVersionEmail().Forget();
            }
        }

        [MenuItem("Window/Emergence SDK/Email Submission")]
        public static void ShowWindow()
        {
            EditorPrefs.SetBool(EmailSubmissionFormHasOpened, true);
            GetWindow<EmailSubmissionForm>("Email Submission Form");
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Register for developer updates (optional):");

            email = EditorGUILayout.TextField("Email:", email);

            if (GUILayout.Button("Submit"))
            {
                SendEmail().Forget(); // UniTask coroutine should be run with Forget()
            }
            GUILayout.Space(20);
            if (GUILayout.Button("Privacy Policy & Terms of Service"))
            {
                Application.OpenURL("https://www.emergence.site/assets/documents/Emergence_Privacy%20Policy_FINAL%20(25%20Nov%2021).pdf");
            }
        }

        private static string GetPackageVersion()
        {
            // Assuming the package is in the "Packages/com.emergence.sdk" folder
            string packagePath = "Packages/com.emergence.sdk/package.json";
            if (File.Exists(packagePath))
            {
                string json = File.ReadAllText(packagePath);
                var packageInfo = JsonUtility.FromJson<PackageInfo>(json);
                return packageInfo.version;
            }
            return "Unknown";
        }

        [Serializable]
        private class PackageInfo
        {
            public string version;
        }

        [Serializable]
        private class EmailSubmissionFormParams
        {
            public string from_email;
            public string from_engine = $"Unity {Application.unityVersion}";
            public string from_os = SystemInfo.operatingSystem;
            public string from_emergenceversion = "0.1.3";
            public string from_packageversion;
            public string from_unityversion;
            public string from_emergenceevmtype = "EVMOnline"; // "EVMOnline" or "LocalEVM"
        }

        [Serializable]
        private class EmailSubmissionFormPayload
        {
            public string service_id;
            public string template_id;
            public string user_id;
            public EmailSubmissionFormParams template_params;
        }

        private async UniTaskVoid SendEmail()
        {
            var data = new EmailSubmissionFormPayload
            {
                service_id = "service_txbxvyw",
                template_id = "template_775t29f",
                user_id = "XZJBhUf8kkPvSWNuG",
                template_params = new EmailSubmissionFormParams
                {
                    from_email = email,
                    from_packageversion = GetPackageVersion(),
                    from_unityversion = Application.unityVersion
                },
            };

            var payloadJson = JsonUtility.ToJson(data);

            var request = UnityWebRequest.Put(baseUrl, payloadJson);
            request.method = "POST";
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payloadJson));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var asyncOp = request.SendWebRequest();

            while (!asyncOp.isDone)
            {
                await UniTask.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                EmergenceLogger.LogInfo("Email sent successfully!");
            }
            else
            {
                EmergenceLogger.LogError(request.error);
            }
        }

        private static async UniTaskVoid SendVersionEmail()
        {
            var data = new EmailSubmissionFormPayload
            {
                service_id = "service_txbxvyw",
                template_id = "template_775t29f",
                user_id = "XZJBhUf8kkPvSWNuG",
                template_params = new EmailSubmissionFormParams
                {
                    from_email = "auto@emergence.sdk",
                    from_packageversion = GetPackageVersion(),
                    from_unityversion = Application.unityVersion
                },
            };

            var payloadJson = JsonUtility.ToJson(data);

            var request = UnityWebRequest.Put(baseUrl, payloadJson);
            request.method = "POST";
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payloadJson));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var asyncOp = request.SendWebRequest();

            while (!asyncOp.isDone)
            {
                await UniTask.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                EmergenceLogger.LogInfo("Version email sent successfully!");
            }
            else
            {
                EmergenceLogger.LogError(request.error);
            }
        }
    }
}
#endif
