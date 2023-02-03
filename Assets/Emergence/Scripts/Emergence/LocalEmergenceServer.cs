using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using static EmergenceSDK.Services;

namespace EmergenceSDK
{
    public class LocalEmergenceServer : SingletonComponent<LocalEmergenceServer>
    {
        // private EnvValues envValues = null;
        // private const int DEFAULT_PORT = 57000;
        //
        // private bool started;
        //
        // public string NodeURL { get; private set; }
        //
        // public bool IsStarted()
        // {
        //     return started;
        // }
        //
        // #region Finish
        //
        // public delegate void SuccessFinish();
        // public void Finish(SuccessFinish success, GenericError error)
        // {
        //     if (!CheckEnv()) { return; }
        //     StartCoroutine(CoroutineFinish(success, error));
        // }
        //
        // private IEnumerator CoroutineFinish(SuccessFinish success, GenericError error)
        // {
        //     Debug.Log("Finish request started");
        //     string url = envValues.APIBase + "finish";
        //
        //     using (UnityWebRequest request = UnityWebRequest.Get(url))
        //     {
        //         yield return request.SendWebRequest();
        //         PrintRequestResult("Finish request completed", request);
        //
        //         if (RequestError(request))
        //         {
        //             error?.Invoke(request.error, request.responseCode);
        //         }
        //         else
        //         {
        //             success?.Invoke();
        //         }
        //     }
        // }
        //
        // #endregion Finish

        private new void Awake()
        {
            // // NodeURL = Environment().DefaultNodeURL;
            // if (EmergenceSingleton.Instance.Configuration == null)
            // {
            //     Debug.LogError("Missing configuration. Check the emergence prefab and assign a configuration object.");
            //     throw new ArgumentNullException("Missing configuration. Check the emergence prefab and assign a configuration object.");
            // }
            // envValues = new EnvValues
            // {
            //     APIBase = EmergenceSingleton.Instance.Configuration.APIBase,
            //     CustomEmergenceServerLocation = EmergenceSingleton.Instance.Configuration.CustomEmergenceServerLocation,
            //     CustomEmergenceServerURL = EmergenceSingleton.Instance.Configuration.CustomEmergenceServerURL,
            //     PersonaURL = EmergenceSingleton.Instance.Configuration.PersonaURL,
            //     InventoryURL = EmergenceSingleton.Instance.Configuration.InventoryURL,
            //     AvatarURL = EmergenceSingleton.Instance.Configuration.AvatarURL,
            //     DefaultNodeURL = EmergenceSingleton.Instance.Configuration.Chain.DefaultNodeURL,
            // };

        }

        // private void StopEVMServerProcess()
        // {
        //     if (!CheckEnv()) { return; }
        //     try
        //     {
        //         // TODO avoid using a bat file
        //         Debug.Log("Stopping Emergence Server");
        //         System.Diagnostics.Process.Start("stop-server.bat");
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.Log("Server error: " + e.Message);
        //     }
        // }
        //
        // public bool CheckEnv()
        // {
        //     return envValues != null;
        // }
        //
        // public EnvValues Environment()
        // {
        //     if (!CheckEnv())
        //     {
        //         string err = "Missing env values. Ensure that a proper environment file is available in the project.";
        //         Debug.LogError(err);
        //         throw new ArgumentNullException(err);
        //     }
        //     else
        //     {
        //         return envValues;
        //     }
        // }



       
    }

}