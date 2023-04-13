using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI
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
                Debug.LogWarning("URL is invalid.");
                return;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(imageUrl))
            {
                await request.SendWebRequest().ToUniTask();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("File load error.\n" + request.error);
                    itemImage.texture = RequestImage.Instance.DefaultThumbnail;
                    return;
                }

                using (var decoder = new MG.GIF.Decoder(request.downloadHandler.data))
                {
                    var img = decoder.NextImage();
                    LoadStaticImage(img.CreateTexture());
                    return;
                    
                    //EXPERIMENTAL: This code does enable gif playback, but it is ver resource intensive and causes a lot of lag.
                    //Remove the return statement above to enable it.
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
