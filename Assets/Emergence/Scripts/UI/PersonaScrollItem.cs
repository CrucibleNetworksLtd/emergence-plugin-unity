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

        public void Refresh(Persona persona, bool selected)
        {
            this.persona = persona;

            nameText.gameObject.SetActive(false);
            nameText.text = persona.name;
            photo.texture = persona.AvatarImage;
            unselectedBorder.SetActive(!selected);
            selectedBorder.SetActive(selected);

            RequestImage.Instance.AskForImage(persona.avatar.url);
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
            }
        }

        private void Instance_OnImageFailed(string url, string error, long errorCode)
        {
            if (persona != null && url == persona.avatar.url)
            {
                Debug.LogError("[" + url + "] [" + errorCode + "] " + error);
            }
        }
    }
}