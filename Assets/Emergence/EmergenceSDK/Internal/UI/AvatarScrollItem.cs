using System.Linq;
using EmergenceSDK.Internal.Utils;
using UnityEngine;
using UnityEngine.UI;
using Avatar = EmergenceSDK.Types.Avatar;

namespace EmergenceSDK.Internal.UI
{
    public class AvatarScrollItem : MonoBehaviour
    {
        [Header("UI Reference")]
        [SerializeField]
        private RawImage avatarRawImage;

        [SerializeField]
        private Button selectButton;

        [SerializeField]
        private AspectRatioFitter ratioFitter;

        private Avatar avatar;

        public delegate void ImageCompleted(Avatar avatar, bool success);
        public static event ImageCompleted OnImageCompleted;

        private bool waitingForImageRequest = false;

        private void Awake()
        {
            selectButton.onClick.AddListener(OnSelectClicked);
            RequestImage.Instance.OnImageReady += Instance_OnImageReady;
        }

        private void OnDestroy()
        {
            selectButton.onClick.RemoveListener(OnSelectClicked);
            RequestImage.Instance.OnImageReady -= Instance_OnImageReady;
        }

        public delegate void Selected(Avatar avatar);
        public static event Selected OnAvatarSelected;
        private void OnSelectClicked()
        {
            OnAvatarSelected?.Invoke(avatar);
        }

        public void Refresh(Texture2D texture, Avatar avatar)
        {
            this.avatar = avatar;
            avatarRawImage.texture = texture;

            ratioFitter.aspectRatio = (float)texture.width / (float)texture.height;

            // If avatar is null then this avatar scroll item is the default image,
            // and will never call RequestImage
            if (avatar != null && avatar.meta.content.First().url != null)
            {
                waitingForImageRequest = true;
                if (!RequestImage.Instance.AskForImage(avatar.meta.content.First().url))
                {
                    waitingForImageRequest = false;
                    OnImageCompleted?.Invoke(avatar, false);
                }
            }
            else
            {
                OnImageCompleted?.Invoke(avatar, false);
            }
        }

        private void Instance_OnImageReady(string url, Texture2D texture)
        {
            if (waitingForImageRequest && url.Equals(avatar.meta.content.First().url))
            {
                avatarRawImage.texture = texture;
                waitingForImageRequest = false;
                OnImageCompleted?.Invoke(avatar, true);
            }
        }
    }
}