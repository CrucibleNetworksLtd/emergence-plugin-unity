using UnityEngine;
using UnityEngine.UI;

namespace Emergence
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

        private void Awake()
        {
            selectButton.onClick.AddListener(OnSelectClicked);
            RequestImage.Instance.OnImageReady += Instance_OnImageReady;
            RequestImage.Instance.OnImageFailed += Instance_OnImageFailed;
        }

        private void OnDestroy()
        {
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

            if (!RequestImage.Instance.AskForImage(avatar.url))
            {
                OnImageCompleted?.Invoke(avatar, false);
            }
        }

        private void Instance_OnImageReady(string url, Texture2D texture)
        {
            if (url.Equals(avatar.url))
            {
                avatarRawImage.texture = texture;
                OnImageCompleted?.Invoke(avatar, true);
            }
        }

        private void Instance_OnImageFailed(string url, string error, long errorCode)
        {
            if (url.Equals(avatar.url))
            {
                Debug.LogError("[" + url + "] " + error + " " + errorCode);
                OnImageCompleted?.Invoke(avatar, false);
            }
        }
    }
}