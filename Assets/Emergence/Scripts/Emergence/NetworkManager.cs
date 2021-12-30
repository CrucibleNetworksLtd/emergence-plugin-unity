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
        private readonly string DatabaseAPIPublic = "https://pfy3t4mqjb.execute-api.us-east-1.amazonaws.com/staging/";
        private readonly string DatabaseAPIPrivate = "https://57l0bi6g53.execute-api.us-east-1.amazonaws.com/staging/";
        private readonly string defaultNodeURL = "https://polygon-mainnet.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134";

        private readonly string contractAddress = "0x9498274B8C82B4a3127D67839F2127F2Ae9753f4";
        private readonly string ABI = "[{'inputs':[{'internalType':'string','name':'name','type':'string'},{'internalType':'string','name':'symbol','type':'string'}],'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'owner','type':'address'},{'indexed':true,'internalType':'address','name':'approved','type':'address'},{'indexed':true,'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'Approval','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'owner','type':'address'},{'indexed':true,'internalType':'address','name':'operator','type':'address'},{'indexed':false,'internalType':'bool','name':'approved','type':'bool'}],'name':'ApprovalForAll','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'TokenMinted','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'from','type':'address'},{'indexed':true,'internalType':'address','name':'to','type':'address'},{'indexed':true,'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'Transfer','type':'event'},{'inputs':[{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'approve','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'}],'name':'balanceOf','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getApproved','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'},{'internalType':'address','name':'operator','type':'address'}],'name':'isApprovedForAll','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'player','type':'address'},{'internalType':'string','name':'tokenURI','type':'string'}],'name':'mint','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'nonpayable','type':'function'},{'inputs':[],'name':'name','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'ownerOf','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'safeTransferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'},{'internalType':'bytes','name':'_data','type':'bytes'}],'name':'safeTransferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'operator','type':'address'},{'internalType':'bool','name':'approved','type':'bool'}],'name':'setApprovalForAll','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'bytes4','name':'interfaceId','type':'bytes4'}],'name':'supportsInterface','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'symbol','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'tokenURI','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'transferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'}]";
        private string nodeURL = string.Empty;
        private string gameId = string.Empty;
        private string currentAccessToken = string.Empty;

        private string avatarMetadataURI = string.Empty;

        public static NetworkManager Instance;

        private bool skipWallet = false;

        public delegate void GenericError(string message, long code);

        #region Monobehaviour

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            // TODO handle access token expiration
            /*
            if (Token expired)
            {
                GetToken();
            }
            */
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

        private int attempts = 0;
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
                Process.Start("run-server.bat");
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

                if (request.isHttpError || request.isNetworkError)
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

                if (request.isHttpError || request.isNetworkError)
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

                if (request.isNetworkError || request.isHttpError)
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

                if (request.isNetworkError || request.isHttpError)
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

                if (request.isNetworkError || request.isHttpError)
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

                if (request.isNetworkError || request.isHttpError)
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
                        success?.Invoke(currentAccessToken);
                    }
                }
            }
        }

        #endregion Get Access Token

        #region Disconnect Wallet

        public delegate void DisconnectSuccess();
        public void Disconnect(DisconnectSuccess success, GenericError error)
        {
            if (skipWallet)
            {
                success?.Invoke();
                return;
            }

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

                if (request.isNetworkError || request.isHttpError)
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
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

                if (request.isNetworkError || request.isHttpError)
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

                if (request.isNetworkError || request.isHttpError)
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

                if (request.isNetworkError || request.isHttpError)
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

                if (request.isNetworkError || request.isHttpError)
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

                if (request.isNetworkError || request.isHttpError)
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

                if (request.isNetworkError || request.isHttpError)
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

                if (request.isNetworkError || request.isHttpError)
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

            AccessTokenResponse at = SerializationHelper.Deserialize<AccessTokenResponse>(accessTokenJson);
            currentAccessToken = SerializationHelper.Serialize(at.message.accessToken, false);
        }

        #endregion No Wallet Cheat

        #region Debug info

        public void PrintRequestResult(string name, UnityWebRequest request)
        {
            Debug.Log(name + " completed " + request.responseCode);
            if (request.isHttpError || request.isNetworkError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
            }
        }

        #endregion Debug info

        #region Contracts

        public delegate void SuccessWriteContract();

        public IEnumerator CoroutineWriteContract(SuccessWriteContract success, GenericError error, string ContractAddress, string ABI, string MethodName)
        {
            Debug.Log("WriteContract request started");
            Debug.Log("currentAccessToken: " + currentAccessToken);

            string url = APIBase + "writeMethod?contractAddress=" + ContractAddress + "&methodName=" + MethodName;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", currentAccessToken);
                yield return request.SendWebRequest();
                PrintRequestResult("Write Contract", request);

                if (request.isNetworkError || request.isHttpError)
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    //GetAvatarsResponse response = SerializationHelper.Deserialize<GetAvatarsResponse>(request.downloadHandler.text);
                    //success?.Invoke(response.avatars);
                }
            }
            using (UnityWebRequest request = UnityWebRequest.Post(url, string.Empty))
            {

                //request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPersona));
                request.uploadHandler.contentType = "application/json";

                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();
                PrintRequestResult("Load Contract", request);

                if (request.isNetworkError || request.isHttpError)
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    success?.Invoke();
                }

                /*
                HandshakeResponse response = SerializationHelper.Deserialize<HandshakeResponse>(request.downloadHandler.text);
                if (response.statusCode != 0)
                {
                    error?.Invoke("Problem with handshake", response.statusCode);
                }
                else
                {
                    //success?.Invoke(response.message.address);
                }*/
            }
        }

        public delegate void SuccessLoadContract();

        public void LoadContract(SuccessLoadContract success, GenericError error)
        {
            StartCoroutine(CoroutineLoadContract(success, error));
        }

        public IEnumerator CoroutineLoadContract(SuccessLoadContract success, GenericError error)
        {
            Debug.Log("LoadContract request started");

            Debug.Log("currentAccessToken: " + currentAccessToken);

            var data = new {
                contractAddress = contractAddress,
                ABI = ABI
            };

            string dataString = SerializationHelper.Serialize(data);
            

            string url = APIBase + "loadContract";

            Debug.Log("LoadContract request started with JSON, calling LoadContract_HttpRequestComplete on request completed. Json sent as part of the request: " + dataString);



            using (UnityWebRequest request = UnityWebRequest.Post(url, string.Empty))
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                PrintRequestResult("Load Contract", request);

                if (request.isNetworkError || request.isHttpError)
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    Debug.Log("Load Contract DATA: " + request.downloadHandler.data);
                    success?.Invoke();
                }
            }
        }

        public delegate void SuccessReadContract();

        public void ReadContract(SuccessReadContract success, GenericError error)
        {
            StartCoroutine(CoroutineReadContract(success, error));
        }

        public IEnumerator CoroutineReadContract(SuccessReadContract success, GenericError error)
        {
            Debug.Log("ReadContract request started");
        
            //content string armar con Json
            Debug.Log("currentAccessToken: " + currentAccessToken);

            string methodName = "tokenURI";

            string url = APIBase + "readMethod?contractAddress=" + contractAddress + "&methodName=" + methodName;
            string[] data = { "1" };

            string dataString = SerializationHelper.Serialize(data);

            using (UnityWebRequest request = UnityWebRequest.Post(url, string.Empty))
            {

                request.uploadHandler.contentType = "application/json";
                request.SetRequestHeader("Content-Type", "application/json");
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));

                yield return request.SendWebRequest();
                PrintRequestResult("Load Contract", request);

                if (request.isNetworkError || request.isHttpError)
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    Debug.Log("The response message Is: " + request.downloadHandler.text);
                    //The response message Is: {"statusCode":1,"message":null}
                    //LoadContractResponse response = SerializationHelper.Deserialize<LoadContractResponse>(request.downloadHandler.text);
                    success?.Invoke();
                }
            }
        }

        public delegate void SuccessGetNFTAvatar(string avatarJson);

        public void GetNFTAvatar(SuccessGetNFTAvatar success, GenericError error)
        {
            StartCoroutine(CoroutineGetNFTAvatar(success, error));
        }


        private IEnumerator CoroutineGetNFTAvatar(SuccessGetNFTAvatar success, GenericError error)
        {
            Debug.Log("GetNFTAvatar request started with JSON from " + avatarMetadataURI);
            string url = avatarMetadataURI;

            //using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                
                yield return request.SendWebRequest();

                PrintRequestResult("GetNFTAvatar", request);

                if (request.isNetworkError || request.isHttpError)
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    //success?.Invoke((request.downloadHandler as DownloadHandlerTexture).texture);
                }
            }
        }

        private void DownloadNFTAvatar(string text)
        {
            Debug.Log("DownloadNFTAvatar from" + text);
            //get image reference, assign the texture 2d
        }

        #endregion Contracts
    }
}