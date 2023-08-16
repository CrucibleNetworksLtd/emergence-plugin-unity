using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Services;
using EmergenceSDK.Internal.Utils;
using MG.GIF;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI.Inventory
{
    public class InventoryItemThumbnail : MonoBehaviour
    {
        [SerializeField]
        private RawImage itemImage;

        private bool isPlaying = false;

        private List<Texture2D> frames = new List<Texture2D>();
        private List<float> frameDelays = new List<float>();

        private int curFrame = 0;

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
            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, imageUrl, "");
            int maxFrameSizeBytes = 16778020; // 16MB
            //Note that if you want to load a gif larger than 16MB, you will need to increase this value,
            //this is designed to only download enough for the first frame, so animated gifs will be much larger
            request.SetRequestHeader("Range", $"bytes=0-{maxFrameSizeBytes - 1}");
            
            await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
            if (request.result != UnityWebRequest.Result.Success)
            {
                EmergenceLogger.LogWarning("File load error.\n" + request.error);
                itemImage.texture = RequestImage.Instance.DefaultThumbnail;
                return;
            }

            using (var decoder = new Decoder(request.downloadHandler.data))
            {
                WebRequestService.CleanupRequest(request);
                try
                {
                    var img = decoder.NextImage();
                    LoadStaticImage(img.CreateTexture());
                }
                catch (UnsupportedGifException)
                {
                    EmergenceLogger.LogInfo("Invalid gif.");
                    itemImage.texture = RequestImage.Instance.DefaultThumbnail;
                }
            }
        }
    }
}
