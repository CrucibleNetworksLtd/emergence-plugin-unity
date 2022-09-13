using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;
using static EmergenceSDK.Services;

namespace EmergenceSDK
{
    public class LocalEmergenceServer : SingletonComponent<LocalEmergenceServer>
    {
        private EnvValues envValues = null;
        private const int DEFAULT_PORT = 57000;



        #region Start and Stop
        public void LaunchLocalServerProcess(bool hidden = true)
        {
            //nodeURL, gameId
            if (!CheckEnv()) { return; }

            bool started = false;
            System.Diagnostics.Process[] pname = System.Diagnostics.Process.GetProcessesByName("EmergenceEVMLocalServer");

            if (pname.Length > 0)
            {
                Debug.LogWarning("Existing process for EVM server found, trying connecting with the default port. This might fail if the EVM server was started with another port.");

                string url = BuildLocalServerURL(DEFAULT_PORT);
                envValues.APIBase = url + "api/";

                started = true;
            }
            else
            {
                Debug.LogWarning("Process for EVM server not found, trying to launch");
                started = LaunchEVMServerProcess(hidden);
            }
        }


        public void KillLocalServerProcess()
        {
            if (!CheckEnv()) { return; }
            Debug.Log("Sending Finish command to EVM Server");
            Finish(() =>
            {
                Debug.Log("EVM Server process Finished successfully");
            },
            (error, code) =>
            {
                Debug.LogWarning("EVM Server process did not respond to Finish, trying to stop it forcefully");
                StopEVMServerProcess();
            });
        }
        #endregion

        #region Finish

        public delegate void SuccessFinish();
        public void Finish(SuccessFinish success, GenericError error)
        {
            if (!CheckEnv()) { return; }
            StartCoroutine(CoroutineFinish(success, error));
        }

        private IEnumerator CoroutineFinish(SuccessFinish success, GenericError error)
        {
            Debug.Log("Finish request started");
            string url = envValues.APIBase + "finish";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                PrintRequestResult("Finish request completed", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    success?.Invoke();
                }
            }
        }

        #endregion Finish

        private void StopEVMServerProcess()
        {
            if (!CheckEnv()) { return; }
            try
            {
                // TODO avoid using a bat file
                Debug.Log("Stopping Emergence Server");
                System.Diagnostics.Process.Start("stop-server.bat");
            }
            catch (Exception e)
            {
                Debug.Log("Server error: " + e.Message);
            }
        }

        private void Awake()
        {
            TextAsset envFile;

            try
            {
                envFile = Resources.Load<TextAsset>("emergence.env.dev");

                if (envFile == null)
                {
                    envFile = Resources.Load<TextAsset>("emergence.env.staging");
                }

                if (envFile == null)
                {
                    envFile = Resources.Load<TextAsset>("emergence.env");
                }

                if (envFile == null)
                {
                    Debug.LogError("emergence.env file missing from Resources folder");
                    return;
                }

                envValues = SerializationHelper.Deserialize<EnvValues>(envFile.text);

                if (envValues == null)
                {
                    Debug.LogError("emergence.env file is corrupted or missing");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private bool CheckEnv()
        {
            return envValues != null;
        }

        private string BuildLocalServerURL(int forcedPort = 0)
        {
            // Look for free ports and update APIBase with it
            string serverURL = "http://localhost";

            if (envValues.APIBase != null && envValues.APIBase.Trim().Length > 0)
            {
                serverURL = envValues.APIBase;
            }

            UriBuilder uriBuilder = new UriBuilder(serverURL);
            if (forcedPort > 0)
            {
                uriBuilder.Port = forcedPort;
            }
            else
            {
                uriBuilder.Port = GetNextFreePort();
            }

            return uriBuilder.ToString();
        }

        private int GetNextFreePort()
        {
            int lookingPort = DEFAULT_PORT;

            while (lookingPort < 65535)
            {
                int port = CheckFreePort(lookingPort);

                if (port < DEFAULT_PORT)
                {
                    Debug.Log($"Port {lookingPort} is taken, checking next port...");
                    lookingPort++;
                    continue;
                }

                Debug.Log($"Found free port {port}");
                return port;
            }

            Debug.LogError("Couldn't find a free port!");
            return -1;
        }

        private int CheckFreePort(int port)
        {
            int foundPort = -1;
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                foundPort = ((IPEndPoint)listener.LocalEndpoint).Port;
                listener.Stop();
            }
            catch (Exception e)
            {
                // If the code gets here we know it's basically that the port it's being used
                string message = e.Message;
            }

            return foundPort;
        }

        private bool LaunchEVMServerProcess(bool hidden)
        {
            if (!CheckEnv()) { return false; }
            bool started = false;
            try
            {
                string url = BuildLocalServerURL();
                envValues.APIBase = url + "api/";

                string urls = " --urls=\"" + url + "\"";
                string walletConnect = @" --walletconnect={""""""Name"""""":""""""Crucibletest"""""",""""""Description"""""":""""""UnityEngine+WalletConnect"""""",""""""Icons"""""":""""""https://crucible.network/wp-content/uploads/2020/10/cropped-crucible_favicon-32x32.png"""""",""""""URL"""""":""""""https://crucible.network""""""}";
                string processId = " --processid=" + System.Diagnostics.Process.GetCurrentProcess().Id;

                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = "Server\\EmergenceEVMLocalServer.exe";
                // Triple doubled double-quotes are needed for the server to receive CMD params with single double quotes (sigh...)
                startInfo.Arguments = urls + walletConnect + processId;

                if (hidden)
                {
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }

                System.Diagnostics.Process serverProcess = new System.Diagnostics.Process();
                serverProcess.StartInfo = startInfo;
                serverProcess.EnableRaisingEvents = true;
                serverProcess.Start();

                Debug.Log("Running Emergence Server");
                started = true;
            }
            catch (Exception e)
            {
                Debug.Log("Server error: " + e.Message);
            }

            return started;
        }
    }

}