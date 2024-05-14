using System;
using System.Collections;
using System.Collections.Generic;
using EmergenceSDK.Internal.Services;
using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Types
{
    public class TextureWebResponse : WebResponse
    {
        public Texture2D Texture => ((DownloadHandlerTexture)Request.downloadHandler)?.texture;
        public TextureWebResponse(UnityWebRequest request) : base(request) { }
    }
}