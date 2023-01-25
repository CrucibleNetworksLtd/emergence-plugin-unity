using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK
{
    public class InventoryItemEntry : MonoBehaviour
    {
        
        [SerializeField] private RawImage itemImage;
        [SerializeField] private TextMeshProUGUI itemName;
        private string url;

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

        public void SetImageUrl(string url)
        {
            RequestImage.Instance.AskForImage(url);
        }
        
        private void Instance_OnImageReady(string _url, Texture2D texture)
        {
            if (_url.Equals(item?.meta?.content?.First().url))
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
