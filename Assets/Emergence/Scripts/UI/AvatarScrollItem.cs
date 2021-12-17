using UnityEngine;
using UnityEngine.UI;

namespace Emergence
{
    public class AvatarScrollItem : MonoBehaviour
    {
        [Header("UI Reference")]
        [SerializeField]
        private RawImage avatar;

        [SerializeField]
        private Button selectButton;

        [SerializeField]
        private AspectRatioFitter ratioFitter;

        private string id;

        private void Awake()
        {
            selectButton.onClick.AddListener(OnSelectClicked);
        }

        public delegate void Selected(string id);
        public static event Selected OnAvatarSelected;
        private void OnSelectClicked()
        {
            OnAvatarSelected?.Invoke(id);
        }

        public void Refresh(Texture2D avatar, string id)
        {
            this.id = id;
            this.avatar.texture = avatar;

            ratioFitter.aspectRatio = (float)avatar.width / (float)avatar.height;
        }
    }
}