using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using MG.GIF;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI.Inventory
{
    public class InventoryItemThumbnail : MonoBehaviour
    {
        [SerializeField]
        private RawImage itemImage;

        private static HttpClient client = new();

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

                var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
                request.Headers.Range = new RangeHeaderValue(0, maxFrameSizeBytes - 1);

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    EmergenceLogger.LogWarning("File load error.\n" + response.ReasonPhrase);
                    itemImage.texture = RequestImage.Instance.DefaultThumbnail;
                    return;
                }

                byte[] imageData = await response.Content.ReadAsByteArrayAsync();

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
            catch (HttpRequestException e)
            {
                EmergenceLogger.LogError("Error making the HTTP request.\n" + e.Message);
                itemImage.texture = RequestImage.Instance.DefaultThumbnail;
            }
        }
    }
}
