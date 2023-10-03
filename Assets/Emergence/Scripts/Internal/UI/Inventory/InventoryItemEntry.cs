using System;
using System.Linq;
using DG.Tweening.Core.Enums;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace EmergenceSDK.Internal.UI.Inventory
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
        
        /// <summary>
        /// A shallow copy of the item that this entry represents, editing this item will have no effect on the inventory.
        /// </summary>
        public InventoryItem Item => new InventoryItem(item);
        private InventoryItem item;

        public void Start()
        {
            ItemThumbnail.LoadStaticImage(RequestImage.Instance.DownloadingThumbnail);
        }

        public void SetItem(InventoryItem item)
        {
            this.item = item;
            
            ItemName.text = item?.Meta?.Name;
            
            var thumbnailType = ThumbnailType.None;
            
            if (item?.Meta?.Content?.FirstOrDefault()?.MimeType == "image/png" ||
                item?.Meta?.Content?.FirstOrDefault()?.MimeType == "image/jpeg")
            {
                thumbnailType = ThumbnailType.Static;
            }
            else if (item?.Meta?.Content?.FirstOrDefault()?.MimeType == "image/gif")
            {
                thumbnailType = ThumbnailType.Gif;
            }

            SetImageUrl(item?.Meta?.Content?.FirstOrDefault()?.URL, thumbnailType);
        }

        public void SetImageUrl(string imageUrl, ThumbnailType thumbnailType)
        {
            switch (thumbnailType)
            {
                case ThumbnailType.Static:
                    RequestImage.Instance.AskForImage(imageUrl, Instance_OnImageReady, Instance_OnImageFailed);
                    break;
                case ThumbnailType.Gif:
                    ItemThumbnail.LoadGif(imageUrl);
                    break;
                case ThumbnailType.None:
                    ItemThumbnail.LoadStaticImage(RequestImage.Instance.DefaultThumbnail);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(thumbnailType), thumbnailType, null);
            }
        }

        
        private void Instance_OnImageReady(string urlIn, Texture2D texture)
        {
            ItemThumbnail.LoadStaticImage(texture);
        }

        private void Instance_OnImageFailed(string error, long errorCode)
        {
            ItemThumbnail.LoadStaticImage(RequestImage.Instance.DefaultThumbnail);
            EmergenceLogger.LogWarning(error + " " + errorCode);
        }
        
        public enum ThumbnailType
        {
            Static,
            Gif,
            None
        }
    }
}
