using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using UnityEngine;

namespace EmergenceSDK
{
    public class TextureEditor : MonoBehaviour
    {
        void Start()
        {
            DownloadImage downloadImage = new DownloadImage();
            downloadImage.Download(null, "https://www.litmus.com/wp-content/uploads/2021/02/motion-tween-example.gif", HandleImageDownloadSuccess, (url, error, code) => { }).Forget();
            
        }

        private void HandleImageDownloadSuccess(string url, Texture2D texture, DownloadImage self)
        {
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;
        }
    }
}
