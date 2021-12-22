using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Emergence
{
    public class NetworkManager : MonoBehaviour
    {
        private readonly string APIBase = "http://localhost:50733/api/";
        private readonly string DatabaseAPIPublic = "https://pfy3t4mqjb.execute-api.us-east-1.amazonaws.com/staging/";
        private readonly string DatabaseAPIPrivate = "https://57l0bi6g53.execute-api.us-east-1.amazonaws.com/staging/";
        private readonly string defaultNodeURL = "https://polygon-mainnet.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134";

        private string nodeURL = string.Empty;
        private string gameId = string.Empty;
        private string currentAccessToken = string.Empty;

        public static NetworkManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            /*
            if (Token expired)
            {
                GetToken();
            }
            */
        }

        public void StartEVMServer(string nodeURL, string gameId)
        {
            this.nodeURL = defaultNodeURL;

            if (!String.IsNullOrEmpty(nodeURL.Trim()))
            {
                this.nodeURL = nodeURL;
            }

            this.gameId = gameId;

            Ping(() =>
            {

            },
            (error, code) =>
            {
                Debug.LogWarning("EVM code not running, trying to launch");

                try
                {
                    // TODO send process id set a reference to the current process and use System.Diagnostics's Process.Id property:int nProcessID = Process.GetCurrentProcess().Id;
                    System.Diagnostics.Process.Start("run-server.bat");
                    Debug.Log("Running Emergence Server");
                }
                catch (Exception e)
                {
                    Debug.Log("Server error: " + e.Message);
                }
            });
        }

        public void StopEVMServer()
        {
            Finish(() =>
            {

            },
            (error, code) =>
            {
                try
                {
                    System.Diagnostics.Process.Start("stop-server.bat");
                    Debug.Log("Stopping Emergence Server");
                }
                catch (Exception e)
                {
                    Debug.Log("Server error: " + e.Message);
                }
            });
        }

        public delegate void PingSuccess();
        public delegate void GenericError(string message, long code);
        public void Ping(PingSuccess success, GenericError error)
        {
            StartCoroutine(CoroutinePing(success, error));
        }

        private IEnumerator CoroutinePing(PingSuccess success, GenericError error)
        {
            Debug.Log("CoroutinePing request started");

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
                    success?.Invoke();
                }
            }
        }

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

        public delegate void HandshakeSuccess();
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

                PrintRequestResult("GetQrCode", request);

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

        public delegate void BalanceSuccess(string balance);

        public void GetBalance(BalanceSuccess success, GenericError error)
        {
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
                    success?.Invoke(request.downloadHandler.text);
                }
            }
        }

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
                    AccessTokenResponse accesstokenResponse = SerializationHelper.Deserialize<AccessTokenResponse>(request.downloadHandler.text);
                    currentAccessToken = SerializationHelper.Serialize(accesstokenResponse.message.accessToken, false);
                    success?.Invoke(currentAccessToken);
                }
            }
        }

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

        private void PrintRequestResult(string name, UnityWebRequest request)
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
    }
}