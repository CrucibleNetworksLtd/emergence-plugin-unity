using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Services;
using EmergenceSDK.Internal.Types;
using MG.GIF;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Utils
{
    public class DownloadImage
    {
        public RequestImage.DownloadReady successCallback = null;
        public RequestImage.DownloadFailed failedCallback = null;
        
        enum ImageFormat
        {
            Unknown,
            JPG,
            GIF,
            PNG,
            BMP,
            TGA
        }

        public async UniTask Download(RequestImage ri, string url, RequestImage.DownloadReady success, RequestImage.DownloadFailed failed)
        {
            successCallback = success;
            failedCallback = failed;

            await MakeRequest(url);
        }

        private async UniTask MakeRequest(string url)
        {
            var request = UnityWebRequest.Get(url);
            WebResponse response;

            try
            {
                response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
            }
            catch (Exception e)
            {
                failedCallback?.Invoke(url, e.Message, 0); // Use 0 or a different default value for non-http-related exceptions.
                return;
            }

            if (!response.IsSuccess)
            {
                failedCallback?.Invoke(url, response.Response, response.StatusCode);
                EmergenceLogger.LogWarning("Failed to download image at " + url);
                return;
            }

            byte[] imageBytes = response.DownloadHandler.data;
            var texture = await GetTextureFromImageBytes(imageBytes, url);
            successCallback?.Invoke(url, texture, this);
        }
        
        public async UniTask<Texture2D> GetTextureFromImageBytes(byte[] imageBytes, string url)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, 0, false);
            switch (DetectImageFormat(imageBytes))
            {
                case ImageFormat.JPG:
                case ImageFormat.PNG:
                case ImageFormat.BMP:
                case ImageFormat.TGA:
                    texture.LoadImage(imageBytes);
                    break;
                case ImageFormat.GIF:
                    try
                    {
                        texture = await GifToJpegConverter.ConvertGifToJpegFromUrl(url);
                        break;
                    }
                    catch (UnsupportedGifException)
                    {
                        texture = Resources.Load<Texture2D>("NoPreviewImage");
                    }
                    break;
                case ImageFormat.Unknown:
                default:
                    texture = Resources.Load<Texture2D>("NoPreviewImage");
                    break;
            }
            return texture;
        }
        
        static ImageFormat DetectImageFormat(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length < 4)
            {
                return ImageFormat.Unknown;
            }

            byte[] jpgMagicBytes = { 0xFF, 0xD8, 0xFF };
            byte[] gifMagicBytes = { 0x47, 0x49, 0x46, 0x38 };
            byte[] pngMagicBytes = { 0x89, 0x50, 0x4E, 0x47 };
            byte[] bmpMagicBytes = { 0x42, 0x4D };
            byte[] tgaMagicBytes = { 0x00, 0x02 };

            // Detect JPG
            if (IsPrefixOf(jpgMagicBytes, byteArray))
            {
                return ImageFormat.JPG;
            }

            // Detect GIF
            if (IsPrefixOf(gifMagicBytes, byteArray))
            {
                return ImageFormat.GIF;
            }

            // Detect PNG
            if (IsPrefixOf(pngMagicBytes, byteArray))
            {
                return ImageFormat.PNG;
            }

            // Detect BMP
            if (IsPrefixOf(bmpMagicBytes, byteArray))
            {
                return ImageFormat.BMP;
            }

            // Detect TGA
            if (IsPrefixOf(tgaMagicBytes, byteArray))
            {
                return ImageFormat.TGA;
            }

            return ImageFormat.Unknown;
        }

        static bool IsPrefixOf(byte[] prefix, byte[] array)
        {
            if (prefix.Length > array.Length)
            {
                return false;
            }

            for (int i = 0; i < prefix.Length; i++)
            {
                if (prefix[i] != array[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
