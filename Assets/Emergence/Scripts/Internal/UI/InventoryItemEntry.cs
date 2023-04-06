using System;
using System.Linq;
using DG.Tweening.Core.Enums;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace EmergenceSDK.Internal.UI
{
    public class InventoryItemEntry : MonoBehaviour
    {
        [FormerlySerializedAs("itemThumbnail")] [SerializeField] 
        private InventoryItemThumbnail ItemThumbnail;
        [FormerlySerializedAs("itemName")] [SerializeField] 
        private TextMeshProUGUI ItemName;
        private string url;
        
        private bool loadOnStart;
        private bool rotateOnLoading;
        private bool outputDebugLog;
        
        private InventoryItem item;

        public void SetItem(InventoryItem item)
        {
            this.item = item;
            
            ItemName.text = item?.Meta?.Name;
            SetImageUrl(item?.Meta?.Content?.FirstOrDefault()?.URL);
        }

        public void SetImageUrl(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                RequestImage.Instance.AskForDefaultImage();
            }
            else if (IsGifUrl(imageUrl)) 
            {
                ItemThumbnail.LoadGif(imageUrl);
            }
            else 
            {
                RequestImage.Instance.AskForImage(imageUrl, Instance_OnImageReady, Instance_OnImageFailed);
            }
        }

        private bool IsGifUrl(string url)
        {
            string lowerUrl = url.ToLower();
            string[] urlParts = lowerUrl.Split('.');
            string extension = urlParts[urlParts.Length - 1];

            // Check if the extension is "gif" and the URL has a valid format
            return extension == "gif" && Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        
        private void Instance_OnImageReady(string urlIn, Texture2D texture)
        {
            ItemThumbnail.LoadStaticImage(texture);
        }

        private void Instance_OnImageFailed(string imageUrl, string error, long errorCode)
        {
            Debug.LogError("[" + imageUrl + "] " + error + " " + errorCode);
        }
    }
}
