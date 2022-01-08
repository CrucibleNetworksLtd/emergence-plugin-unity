using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Emergence
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

        [SerializeField]
        private Button usePersonaAsCurrentButton;

        private Persona persona;

        public delegate void ImageCompleted(Persona persona, bool success);
        public static event ImageCompleted OnImageCompleted;

        private void Awake()
        {
            selectButton.onClick.AddListener(OnSelectClicked);
            usePersonaAsCurrentButton.onClick.AddListener(OnUsePersonaAsCurrentClicked);

            RequestImage.Instance.OnImageReady += Instance_OnImageReady;
            RequestImage.Instance.OnImageFailed += Instance_OnImageFailed;
        }

        private void OnDestroy()
        {
            RequestImage.Instance.OnImageReady -= Instance_OnImageReady;
            RequestImage.Instance.OnImageFailed -= Instance_OnImageFailed;
        }

        public delegate void Selected(Persona persona);
        public static event Selected OnSelected;
        private void OnSelectClicked()
        {
            OnSelected?.Invoke(persona);
        }

        public static event Selected OnUsePersonaAsCurrent;
        private void OnUsePersonaAsCurrentClicked()
        {
            OnUsePersonaAsCurrent?.Invoke(persona);
        }

        public void Refresh(Texture2D texture, Persona persona, bool selected)
        {
            this.persona = persona;

            nameText.gameObject.SetActive(false);
            nameText.text = persona.name;
            
            if (persona.AvatarImage == null)
            {
                persona.AvatarImage = texture;
            }
            else
            {
                photo.texture = persona.AvatarImage;
            }

            unselectedBorder.SetActive(!selected);
            selectedBorder.SetActive(selected);

            if (!RequestImage.Instance.AskForImage(persona.avatar.url))
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
            if (persona != null && url == persona.avatar.url)
            {
                persona.AvatarImage = texture;
                photo.texture = persona.AvatarImage;
                OnImageCompleted?.Invoke(persona, true);
            }
        }

        private void Instance_OnImageFailed(string url, string error, long errorCode)
        {
            if (persona != null && url == persona.avatar.url)
            {
                Debug.LogError("[" + url + "] [" + errorCode + "] " + error);
                OnImageCompleted?.Invoke(persona, false);
            }
        }
    }
}