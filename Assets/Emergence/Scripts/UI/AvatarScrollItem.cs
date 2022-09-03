using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK
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

        private Persona.Avatar avatar;

        public delegate void ImageCompleted(Persona.Avatar avatar, bool success);
        public static event ImageCompleted OnImageCompleted;

        private bool waitingForImageRequest = false;

        private void Awake()
        {
            selectButton.onClick.AddListener(OnSelectClicked);
            RequestImage.Instance.OnImageReady += Instance_OnImageReady;
            RequestImage.Instance.OnImageFailed += Instance_OnImageFailed;
        }

        private void OnDestroy()
        {
            selectButton.onClick.RemoveListener(OnSelectClicked);
            RequestImage.Instance.OnImageReady -= Instance_OnImageReady;
            RequestImage.Instance.OnImageFailed -= Instance_OnImageFailed;
        }

        public delegate void Selected(Persona.Avatar avatar);
        public static event Selected OnAvatarSelected;
        private void OnSelectClicked()
        {
            OnAvatarSelected?.Invoke(avatar);
        }

        public void Refresh(Texture2D texture, Persona.Avatar avatar)
        {
            this.avatar = avatar;
            avatarRawImage.texture = texture;

            ratioFitter.aspectRatio = (float)texture.width / (float)texture.height;

            // If avatar is null then this avatar scroll item is the default image,
            // and will never call RequestImage
            if (avatar != null && avatar.url != null)
            {
                waitingForImageRequest = true;
                if (!RequestImage.Instance.AskForImage(avatar.url))
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
            if (waitingForImageRequest && url.Equals(avatar.url))
            {
                avatarRawImage.texture = texture;
                waitingForImageRequest = false;
                OnImageCompleted?.Invoke(avatar, true);
            }
        }

        private void Instance_OnImageFailed(string url, string error, long errorCode)
        {
            if (waitingForImageRequest && url.Equals(avatar.url))
            {
                waitingForImageRequest = false;
                Debug.LogError("[" + url + "] " + error + " " + errorCode);
                OnImageCompleted?.Invoke(avatar, false);
            }
        }
    }
}