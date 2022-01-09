using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Emergence
{
    public class NetworkManager : MonoBehaviour
    {
        private readonly string APIBase = "http://localhost:50733/api/";
        private readonly string defaultNodeURL = "https://polygon-mainnet.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134";

        // Dev
        private readonly string DatabaseAPIPrivate = "https://57l0bi6g53.execute-api.us-east-1.amazonaws.com/staging/";

        // Staging
        //private readonly string DatabaseAPIPrivate = "https://x8iq9e5fq1.execute-api.us-east-1.amazonaws.com/staging";

        // Prod
        //private readonly string DatabaseAPIPrivate = "https://i30mnhu5vg.execute-api.us-east-1.amazonaws.com/prod";



        private string nodeURL = string.Empty;
        private string gameId = string.Empty;
        private string currentAccessToken = string.Empty;

        public static NetworkManager Instance;

        private bool skipWallet = false;

        public delegate void GenericError(string message, long code);

        #region Monobehaviour

        private void Awake()
        {
            Instance = this;
        }

        private bool refreshingToken = false;
        private void Update()
        {
            if (EmergenceManager.Instance == null)
            {
                return;
            }

            bool uiIsVisible = EmergenceManager.Instance.gameObject.activeSelf;

            if (!skipWallet && uiIsVisible && !refreshingToken && HasAccessToken)
            {
                long now = DateTimeOffset.Now.ToUnixTimeSeconds();

                if (expiration.expiresOn - now < 0)
                {
                    refreshingToken = true;
                    ModalPromptOK.Instance.Show("Token expired. Check your wallet for renewal", () =>
                    {
                        GetAccessToken((token) =>
                        {
                            refreshingToken = false;
                        },
                        (error, code) =>
                        {
                            Debug.LogError("[" + code + "] " + error);
                            refreshingToken = false;
                        });
                    });
                }
            }
        }

        #endregion Monobehaviour

        #region Properties

        public bool HasAccessToken
        {
            get
            {
                return currentAccessToken.Length > 0;
            }
        }

        #endregion Properties

        #region EVM Server

        #region Start and Stop

        public void SetupAndStartEVMServer(string nodeURL, string gameId)
        {
            this.nodeURL = defaultNodeURL;

            if (!String.IsNullOrEmpty(nodeURL.Trim()))
            {
                this.nodeURL = nodeURL;
            }

            this.gameId = gameId;

            StartEVMServer();
        }

        public bool StartEVMServer()
        {
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
            StartCoroutine(CoroutineIsConnected(success, error));
        }

        private IEnumerator CoroutineIsConnected(IsConnectedSuccess success, GenericError error)
        {
            Debug.Log("CoroutineIsConnected request started");

            string url = APIBase + "isConnected";

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
            StartCoroutine(CoroutineReinitializeWalletConnect(success, error));
        }

        private IEnumerator CoroutineReinitializeWalletConnect(ReinitializeWalletConnectSuccess success, GenericError error)
        {
            Debug.Log("CoroutineReinitializeWalletConnect request started");

            string url = APIBase + "reinitializewalletconnect";

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
            StartCoroutine(CoroutineGetQrCode(success, error));
        }

        private IEnumerator CoroutineGetQrCode(QRCodeSuccess success, GenericError error)
        {
            Debug.Log("GetQrCode request started");
            string url = APIBase + "qrcode";

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
            StartCoroutine(CoroutineHandshake(success, error));
        }

        private IEnumerator CoroutineHandshake(HandshakeSuccess success, GenericError error)
        {
            Debug.Log("Handshake request started");
            string url = APIBase + "handshake" + "?nodeUrl=" + nodeURL;

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
            string url = APIBase + "getbalance";

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
            StartCoroutine(CoroutineGetAccessToken(success, error));
        }

        private IEnumerator CoroutineGetAccessToken(AccessTokenSuccess success, GenericError error)
        {
            Debug.Log("GetAccessToken request started");
            string url = APIBase + "get-access-token";

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
            string url = APIBase + "killSession";

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
            StartCoroutine(CoroutineFinish(success, error));
        }

        private IEnumerator CoroutineFinish(SuccessFinish success, GenericError error)
        {
            Debug.Log("Finish request started");
            string url = APIBase + "finish";

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

        #endregion EVM Server

        #region AWS API

        public delegate void SuccessPersonas(List<Persona> personas, Persona currentPersona);

        public void GetPersonas(SuccessPersonas success, GenericError error)
        {
            StartCoroutine(CoroutineGetPersonas(success, error));
        }

        private IEnumerator CoroutineGetPersonas(SuccessPersonas success, GenericError error)
        {
            Debug.Log("GetPersonas request started");
            string url = DatabaseAPIPrivate + "personas";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", currentAccessToken);
                yield return request.SendWebRequest();
                PrintRequestResult("GetPersonas", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    PersonasResponse personasResponse = SerializationHelper.Deserialize<PersonasResponse>(request.downloadHandler.text);
                    success?.Invoke(personasResponse.personas, personasResponse.personas.FirstOrDefault(p => p.id == personasResponse.selected));
                }
            }
        }

        public delegate void SuccessCreatePersona();
        public void CreatePersona(Persona persona, SuccessCreatePersona success, GenericError error)
        {
            StartCoroutine(CoroutineCreatePersona(persona, success, error));
        }

        private IEnumerator CoroutineCreatePersona(Persona persona, SuccessCreatePersona success, GenericError error)
        {
            Debug.Log("CreatePersona request started");
            string jsonPersona = SerializationHelper.Serialize(persona);
            Debug.Log("Json Persona: " + jsonPersona);
            Debug.Log("currentAccessToken: " + currentAccessToken);

            string url = DatabaseAPIPrivate + "persona";

            using (UnityWebRequest request = UnityWebRequest.Post(url, string.Empty))
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPersona));
                request.uploadHandler.contentType = "application/json";

                request.SetRequestHeader("Authorization", currentAccessToken);

                yield return request.SendWebRequest();
                PrintRequestResult("Save Persona", request);

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

        public delegate void SuccessEditPersona();
        public void EditPersona(Persona persona, SuccessEditPersona success, GenericError error)
        {
            StartCoroutine(CoroutineEditPersona(persona, success, error));
        }

        private IEnumerator CoroutineEditPersona(Persona persona, SuccessEditPersona success, GenericError error)
        {
            Debug.Log("Edit Persona request started");
            string jsonPersona = SerializationHelper.Serialize(persona);
            Debug.Log("Json Persona: " + jsonPersona);
            Debug.Log("currentAccessToken: " + currentAccessToken);

            string url = DatabaseAPIPrivate + "persona";

            using (UnityWebRequest request = UnityWebRequest.Post(url, string.Empty))
            {
                request.method = "PATCH";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPersona));
                request.uploadHandler.contentType = "application/json";

                request.SetRequestHeader("Authorization", currentAccessToken);

                yield return request.SendWebRequest();
                PrintRequestResult("Save Persona", request);

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

        public delegate void SuccessDeletePersona();
        public void DeletePersona(Persona persona, SuccessDeletePersona success, GenericError error)
        {
            StartCoroutine(CoroutineDeletePersona(persona, success, error));
        }

        private IEnumerator CoroutineDeletePersona(Persona persona, SuccessDeletePersona success, GenericError error)
        {
            Debug.Log("DeletePersona request started");
            string url = DatabaseAPIPrivate + "persona/" + persona.id;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "DELETE";
                request.SetRequestHeader("Authorization", currentAccessToken);
                yield return request.SendWebRequest();
                PrintRequestResult("Delete Persona Persona", request);

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

        public delegate void SuccessSetCurrentPersona();
        public void SetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, GenericError error)
        {
            StartCoroutine(CoroutineSetCurrentPersona(persona, success, error));
        }

        private IEnumerator CoroutineSetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, GenericError error)
        {
            Debug.Log("Set Current Persona request started");
            string url = DatabaseAPIPrivate + "setActivePersona/" + persona.id;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "PATCH";
                request.SetRequestHeader("Authorization", currentAccessToken);
                yield return request.SendWebRequest();
                PrintRequestResult("Set Current Persona", request);

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

        public delegate void SuccessAvatars(List<Persona.Avatar> avatar);

        public void GetAvatars(SuccessAvatars success, GenericError error)
        {
            StartCoroutine(CoroutineGetAvatars(success, error));
        }

        private IEnumerator CoroutineGetAvatars(SuccessAvatars success, GenericError error)
        {
            Debug.Log("Get Avatars request started");
            string url = DatabaseAPIPrivate + "userUnlockedAvatars?id=" + gameId;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", currentAccessToken);
                yield return request.SendWebRequest();
                PrintRequestResult("Get Avatars", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    GetAvatarsResponse response = SerializationHelper.Deserialize<GetAvatarsResponse>(request.downloadHandler.text);
                    success?.Invoke(response.avatars);
                }
            }
        }

        #endregion AWS API

        #region No Wallet Cheat

        public void SkipWallet(bool skip, string accessTokenJson)
        {
            skipWallet = skip;

            AccessTokenResponse response = SerializationHelper.Deserialize<AccessTokenResponse>(accessTokenJson);
            currentAccessToken = SerializationHelper.Serialize(response.message.accessToken, false);
            ProcessExpiration(response.message.accessToken.message);
        }

        #endregion No Wallet Cheat

        #region Utilities

        private class Expiration
        {
            [JsonProperty("expires-on")]
            public long expiresOn;
        }

        private Expiration expiration;
        private void ProcessExpiration(string expirationMessage)
        {
            expiration = SerializationHelper.Deserialize<Expiration>(expirationMessage);
        }

        private bool RequestError(UnityWebRequest request)
        {
#if UNITY_2020_1_OR_NEWER
            return (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError ||
                request.result == UnityWebRequest.Result.DataProcessingError);
#else
            return (request.isHttpError || request.isNetworkError);
#endif
        }

        private void PrintRequestResult(string name, UnityWebRequest request)
        {
            Debug.Log(name + " completed " + request.responseCode);
            if (RequestError(request))
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
            }
        }

        #endregion Utilities

        #region Contracts

        #region Load Contract

        public delegate void SuccessLoadContract();
        public void LoadContract(string contractAddress, string ABI, SuccessLoadContract success, GenericError error)
        {
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
            string url = APIBase + "loadContract";

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
            StartCoroutine(CoroutineReadContract<T, U>(contractAddress, methodName, body, success, error));
        }

        public IEnumerator CoroutineReadContract<T, U>(string contractAddress, string methodName, U body, SuccessReadContract<T> success, GenericError error)
        {
            Debug.Log("ReadContract request started [" + contractAddress + "] / " + methodName);

            string url = APIBase + "readMethod?contractAddress=" + contractAddress + "&methodName=" + methodName;

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
            StartCoroutine(CoroutineWriteContract<T, U>(contractAddress, methodName, body, success, error));
        }

        public IEnumerator CoroutineWriteContract<T, U>(string contractAddress, string methodName, U body, SuccessWriteContract<T> success, GenericError error)
        {
            Debug.Log("WriteContract request started [" + contractAddress + "] / " + methodName);

            string url = APIBase + "writeMethod?contractAddress=" + contractAddress + "&methodName=" + methodName;

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
    }
}