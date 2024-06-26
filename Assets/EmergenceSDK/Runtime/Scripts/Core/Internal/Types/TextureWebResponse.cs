using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Runtime.Internal.Types
{
    public class TextureWebResponse : WebResponse
    {
        public Texture2D Texture => ((DownloadHandlerTexture)Request.downloadHandler)?.texture;
        public TextureWebResponse(UnityWebRequest request) : base(request) { }
    }
}