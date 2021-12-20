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
            UnityEngine.Debug.Log("IsConnected request started");
            if (!String.IsNullOrEmpty(nodeURL))
            {
                //UnityEngine.Debug.Log("nodeURL override " + nodeURL);
            }
            else
            {
                nodeURL = networkManager.defaultNodeURL;
                //UnityEngine.Debug.Log("using default url " + networkManager.defaultNodeURL);
            }

            // TODO send process id set a reference to the current process and use System.Diagnostics's Process.Id property:int nProcessID = Process.GetCurrentProcess().Id;

            string uri = networkManager.APIBase + "isConnected";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();
                StartCoroutine(GetQrCode());
                StartCoroutine(GetHandshake());

            }
        }

        IEnumerator GetQrCode()
        {
            UnityEngine.Debug.Log("GetQrCode request started");
            string url = networkManager.APIBase + "qrcode";
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                UnityEngine.Debug.Log(request.error);
            }
            else 
            { 
                rawQRImage.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            }
            UnityEngine.Debug.Log("GetQrCode request completed");
            //UnityEngine.Debug.Log(request.isDone.ToString() + "QRrequest done");
            //StartCoroutine(GetWallectConnectUri());
            
            
        }

        IEnumerator GetWallectConnectUri()
        {
            //throw new NotImplementedException();
            UnityEngine.Debug.Log("GetWallectConnectUri request started");
            string uri = networkManager.APIBase + "getwalletconnecturi";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();
                UnityEngine.Debug.Log("GetWallectConnectUri request completed");
                StartCoroutine(GetHandshake());

            }
        }
        public IEnumerator GetHandshake()
        {
            UnityEngine.Debug.Log("GetHandshake request started");
            if (!String.IsNullOrEmpty(nodeURL))
            {
                //UnityEngine.Debug.Log("nodeURL override " + nodeURL);
            }
            else
            {
                nodeURL = networkManager.defaultNodeURL;
            }


            string uri = networkManager.APIBase + "handshake" + "?nodeUrl=" + nodeURL;

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.responseCode == 200)
                {
                    //StartCoroutine(GetQrCode());
                    StartCoroutine(ReinitializeWalletConnect());
                }
                UnityEngine.Debug.Log("GetHandshake request completed");
            }

            

        }

        IEnumerator ReinitializeWalletConnect()
        {
            UnityEngine.Debug.Log("ReinitializeWalletConnect request started");
            string url = networkManager.APIBase + "reinitializewalletconnect";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

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
            UnityEngine.Debug.Log("GetAccessToken request started");
            string uri = networkManager.APIBase + "get-access-token";
            ///save access token
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();
                if (webRequest.responseCode == 200)
                {
                    //parse json and get access token value
                    currentAccessToken = webRequest.GetResponseHeader("accessToken");
                }
            }
        }

        IEnumerator GetBalance()
        {
            UnityEngine.Debug.Log("GetBalance request started");
            string uri = networkManager.APIBase + "getbalance";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();
                if (webRequest.responseCode == 200)
                {
                    //parse json and get the balance from the result
                    //StartCoroutine(GetAccessToken());
                }

            }
        }


    }

}
