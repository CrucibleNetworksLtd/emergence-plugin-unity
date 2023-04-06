using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace EmergenceSDK.Internal.Utils
{
    public class DownloadImage
    {
        UnityWebRequest request = null;
        public RequestImage.DownloadReady successCallback = null;
        public RequestImage.DownloadFailed failedCallback = null;

        public async UniTask Download(RequestImage ri, string url, RequestImage.DownloadReady success, RequestImage.DownloadFailed failed)
        {
            request = UnityWebRequestTexture.GetTexture(url);
            successCallback = success;
            failedCallback = failed;

            await MakeRequest();
        }

        private async UniTask MakeRequest()
        {
            await request.SendWebRequest();

            if (request.responseCode == 200)
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, 0, false);
                bool error = false;

                if (request.downloadedBytes > 0)
                {
                    try
                    {
                        texture = DownloadHandlerTexture.GetContent(request);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e.Message);
                        error = true;
                    }

                    if (texture == null)
                    {
                        Debug.LogWarning("Couldn't convert downloaded image at " + request.url);
                        failedCallback?.Invoke(request.url, "Couldn't convert downloaded image", 0);
                    }
                }
                else
                {
                    error = true;
                }

                if (error)
                {
                    // Transparent texture
                    texture.SetPixels(new Color[]
                    {
                        new Color(0,0,0,0),
                        new Color(0,0,0,0),
                        new Color(0,0,0,0),
                        new Color(0,0,0,0),
                    });

                    texture.Apply();
                }

                successCallback?.Invoke(request.url, texture, this);
            }
            else
            {
                failedCallback?.Invoke(request.url, request.error, request.responseCode);
                Debug.LogWarning("Failed to download image at " + request.url);
            }

            request = null;
        }
    }
}