using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using UnityEngine;

namespace EmergenceSDK
{
    public class TextureEditor : MonoBehaviour
    {
        async void Start()
        {
            //DownloadImage downloadImage = new DownloadImage();
            //downloadImage.Download(null, "https://camo.githubusercontent.com/e8ca3b797d79bc5e8e08b23a36f2839de7790ccfe97496bba7052bd1df156301/68747470733a2f2f677761726564642e6769746875622e696f2f6d674769662f627574746572666c792e676966", HandleImageDownloadSuccess, (url, error, code) => { }).Forget();
            gameObject.GetComponent<Renderer>().material.mainTexture = await GifToJpegConverter.ConvertGifToJpegFromUrl("https://techboomers.com/wp-content/uploads/2018/11/example-1.gif");
        }

        private void HandleImageDownloadSuccess(string url, Texture2D texture, DownloadImage self)
        {
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;
        }
    }
}
