using System.Collections;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    public class QRCodeService : MonoBehaviour, IQRCodeService
    {
        public void GetQRCode(QRCodeSuccess success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineGetQrCode(success, errorCallback));
        }

        private IEnumerator CoroutineGetQrCode(QRCodeSuccess success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "qrcode";

            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();

                EmergenceUtils.PrintRequestResult("GetQrCode", request);

                if (EmergenceUtils.RequestError(request))
                {
                    errorCallback?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    string deviceId = request.GetResponseHeader("deviceId");
                    success?.Invoke((request.downloadHandler as DownloadHandlerTexture).texture, deviceId);
                }
            }
        }
    }
}