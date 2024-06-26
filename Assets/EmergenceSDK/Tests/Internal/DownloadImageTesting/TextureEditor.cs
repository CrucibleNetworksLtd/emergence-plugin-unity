using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Internal.Utils;
using UnityEngine;

namespace EmergenceSDK.Tests.Internal.DownloadImageTesting
{
    public class TextureEditor : MonoBehaviour
    {
        void Start()
        {
            DownloadImage downloadImage = new DownloadImage();
            downloadImage.Download(null, "https://ipfs.io/ipfs/QmSFGKuCsZEPzEEuPUGHyuc1YMsqi4jJdgKkDhyxgu1AHS/33.gif", HandleImageDownloadSuccess, (url, error, code) => { }).Forget();
            
            //gameObject.GetComponent<Renderer>().material.mainTexture = await GifToJpegConverter.ConvertGifToJpegFromUrl("https://upload.wikimedia.org/wikipedia/commons/d/d3/Newtons_cradle_animation_book_2.gif");
            //gameObject.GetComponent<Renderer>().material.mainTexture = await GifToJpegConverter.ConvertGifToJpegFromUrl("https://slackmojis.com/emojis/3643-cool-doge/download");
        }

        private void HandleImageDownloadSuccess(string url, Texture2D texture, DownloadImage self)
        {
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;
        }
    }
}
