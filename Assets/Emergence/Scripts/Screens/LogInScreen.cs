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

        private readonly string APIBase = "http://localhost:50733/api/";
        private readonly string DatabaseAPIPublic = "https://pfy3t4mqjb.execute-api.us-east-1.amazonaws.com/staging/";
        private readonly string DatabaseAPIPrivate = "https://57l0bi6g53.execute-api.us-east-1.amazonaws.com/staging/";
        private readonly string defaultNodeURL = "https://polygon-mainnet.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134";

        [SerializeField]
        private string nodeURL;

        public bool serverRunning = false;

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
                UnityEngine.Debug.Log("nodeURL override " + nodeURL);
            }
            else
            {
                nodeURL = defaultNodeURL;
                UnityEngine.Debug.Log("using default url " + defaultNodeURL);
            }

            // TODO send process id set a reference to the current process and use System.Diagnostics's Process.Id property:int nProcessID = Process.GetCurrentProcess().Id;

            string uri = APIBase + "isConnected";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();
                StartCoroutine(GetQrCode());
            }
        }

        IEnumerator GetQrCode()
        {
            UnityEngine.Debug.Log("GetQrCode request started");
            string url = APIBase + "qrcode";
            UnityEngine.Debug.Log("Geting QRCode request started");
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
                UnityEngine.Debug.Log(request.error);
            else
                rawQRImage.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            UnityEngine.Debug.Log(request.isDone.ToString() + "QRrequest done");
            StartCoroutine(GetWallectConnectUri());
        }

        IEnumerator GetWallectConnectUri()
        {
            //throw new NotImplementedException();
            UnityEngine.Debug.Log("GetWallectConnectUri request started");
            string uri = APIBase + "getwalletconnecturi";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();

                StartCoroutine(GetHandshake());
            }
        }
        public IEnumerator GetHandshake()
        {
            UnityEngine.Debug.Log("GetHandshake request started");
            if (!String.IsNullOrEmpty(nodeURL))
            {
                UnityEngine.Debug.Log("nodeURL override " + nodeURL);
            }
            else
            {
                nodeURL = defaultNodeURL;
                UnityEngine.Debug.Log("using default url " + defaultNodeURL);
            }


            string uri = APIBase + "handshake" + "?nodeUrl=" + nodeURL;

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();

                //TODO: move to helper file, handle network errors 
                /*
                string[] pages = uri.Split('/');
                int page = pages.Length - 1;


                switch (webRequest.responseCode)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        UnityEngine.Debug.LogError("GetHandshake request: Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        UnityEngine.Debug.LogError("GetHandshake request + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                         UnityEngine.Debug.Log("GetHandshake request Success");
                         StartCoroutine(GetQrCode());
                        break;
                }*/

            }
            UnityEngine.Debug.Log("GetHandshake request completed");

        }

    }

}
