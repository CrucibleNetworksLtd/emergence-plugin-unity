using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EmergenceSDK
{
    public class PersonaScrollItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField]
        private RawImage photo;

        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private GameObject unselectedBorder;

        [SerializeField]
        private GameObject selectedBorder;

        [SerializeField]
        private Button selectButton;

        private Persona persona;

        public delegate void ImageCompleted(Persona persona, bool success);
        public static event ImageCompleted OnImageCompleted;

        private bool waitingForImageRequest = false;
        private int index;

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

        public delegate void Selected(Persona persona, int childIndex);
        public static event Selected OnSelected;
        private void OnSelectClicked()
        {
            OnSelected?.Invoke(persona, index);
        }

        public void Refresh(Texture2D texture, Persona persona, bool selected)
        {
            this.persona = persona;
            this.index = this.transform.GetSiblingIndex();
            nameText.gameObject.SetActive(false);
            nameText.text = persona.name;

            if (persona.AvatarImage == null)
            {
                persona.AvatarImage = texture;
            }

            photo.texture = persona.AvatarImage;

            unselectedBorder.SetActive(!selected);
            selectedBorder.SetActive(selected);

            if (persona.avatar != null && persona.avatar.url != null)
            {
                waitingForImageRequest = true;
                if (!RequestImage.Instance.AskForImage(persona.avatar.url))
                {
                    waitingForImageRequest = false;
                    OnImageCompleted?.Invoke(persona, false);
                }
            }
            else
            {
                OnImageCompleted?.Invoke(persona, false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            nameText.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            nameText.gameObject.SetActive(false);
        }

        private void Instance_OnImageReady(string url, Texture2D texture)
        {
            if (waitingForImageRequest && url == persona.avatar.url)
            {
                persona.AvatarImage = texture;
                photo.texture = persona.AvatarImage;
                waitingForImageRequest = false;
                OnImageCompleted?.Invoke(persona, true);
            }
        }

        private void Instance_OnImageFailed(string url, string error, long errorCode)
        {
            if (waitingForImageRequest && url == persona.avatar.url)
            {
                waitingForImageRequest = false;
                Debug.LogError("[" + url + "] [" + errorCode + "] " + error);
                OnImageCompleted?.Invoke(persona, false);
            }
        }
    }
}