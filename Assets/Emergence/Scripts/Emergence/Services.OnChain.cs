using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace EmergenceSDK
{
    public partial class Services
    {
        #region EVM Server

        private const int DEFAULT_PORT = 57000;

        #region Start and Stop



#if DEBUG
        [Obsolete("Used updated API", true)]
#endif        
        public void SetupAndStartEVMServer(string nodeURL, string gameId, bool hidden = true)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }

            // this.gameId = gameId;

            StartEVMServer(hidden);
        }

#if DEBUG
        [Obsolete("Used updated API", true)]
#endif        
        public bool StartEVMServer(bool hidden = true)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return false; }

            bool started = false;
            Process[] pname = Process.GetProcessesByName("EmergenceEVMLocalServer");

            if (pname.Length > 0)
            {
                Debug.LogWarning("Existing process for EVM server found, trying connecting with the default port. This might fail if the EVM server was started with another port.");

                string url = BuildLocalServerURL(DEFAULT_PORT);
                LocalEmergenceServer.Instance.Environment().APIBase = url + "api/";

                started = true;
            }
            else
            {
                Debug.LogWarning("Process for EVM server not found, trying to launch");
                started = LaunchEVMServerProcess(hidden);
            }

            return started;
        }

#if DEBUG
        [Obsolete("Used updated API", true)]
#endif        
        public void StopEVMServer()
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
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

        private string BuildLocalServerURL(int forcedPort = 0)
        {
            // Look for free ports and update APIBase with it
            string serverURL = "http://localhost";

            if (LocalEmergenceServer.Instance.Environment().APIBase != null && LocalEmergenceServer.Instance.Environment().APIBase.Trim().Length > 0)
            {
                serverURL = LocalEmergenceServer.Instance.Environment().APIBase;
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

        private bool LaunchEVMServerProcess(bool hidden)
        {

            if (!LocalEmergenceServer.Instance.CheckEnv()) { return false; }
            bool started = false;
            try
            {
                string url = BuildLocalServerURL();
                LocalEmergenceServer.Instance.Environment().APIBase = url + "api/";

                string urls = " --urls=\"" + url + "\"";
                string walletConnect = @" --walletconnect={""""""Name"""""":""""""Crucibletest"""""",""""""Description"""""":""""""UnityEngine+WalletConnect"""""",""""""Icons"""""":""""""https://crucible.network/wp-content/uploads/2020/10/cropped-crucible_favicon-32x32.png"""""",""""""URL"""""":""""""https://crucible.network""""""}";
                string processId = " --processid=" + Process.GetCurrentProcess().Id;

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "Server\\EmergenceEVMLocalServer.exe";
                // Triple doubled double-quotes are needed for the server to receive CMD params with single double quotes (sigh...)
                startInfo.Arguments = urls + walletConnect + processId;

                if (hidden)
                {
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }

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
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
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
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineIsConnected(success, error));
        }

        private IEnumerator CoroutineIsConnected(IsConnectedSuccess success, GenericError error)
        {
            Debug.Log("CoroutineIsConnected request started");

            string url = LocalEmergenceServer.Instance.Environment().APIBase + "isConnected";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                PrintRequestResult("IsConnected", request);
                if (ProcessRequest<IsConnectedResponse>(request, error, out var response))
                {
                    success?.Invoke(response.isConnected);
                }
            }
        }

        #endregion Is Connected

        #region Reinitialize WalletConnect

        public delegate void ReinitializeWalletConnectSuccess(bool disconnected);
        public void ReinitializeWalletConnect(ReinitializeWalletConnectSuccess success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineReinitializeWalletConnect(success, error));
        }

        private IEnumerator CoroutineReinitializeWalletConnect(ReinitializeWalletConnectSuccess success, GenericError error)
        {
            Debug.Log("CoroutineReinitializeWalletConnect request started");

            string url = LocalEmergenceServer.Instance.Environment().APIBase + "reinitializewalletconnect";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                PrintRequestResult("ReinitializeWalletConnect", request);
                if (ProcessRequest<ReinitializeWalletConnectResponse>(request, error, out var response))
                {
                    success?.Invoke(response.disconnected);
                }
            }
        }

        #endregion Reinitialize WalletConnect
        #region Request to Sign WalletConnect
        public delegate void RequestToSignSuccess(string signedMessage);
        public void RequestToSignWalletConnect(string messageToSign, RequestToSignSuccess success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineRequestToSignWalletConnect(messageToSign, success, error));
        }

        private IEnumerator CoroutineRequestToSignWalletConnect(string messageToSign, RequestToSignSuccess success, GenericError error)
        {
            Debug.Log("CoroutineRequestToSignWalletConnect request started");
            var content = "{\"message\": \"" + messageToSign + "\"}";

            string url = LocalEmergenceServer.Instance.Environment().APIBase + "request-to-sign";

            using (UnityWebRequest request = UnityWebRequest.Post(url, content))
            {
                request.method = "POST";
                request.uploadHandler.contentType = "application/json";
                request.SetRequestHeader("accept", "application/json");

                yield return request.SendWebRequest();
                PrintRequestResult("ReinitializeWalletConnect", request);
                if (ProcessRequest<RequestToSignResponse>(request, error, out var response))
                {
                    success?.Invoke(response.SignedMessage);
                }
            }
        }
        #endregion

        #region QR Code

        public delegate void QRCodeSuccess(Texture2D qrCode);
        public void GetQRCode(QRCodeSuccess success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineGetQrCode(success, error));
        }

        private IEnumerator CoroutineGetQrCode(QRCodeSuccess success, GenericError error)
        {
            Debug.Log("GetQrCode request started");
            string url = LocalEmergenceServer.Instance.Environment().APIBase + "qrcode";

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

        #region Handshake Address Properties

        private string address = string.Empty;

        public bool HasAddress
        {
            get
            {
                return address != null && address.Trim() != string.Empty;
            }
        }

        public string Address
        {
            get
            {
                return address;
            }
            private set
            {
                address = value;
            }
        }

        #endregion Handshake Address Properties

        public delegate void HandshakeSuccess(string walletAddress);
        public void Handshake(HandshakeSuccess success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineHandshake(success, error));
        }

        private IEnumerator CoroutineHandshake(HandshakeSuccess success, GenericError error)
        {
            Debug.Log("Handshake request started");
            string url = LocalEmergenceServer.Instance.Environment().APIBase + "handshake" + "?nodeUrl=" + LocalEmergenceServer.Instance.NodeURL;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                PrintRequestResult("Handshake", request);
                if (ProcessRequest<HandshakeResponse>(request, error, out var response))
                {
                    Address = response.address;
                    EmergenceSingleton.Instance.SetCachedAddress(response.address);
                    success?.Invoke(Address);
                }
            }
        }

        #endregion Handshake

        #region Create Wallet

        public delegate void CreateWalletSuccess();

        public void CreateWallet(string path, string password, CreateWalletSuccess success, GenericError error)
        {
            StartCoroutine(CoroutineCreateWallet(path, password, success, error));
        }

        private IEnumerator CoroutineCreateWallet(string path, string password, CreateWalletSuccess success, GenericError error)
        {
            Debug.Log("CreateWallet request started");

            string url = LocalEmergenceServer.Instance.Environment().APIBase + "createWallet" + "?path=" + path + "&password=" + password;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";

                yield return request.SendWebRequest();
                PrintRequestResult("Create Wallet", request);
                if (ProcessRequest<string>(request, error, out var response))
                {
                    success?.Invoke();
                }
            }
        }

        #endregion Create Wallet

        #region Create Key Store

        public delegate void CreateKeyStoreSuccess();

        public void CreateKeyStore(string privateKey, string password, string publicKey, string path, CreateKeyStoreSuccess success, GenericError error)
        {
            StartCoroutine(CoroutineKeyStore(privateKey, password, publicKey, path, success, error));
        }

        private IEnumerator CoroutineKeyStore(string privateKey, string password, string publicKey, string path, CreateKeyStoreSuccess success, GenericError error)
        {

            Debug.Log("Create KeyStore request started");

            string url = LocalEmergenceServer.Instance.Environment().APIBase + "createKeyStore" + "?privateKey=" + privateKey + "&password=" + password + "&publicKey=" + publicKey + "&path=" + path;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";

                yield return request.SendWebRequest();
                PrintRequestResult("Key Store", request);
                if (ProcessRequest<string>(request, error, out var response))
                {
                    success?.Invoke();
                }
            }
        }

        #endregion Create Key Store

        #region Load Account

        public delegate void LoadAccountSuccess();

        public void LoadAccount(string name, string password, string path, string nodeURL, string chainId, LoadAccountSuccess success, GenericError error)
        {
            StartCoroutine(CoroutineLoadAccount(name, password, path, nodeURL, chainId, success, error));
        }

        private IEnumerator CoroutineLoadAccount(string name, string password, string path, string nodeURL, string chainId, LoadAccountSuccess success, GenericError error)
        {
            Debug.Log("Load Account request started");

            Account data = new Account()
            {
                name = name,
                password = password,
                path = path,
                nodeURL = nodeURL,
                chainId = chainId
            };

            string dataString = SerializationHelper.Serialize(data, false);
            string url = LocalEmergenceServer.Instance.Environment().APIBase + "loadAccount";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                PrintRequestResult("Load Account", request);
                if (ProcessRequest<LoadAccountResponse>(request, error, out var response))
                {
                    success?.Invoke();
                }
            }
        }

        #endregion Load Account

        #region Get Balance

        public delegate void BalanceSuccess(string balance);
        public void GetBalance(BalanceSuccess success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }

            if (skipWallet)
            {
                success?.Invoke("No wallet");
                return;
            }

            if (disconnectInProgress)
            {
                return;
            }

            StartCoroutine(CoroutineGetBalance(Address, success, error));
        }

        private IEnumerator CoroutineGetBalance(string address, BalanceSuccess success, GenericError error)
        {
            Debug.Log("Get Balance request started");

            string url = LocalEmergenceServer.Instance.Environment().APIBase + "getbalance" + "?nodeUrl=" + LocalEmergenceServer.Instance.NodeURL + "&address=" + address;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                PrintRequestResult("Get Balance", request);
                if (ProcessRequest<GetBalanceResponse>(request, error, out var response))
                {
                    success?.Invoke(response.balance);
                }
            }
        }

        #endregion Get Balance

        #region Get Access Token

        #region Properties

        public bool HasAccessToken
        {
            get
            {
                return currentAccessToken.Length > 0;
            }
        }

        public string AccessToken
        {
            get
            {
                return currentAccessToken;
            }
            set
            {
                currentAccessToken = value;
            }
        }

        #endregion Properties

        public delegate void AccessTokenSuccess(string accessToken);
        public void GetAccessToken(AccessTokenSuccess success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineGetAccessToken(success, error));
        }

        private IEnumerator CoroutineGetAccessToken(AccessTokenSuccess success, GenericError error)
        {
            Debug.Log("GetAccessToken request started");
            string url = LocalEmergenceServer.Instance.Environment().APIBase + "get-access-token";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                PrintRequestResult("GetAccessToken", request);
                if (ProcessRequest<AccessTokenResponse>(request, error, out var response))
                {
                    currentAccessToken = SerializationHelper.Serialize(response.AccessToken, false);
                    ProcessExpiration(response.AccessToken.message);
                    success?.Invoke(currentAccessToken);
                }
            }
        }

        #endregion Get Access Token

        #region Validate Access Token

        public delegate void ValidateAccessTokenSuccess(bool valid);
        public void ValidateAccessToken(ValidateAccessTokenSuccess success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineValidateAccessToken(success, error));
        }

        private IEnumerator CoroutineValidateAccessToken(ValidateAccessTokenSuccess success, GenericError error)
        {
            Debug.Log("ValidateAccessToken request started");

            string url = LocalEmergenceServer.Instance.Environment().APIBase + "validate-access-token" + "?accessToken=" + currentAccessToken;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                PrintRequestResult("ValidateAccessToken", request);
                if (ProcessRequest<ValidateAccessTokenResponse>(request, error, out var response))
                {
                    success?.Invoke(response.valid);
                }
            }
        }

        #endregion Validate Access Token

        #region Validate Signed Message

        public delegate void ValidateSignedMessageSuccess(bool valid);
        public void ValidateSignedMessage(string message, string signedMessage, string address, ValidateSignedMessageSuccess success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineValidateSignedMessage(message, signedMessage, address, success, error));
        }

        private IEnumerator CoroutineValidateSignedMessage(string message, string signedMessage, string address, ValidateSignedMessageSuccess success, GenericError error)
        {
            Debug.Log("ValidateSignedMessage request started");

            ValidateSignedMessageRequest data = new ValidateSignedMessageRequest()
            {
                message = message,
                signedMessage=signedMessage,
                address=address
            };

            string dataString = SerializationHelper.Serialize(data, false);

            string url = LocalEmergenceServer.Instance.Environment().APIBase + "validate-signed-message" + "?request=" + currentAccessToken;
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";
                yield return request.SendWebRequest();
                PrintRequestResult("ValidateSignedMessage", request);
                if (ProcessRequest<ValidateSignedMessageResponse>(request, error, out var response))
                {
                    success?.Invoke(response.valid);
                }
            }
        }

        #endregion Validate Signed Message

        #region Disconnect Wallet

        private bool disconnectInProgress = false;
        public delegate void DisconnectSuccess();
        public void Disconnect(DisconnectSuccess success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }

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
            string url = LocalEmergenceServer.Instance.Environment().APIBase + "killSession";

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
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineFinish(success, error));
        }

        private IEnumerator CoroutineFinish(SuccessFinish success, GenericError error)
        {
            Debug.Log("Finish request started");
            string url = LocalEmergenceServer.Instance.Environment().APIBase + "finish";

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

        public delegate void LoadContractSuccess();
        public void LoadContract(string contractAddress, string ABI, LoadContractSuccess success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineLoadContract(contractAddress, ABI, success, error));
        }

        public IEnumerator CoroutineLoadContract(string contractAddress, string ABI, LoadContractSuccess success, GenericError error)
        {
            Debug.Log("LoadContract request started");

            Contract data = new Contract()
            {
                contractAddress = contractAddress,
                ABI = ABI
            };

            string dataString = SerializationHelper.Serialize(data, false);
            string url = LocalEmergenceServer.Instance.Environment().APIBase + "loadContract";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                PrintRequestResult("Load Contract", request);
                if (ProcessRequest<LoadContractResponse>(request, error, out var response))
                {
                    success?.Invoke();
                }
            }
        }

        #endregion Load Contract

        #region GetTransactionStatus
        public delegate void GetTransactionStatusSuccess<T>(T response);
        internal void GetTransactionStatus<T>(string transactionHash, string nodeURL, GetTransactionStatusSuccess<T> success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineGetTransactionStatus<T>(transactionHash, nodeURL, success, error));
        }

        private IEnumerator CoroutineGetTransactionStatus<T>(string transactionHash, string nodeURL, GetTransactionStatusSuccess<T> success, GenericError error)
        {
            Debug.Log("Get Transaction Status request started [" + transactionHash + "] / " + nodeURL);

            string url = LocalEmergenceServer.Instance.Environment().APIBase + "GetTransactionStatus?transactionHash=" + transactionHash + "&nodeURL=" + nodeURL;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                PrintRequestResult("Get Transaction Status", request);
                if (ProcessRequest<T>(request, error, out var response))
                {
                    success?.Invoke(response); // Should we change this pattern?
                }
            }
        }
        #endregion

        #region GetBlockNumber

        public delegate void GetBlockNumberSuccess<T>(T response);
        internal void GetBlockNumber<T, U>(string transactionHash, string nodeURL, U body, GetBlockNumberSuccess<T> success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineGetBlockNumber<T, U>(transactionHash, nodeURL, body, success, error));
        }

        private IEnumerator CoroutineGetBlockNumber<T, U>(string transactionHash, string nodeURL, U body, GetBlockNumberSuccess<T> success, GenericError error)
        {
            Debug.Log("Get Block Number request started [" + transactionHash + "] / " + nodeURL);

            string url = LocalEmergenceServer.Instance.Environment().APIBase + "getBlockNumber?nodeURL=" + nodeURL;

            string dataString = SerializationHelper.Serialize(body, false);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                PrintRequestResult("Get Block Number", request);
                if (ProcessRequest<T>(request, error, out var response))
                {
                    success?.Invoke(response);
                }
            }
        }
        #endregion


        #region Read Method

        public delegate void ReadMethodSuccess<T>(T response);
        public void ReadContract<T, U>(string contractAddress, string methodName, U body, ReadMethodSuccess<T> success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineReadMethod<T, U>(contractAddress, methodName, body, success, error));
        }

        public IEnumerator CoroutineReadMethod<T, U>(string contractAddress, string methodName, U body, ReadMethodSuccess<T> success, GenericError error)
        {
            Debug.Log("ReadMethod request started [" + contractAddress + "] / " + methodName);

            string url = LocalEmergenceServer.Instance.Environment().APIBase + "readMethod?contractAddress=" + contractAddress + "&methodName=" + methodName;

            string dataString = SerializationHelper.Serialize(body, false);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                PrintRequestResult("Read Contract", request);
                if (ProcessRequest<T>(request, error, out var response))
                {
                    success?.Invoke(response);
                }
            }
        }

        #endregion Read Contract

        #region Write Method

        public delegate void WriteMethodSuccess<T>(T response);
        public void WriteContract<T, U>(string contractAddress, string methodName, string localAccountName, string gasPrice, U body, WriteMethodSuccess<T> success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineWriteMethod<T, U>(contractAddress, methodName, localAccountName, gasPrice, body, success, error));
        }

        public IEnumerator CoroutineWriteMethod<T, U>(string contractAddress, string methodName, string localAccountName, string gasPrice, U body, WriteMethodSuccess<T> success, GenericError error)
        {
            Debug.Log("WriteContract request started [" + contractAddress + "] / " + methodName);

            string gasPriceString = String.Empty;
            string localAccountNameString = String.Empty;

            if (gasPrice != String.Empty && localAccountName != String.Empty)
            {
                gasPriceString = "&gasPrice=" + gasPrice;
                localAccountNameString = "&localAccountName=" + localAccountName;
            }

            string url = LocalEmergenceServer.Instance.Environment().APIBase + "writeMethod?contractAddress=" + contractAddress + "&methodName=" + methodName + localAccountNameString + gasPriceString;

            string dataString = SerializationHelper.Serialize(body, false);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                PrintRequestResult("Write Contract", request);
                if (ProcessRequest<T>(request, error, out var response))
                {
                    success?.Invoke(response);
                }
            }
        }

        #endregion Write Contract

        #endregion Contracts

        #endregion EVM Server
    }
}
