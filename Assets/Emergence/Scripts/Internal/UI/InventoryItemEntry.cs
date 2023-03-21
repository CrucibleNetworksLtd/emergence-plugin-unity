using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI
{
    public class InventoryItemEntry : MonoBehaviour
    {
        [SerializeField] 
        private InventoryItemThumbnail itemThumbnail;
        [SerializeField] 
        private TextMeshProUGUI itemName;
        private string url;
        
        private bool loadOnStart;
        private bool rotateOnLoading;
        private bool outputDebugLog;
        
        private InventoryItem item;

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

        public void SetItem(InventoryItem item)
        {
            this.item = item;
            
            itemName.text = item?.meta?.name;
            SetImageUrl(item?.meta?.content?.First().url);
        }

        public void SetImageUrl(string imageUrl)
        {
            //TODO: more robust check for gif
            if (!string.IsNullOrEmpty(imageUrl) && imageUrl.ToLower().Contains("gif")) 
            {
                itemThumbnail.LoadGif(imageUrl);
            }
            else 
            {
                RequestImage.Instance.AskForImage(imageUrl);
            }
        }
        
        private void Instance_OnImageReady(string _url, Texture2D texture)
        {
            if (_url.Equals(item?.meta?.content?.First().url))
            {
                itemThumbnail.LoadStaticImage(texture);
            }
        }

        private void Instance_OnImageFailed(string imageUrl, string error, long errorCode)
        {
            Debug.LogError("[" + imageUrl + "] " + error + " " + errorCode);
        }
    }
}
