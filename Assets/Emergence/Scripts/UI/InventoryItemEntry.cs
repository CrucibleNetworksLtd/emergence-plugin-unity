using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK
{
    public class InventoryItemEntry : MonoBehaviour
    {
        
        [SerializeField] private RawImage itemImage;
        public TextMeshProUGUI itemName;
        public string url;
        
        private void Awake()
        {
            RequestImage.Instance.OnImageReady += Instance_OnImageReady;
            RequestImage.Instance.OnImageFailed += Instance_OnImageFailed;
        }
        
        private void OnDestroy()
        {
            RequestImage.Instance.OnImageReady -= Instance_OnImageReady;
            RequestImage.Instance.OnImageFailed -= Instance_OnImageFailed;
        }

        public void SetImageUrl(string url)
        {
            RequestImage.Instance.AskForImage(url);
        }
        
        private void Instance_OnImageReady(string _url, Texture2D texture)
        {
            if (_url.Equals(url))
            {
                itemImage.texture = texture;
            }
        }

        private void Instance_OnImageFailed(string url, string error, long errorCode)
        {
            Debug.LogError("[" + url + "] " + error + " " + errorCode);
        }

    }
}
