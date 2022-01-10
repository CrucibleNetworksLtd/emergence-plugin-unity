using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace EmergenceSDK
{
    public partial class Services
    {
        #region EVM Server

        #region Start and Stop

        public void SetupAndStartEVMServer(string nodeURL, string gameId)
        {
            if (!CheckEnv()) { return; }

            this.nodeURL = envValues.defaultNodeURL;

            if (!String.IsNullOrEmpty(nodeURL.Trim()))
            {
                this.nodeURL = nodeURL;
            }

            this.gameId = gameId;

            StartEVMServer();
        }

        public bool StartEVMServer()
        {
            if (!CheckEnv()) { return false; }

            bool started = false;
            Process[] pname = Process.GetProcessesByName("EmergenceEVMLocalServer");

            if (pname.Length > 0)
            {
                Debug.Log("Process for EVM server found");
                started = true;
            }
            else
            {
                Debug.LogWarning("Process for EVM server not found, trying to launch");
                started = LaunchEVMServerProcess();
            }

            return started;
        }

        public void StopEVMServer()
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

        private bool LaunchEVMServerProcess()
        {
            if (!CheckEnv()) { return false; }
            bool started = false;
            try
            {
                // TODO send process id set a reference to the current process and use System.Diagnostics's Process.Id property:int nProcessID = Process.GetCurrentProcess().Id;
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "Server\\EmergenceEVMLocalServer.exe";
                // Triple doubled double-quotes are needed for the server to receive CMD params with single double quotes (sigh...)
                startInfo.Arguments = @"--walletconnect={""""""Name"""""":""""""Crucibletest"""""",""""""Description"""""":""""""UnrealEngine+WalletConnect"""""",""""""Icons"""""":""""""https://crucible.network/wp-content/uploads/2020/10/cropped-crucible_favicon-32x32.png"""""",""""""URL"""""":""""""https://crucible.network""""""}";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process serverProcess = new Process();
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

        private void StopEVMServerProcess()
        {
            if (!CheckEnv()) { return; }
            try
            {
                // TODO avoid using a bat file
                Debug.Log("Stopping Emergence Server");
                Process.Start("stop-server.bat");
            }
            catch (Exception e)
            {
                Debug.Log("Server error: " + e.Message);
            }
        }

        #endregion Start and Stop

        #region Is Connected

        public delegate void IsConnectedSuccess(bool connected);
        public void IsConnected(IsConnectedSuccess success, GenericError error)
        {
            if (!CheckEnv()) { return; }
            StartCoroutine(CoroutineIsConnected(success, error));
        }

        private IEnumerator CoroutineIsConnected(IsConnectedSuccess success, GenericError error)
        {
            Debug.Log("CoroutineIsConnected request started");

            string url = envValues.APIBase + "isConnected";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                PrintRequestResult("IsConnected", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    IsConnectedResponse response = SerializationHelper.Deserialize<IsConnectedResponse>(request.downloadHandler.text);
                    if (response.statusCode != 0)
                    {
                        error?.Invoke("Problem with IsConnected", response.statusCode);
                    }
                    else
                    {
                        success?.Invoke(response.message.isConnected);
                    }
                }
            }
        }

        #endregion Is Connected

        #region Reinitialize WalletConnect

        public delegate void ReinitializeWalletConnectSuccess(bool disconnected);
        public void ReinitializeWalletConnect(ReinitializeWalletConnectSuccess success, GenericError error)
        {
            if (!CheckEnv()) { return; }
            StartCoroutine(CoroutineReinitializeWalletConnect(success, error));
        }

        private IEnumerator CoroutineReinitializeWalletConnect(ReinitializeWalletConnectSuccess success, GenericError error)
        {
            Debug.Log("CoroutineReinitializeWalletConnect request started");

            string url = envValues.APIBase + "reinitializewalletconnect";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                PrintRequestResult("ReinitializeWalletConnect", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    ReinitializeWalletConnectResponse response = SerializationHelper.Deserialize<ReinitializeWalletConnectResponse>(request.downloadHandler.text);
                    if (response.statusCode != 0)
                    {
                        error?.Invoke("Problem with ReinitializeWalletConnect", response.statusCode);
                    }
                    else
                    {
                        success?.Invoke(response.message.disconnected);
                    }
                }
            }
        }

        #endregion Reinitialize WalletConnect

        #region QR Code

        public delegate void QRCodeSuccess(Texture2D qrCode);
        public void GetQRCode(QRCodeSuccess success, GenericError error)
        {
            if (!CheckEnv()) { return; }
            StartCoroutine(CoroutineGetQrCode(success, error));
        }

        private IEnumerator CoroutineGetQrCode(QRCodeSuccess success, GenericError error)
        {
            Debug.Log("GetQrCode request started");
            string url = envValues.APIBase + "qrcode";

            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();

                PrintRequestResult("GetQrCode", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    success?.Invoke((request.downloadHandler as DownloadHandlerTexture).texture);
                }
            }
        }

        #endregion QR Code

        #region Handshake

        public delegate void HandshakeSuccess(string walletAddress);
        public void Handshake(HandshakeSuccess success, GenericError error)
        {
            if (!CheckEnv()) { return; }
            StartCoroutine(CoroutineHandshake(success, error));
        }

        private IEnumerator CoroutineHandshake(HandshakeSuccess success, GenericError error)
        {
            Debug.Log("Handshake request started");
            string url = envValues.APIBase + "handshake" + "?nodeUrl=" + nodeURL;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                PrintRequestResult("Handshake", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    HandshakeResponse response = SerializationHelper.Deserialize<HandshakeResponse>(request.downloadHandler.text);
                    if (response.statusCode != 0)
                    {
                        error?.Invoke("Problem with handshake", response.statusCode);
                    }
                    else
                    {
                        success?.Invoke(response.message.address);
                    }
                }
            }
        }

        #endregion Handshake

        #region Get Balance

        public delegate void BalanceSuccess(string balance);
        public void GetBalance(BalanceSuccess success, GenericError error)
        {
            if (!CheckEnv()) { return; }

            if (skipWallet)
            {
                success?.Invoke("No wallet");
                return;
            }

            if (disconnectInProgress)
            {
                return;
            }

            StartCoroutine(CoroutineGetBalance(success, error));
        }

        private IEnumerator CoroutineGetBalance(BalanceSuccess success, GenericError error)
        {
            Debug.Log("Get Balance request started");
            string url = envValues.APIBase + "getbalance";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                PrintRequestResult("Get Balance", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    GetBalanceResponse response = SerializationHelper.Deserialize<GetBalanceResponse>(request.downloadHandler.text);

                    if (response.statusCode != 0)
                    {
                        error?.Invoke("Problem with GetBalance", response.statusCode);
                    }
                    else
                    {
                        success?.Invoke(response.message.balance);
                    }
                }
            }
        }

        #endregion Get Balance

        #region Get Access Token

        public delegate void AccessTokenSuccess(string accessToken);
        public void GetAccessToken(AccessTokenSuccess success, GenericError error)
        {
            if (!CheckEnv()) { return; }
            StartCoroutine(CoroutineGetAccessToken(success, error));
        }

        private IEnumerator CoroutineGetAccessToken(AccessTokenSuccess success, GenericError error)
        {
            Debug.Log("GetAccessToken request started");
            string url = envValues.APIBase + "get-access-token";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                PrintRequestResult("GetAccessToken", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    AccessTokenResponse response = SerializationHelper.Deserialize<AccessTokenResponse>(request.downloadHandler.text);

                    if (response.statusCode != 0)
                    {
                        error?.Invoke("Problem with GetAccessToken", response.statusCode);
                    }
                    else
                    {
                        currentAccessToken = SerializationHelper.Serialize(response.message.accessToken, false);
                        ProcessExpiration(response.message.accessToken.message);
                        success?.Invoke(currentAccessToken);
                    }
                }
            }
        }

        #endregion Get Access Token

        #region Disconnect Wallet

        private bool disconnectInProgress = false;
        public delegate void DisconnectSuccess();
        public void Disconnect(DisconnectSuccess success, GenericError error)
        {
            if (!CheckEnv()) { return; }

            if (skipWallet)
            {
                success?.Invoke();
                return;
            }

            disconnectInProgress = true;
            StartCoroutine(CoroutineDisconnect(success, error));
        }

        private IEnumerator CoroutineDisconnect(DisconnectSuccess success, GenericError error)
        {
            Debug.Log("Disconnect request started");
            string url = envValues.APIBase + "killSession";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                PrintRequestResult("Disconnect request completed", request);

                if (RequestError(request))
                {
                    disconnectInProgress = false;
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    disconnectInProgress = false;
                    success?.Invoke();
                }
            }
        }

        #endregion Disconnect Wallet

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

        #region Contracts

        #region Load Contract

        public delegate void SuccessLoadContract();
        public void LoadContract(string contractAddress, string ABI, SuccessLoadContract success, GenericError error)
        {
            if (!CheckEnv()) { return; }
            StartCoroutine(CoroutineLoadContract(contractAddress, ABI, success, error));
        }

        public IEnumerator CoroutineLoadContract(string contractAddress, string ABI, SuccessLoadContract success, GenericError error)
        {
            Debug.Log("LoadContract request started");

            Contract data = new Contract()
            {
                contractAddress = contractAddress,
                ABI = ABI
            };

            string dataString = SerializationHelper.Serialize(data, false);
            string url = envValues.APIBase + "loadContract";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                PrintRequestResult("Load Contract", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    LoadContractResponse response = SerializationHelper.Deserialize<LoadContractResponse>(request.downloadHandler.text);
                    if (response.statusCode != 0)
                    {
                        error?.Invoke("Problem loading contract", response.statusCode);
                    }
                    else
                    {
                        success?.Invoke();
                    }
                }
            }
        }

        #endregion Load Contract

        #region Read Contract

        public delegate void SuccessReadContract<T>(T response);
        public void ReadContract<T, U>(string contractAddress, string methodName, U body, SuccessReadContract<T> success, GenericError error)
        {
            if (!CheckEnv()) { return; }
            StartCoroutine(CoroutineReadContract<T, U>(contractAddress, methodName, body, success, error));
        }

        public IEnumerator CoroutineReadContract<T, U>(string contractAddress, string methodName, U body, SuccessReadContract<T> success, GenericError error)
        {
            Debug.Log("ReadContract request started [" + contractAddress + "] / " + methodName);

            string url = envValues.APIBase + "readMethod?contractAddress=" + contractAddress + "&methodName=" + methodName;

            string dataString = SerializationHelper.Serialize(body, false);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                PrintRequestResult("Read Contract", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    T response = SerializationHelper.Deserialize<T>(request.downloadHandler.text);
                    success?.Invoke(response);
                }
            }
        }

        #endregion Read Contract

        #region Write Contract

        public delegate void SuccessWriteContract<T>(T response);
        public void WriteContract<T, U>(string contractAddress, string methodName, U body, SuccessWriteContract<T> success, GenericError error)
        {
            if (!CheckEnv()) { return; }
            StartCoroutine(CoroutineWriteContract<T, U>(contractAddress, methodName, body, success, error));
        }

        public IEnumerator CoroutineWriteContract<T, U>(string contractAddress, string methodName, U body, SuccessWriteContract<T> success, GenericError error)
        {
            Debug.Log("WriteContract request started [" + contractAddress + "] / " + methodName);

            string url = envValues.APIBase + "writeMethod?contractAddress=" + contractAddress + "&methodName=" + methodName;

            string dataString = SerializationHelper.Serialize(body, false);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                PrintRequestResult("Write Contract", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    T response = SerializationHelper.Deserialize<T>(request.downloadHandler.text);
                    success?.Invoke(response);
                }
            }
        }

        #endregion Write Contract

        #endregion Contracts

        #endregion EVM Server
    }
}
