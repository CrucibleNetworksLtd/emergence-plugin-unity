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

        public async void LoadGif(string url, bool autoPlay = true)
        {
            await SetGifFromUrl(url, autoPlay);
        }

        private async UniTask SetGifFromUrl(string imageUrl, bool autoPlay = true)
        {
            bool isWebsiteAlive = await Helpers.IsWebsiteAlive(imageUrl);
            if (string.IsNullOrEmpty(imageUrl) || !isWebsiteAlive)
            {
                EmergenceLogger.LogWarning("URL is invalid.");
                return;
            }

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, imageUrl, "");
            await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);

            if (request.result != UnityWebRequest.Result.Success)
            {
                EmergenceLogger.LogWarning("File load error.\n" + request.error);
                itemImage.texture = RequestImage.Instance.DefaultThumbnail;
                return;
            }

            using (var decoder = new Decoder(request.downloadHandler.data))
            {
                try
                {
                    var img = decoder.NextImage();
                    LoadStaticImage(img.CreateTexture());
                }
                catch (UnsupportedGifException)
                {
                    EmergenceLogger.LogInfo("Invalid gif.");
                    itemImage.texture = RequestImage.Instance.DefaultThumbnail;
                    return;
                }

                //EXPERIMENTAL: This code does enable gif playback, but it is ver resource intensive and causes a lot of lag.
                //Comment back in to try it out!
                /*
                while (img != null)
                {
                    frames.Add(img.CreateTexture());
                    frameDelays.Add(img.Delay / 1000.0f);
                    img = decoder.NextImage();

                    await UniTask.Delay(1000 / 60);
                }
            }

            if (autoPlay)
            {
                _ = PlayGif();
            }
            else
            {
                itemImage.texture = frames[0];
            }
            */
            }
        }

        private async UniTask PlayGif()
        {
            isPlaying = true;
            curFrame = 0;

            while (isPlaying && itemImage != null)
            {
                itemImage.texture = frames[curFrame];
                float delay = frameDelays[curFrame];
                await UniTask.Delay((int)(delay * 1000));
                curFrame = (curFrame + 1) % frames.Count;
            }
        }
    }
}
