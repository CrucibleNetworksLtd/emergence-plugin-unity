using System.Collections;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    public class QRCodeService : IQRCodeService
    {
        public async UniTask GetQRCode(QRCodeSuccess success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "qrcode";

            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            await request.SendWebRequest().ToUniTask();

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