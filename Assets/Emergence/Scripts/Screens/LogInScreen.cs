using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Emergence
{
    public class LogInScreen : MonoBehaviour
    {
        [Header("UI References")]
        public RawImage rawQRImage;
        public TextMeshProUGUI refreshCounterText;

        public float timeRemaining = 60;

        NetworkManager networkManager = new NetworkManager();

        //private readonly string APIBase = "http://localhost:50733/api/";
        //private readonly string DatabaseAPIPublic = "https://pfy3t4mqjb.execute-api.us-east-1.amazonaws.com/staging/";
        //private readonly string DatabaseAPIPrivate = "https://57l0bi6g53.execute-api.us-east-1.amazonaws.com/staging/";
        //private readonly string defaultNodeURL = "https://polygon-mainnet.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134";

        [SerializeField]
        private string nodeURL;

        public bool serverRunning = false;

        private string currentAccessToken;

        private void Start()
        {
            //TODO: ping server/check connectivity 
            StartCoroutine(IsConnected());
        }

        void Update()
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                refreshCounterText.text = Convert.ToInt32(timeRemaining).ToString();
            }
            else
            {
                timeRemaining = 60;
                refreshCounterText.text = "60";
                StartCoroutine(GetQrCode());
            }
        }

        private IEnumerator IsConnected()
        {
            Debug.Log("IsConnected request started");
            if (!String.IsNullOrEmpty(nodeURL))
            {
                //Debug.Log("nodeURL override " + nodeURL);
            }
            else
            {
                nodeURL = networkManager.defaultNodeURL;
                //Debug.Log("using default url " + networkManager.defaultNodeURL);
            }

            // TODO send process id set a reference to the current process and use System.Diagnostics's Process.Id property:int nProcessID = Process.GetCurrentProcess().Id;

            string uri = networkManager.APIBase + "isConnected";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();
                PrintRequestResult("IsConnected", webRequest);

                StartCoroutine(GetQrCode());
                StartCoroutine(GetHandshake());
            }
        }

        IEnumerator GetQrCode()
        {
            Debug.Log("GetQrCode request started");
            string url = networkManager.APIBase + "qrcode";
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url);
            yield return webRequest.SendWebRequest();

            PrintRequestResult("GetQrCode", webRequest);

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else 
            { 
                rawQRImage.texture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
            }
        }

        IEnumerator GetWallectConnectUri()
        {
            Debug.Log("GetWallectConnectUri request started");
            string uri = networkManager.APIBase + "getwalletconnecturi";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();
                PrintRequestResult("GetWallectConnectUri", webRequest);
            }
        }

        public IEnumerator GetHandshake()
        {
            Debug.Log("GetHandshake request started");
            if (!String.IsNullOrEmpty(nodeURL))
            {
                //Debug.Log("nodeURL override " + nodeURL);
            }
            else
            {
                nodeURL = networkManager.defaultNodeURL;
            }

            string uri = networkManager.APIBase + "handshake" + "?nodeUrl=" + nodeURL;

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();
                PrintRequestResult("GetHandshake", webRequest);

                if (webRequest.responseCode == 200)
                {
                    // StartCoroutine(ReinitializeWalletConnect());
                    StartCoroutine(GetBalance());
                    StartCoroutine(GetAccessToken());
                }
            }
        }

        IEnumerator ReinitializeWalletConnect()
        {
            Debug.Log("ReinitializeWalletConnect request started");
            string url = networkManager.APIBase + "reinitializewalletconnect";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();
                PrintRequestResult("ReinitializeWalletConnect", webRequest);
                if (webRequest.responseCode == 200)
                {
                    //parse json and get the balance from the result
                    StartCoroutine(GetBalance());
                    StartCoroutine(GetAccessToken());
                    
                }
                //get the response status code
                //StartCoroutine(GetWallectConnectUri());
            }
        }

        IEnumerator GetAccessToken()
        {
            Debug.Log("GetAccessToken request started");
            string uri = networkManager.APIBase + "get-access-token";
            ///save access token
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();
                PrintRequestResult("GetAccessToken", webRequest);
                if (webRequest.responseCode == 200)
                {
                    //parse json and get access token value
                    currentAccessToken = webRequest.GetResponseHeader("accessToken");
                }
            }
        }

        IEnumerator GetBalance()
        {
            Debug.Log("GetBalance request started");
            string uri = networkManager.APIBase + "getbalance";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();
                PrintRequestResult("GetBalance", webRequest);
                if (webRequest.responseCode == 200)
                {
                    //parse json and get the balance from the result
                }
            }
        }

        private void PrintRequestResult(string name, UnityWebRequest request)
        {
            Debug.Log(name + " completed " + request.responseCode);
            if (request.isHttpError || request.isNetworkError)
            {
                Debug.LogError(request.error);
            } else
            {
                Debug.Log(request.downloadHandler.text);
            }
        }
    }
}
