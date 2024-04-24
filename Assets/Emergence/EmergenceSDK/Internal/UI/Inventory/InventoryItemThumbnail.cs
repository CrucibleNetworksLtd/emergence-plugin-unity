using System;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using MG.GIF;
using UnityEngine;
using UnityEngine.UI;
using EmergenceSDK.Internal.Services;

namespace EmergenceSDK.Internal.UI.Inventory
{
    public class InventoryItemThumbnail : MonoBehaviour
    {
        [SerializeField]
        private RawImage itemImage;
        
        public void LoadStaticImage(Texture2D texture)
        {
            itemImage.texture = texture;
        }

        public async void LoadGif(string url)
        {
            await SetGifFromUrl(url);
        }

        private async UniTask SetGifFromUrl(string imageUrl)
        {
            try
            {
                //Note that if you want to load a gif larger than 16MB, you will need to increase this value,
                //this is designed to only download enough for the first frame of at most a 4k gif, so animated gifs will be much larger
                int maxFrameSizeBytes = 16778020;

                var request = UnityWebRequest.Get(imageUrl);
                request.SetRequestHeader("Range", "bytes=0-" + (maxFrameSizeBytes - 1));

                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);

                if (!response.IsSuccess)
                {
                    EmergenceLogger.LogWarning("File load error.\n");
                    itemImage.texture = RequestImage.Instance.DefaultThumbnail;
                    return;
                }

                byte[] imageData = response.DownloadHandler.data;

                using (var decoder = new Decoder(imageData))
                {
                    try
                    {
                        var img = decoder.NextImage();
                        if(img == null)
                        {
                            itemImage.texture = RequestImage.Instance.DefaultThumbnail;
                            return;
                        }
                        LoadStaticImage(img.CreateTexture());
                    }
                    catch (UnsupportedGifException)
                    {
                        EmergenceLogger.LogInfo("Invalid gif.");
                        itemImage.texture = RequestImage.Instance.DefaultThumbnail;
                    }
                }
            }
            catch (Exception e)
            {
                EmergenceLogger.LogError("Error in SetGifFromUrl.\n" + e.Message);
                itemImage.texture = RequestImage.Instance.DefaultThumbnail;
            }
        }
    }
}
