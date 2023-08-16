using System;
using System.Net.Http;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace EmergenceSDK.Internal.Utils
{
    public class DownloadImage
    {
        HttpClient client = new HttpClient();
        public RequestImage.DownloadReady successCallback = null;
        public RequestImage.DownloadFailed failedCallback = null;

        public async UniTask Download(RequestImage ri, string url, RequestImage.DownloadReady success, RequestImage.DownloadFailed failed)
        {
            successCallback = success;
            failedCallback = failed;

            await MakeRequest(url);
        }

        private async UniTask MakeRequest(string url)
        {
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(url);
            }
            catch (HttpRequestException e)
            {
                failedCallback?.Invoke(url, e.Message, 0); // Use 0 or a different default value for non-http-related exceptions.
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                failedCallback?.Invoke(url, response.ReasonPhrase, (int)response.StatusCode);
                EmergenceLogger.LogWarning("Failed to download image at " + url);
                return;
            }

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, 0, false);
            bool error = false;

            try
            {
                byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                if (!texture.LoadImage(imageBytes))
                {
                    error = true;
                    EmergenceLogger.LogWarning("Couldn't convert downloaded image at " + url);
                    failedCallback?.Invoke(url, "Couldn't convert downloaded image", 0);
                }
            }
            catch (System.Exception e)
            {
                EmergenceLogger.LogError(e.Message);
                error = true;
            }

            if (error)
            {
                // Transparent texture
                texture.SetPixels(new Color[]
                {
                    new Color(0, 0, 0, 0),
                    new Color(0, 0, 0, 0),
                    new Color(0, 0, 0, 0),
                    new Color(0, 0, 0, 0),
                });

                texture.Apply();
            }

            successCallback?.Invoke(url, texture, this);
        }
    }
}
