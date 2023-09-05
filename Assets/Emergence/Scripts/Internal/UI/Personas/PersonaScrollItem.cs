using System.Linq;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI.Personas
{
    public class PersonaScrollItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField]
        private RawImage photo;

        [SerializeField]
        private Mask mask;

        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private GameObject unselectedBorder;

        [SerializeField]
        private GameObject selectedBorder;

        [SerializeField]
        private Button selectButton;

        public Persona Persona
        {
            get;
            private set;
        }

        public delegate void ImageCompleted(Persona persona, bool success);
        public static event ImageCompleted OnImageCompleted;

        private bool waitingForImageRequest = false;
        public int Index
        {
            get;
            private set;
        }

        internal bool IsActive;
        
        private Material clonedMaterial;
        private IAvatarService avatarService;

        public Material Material
        {
            get
            {
                if (clonedMaterial == null)
                {
                    clonedMaterial = Instantiate(photo.material);
                    clonedMaterial.name = gameObject.name;
                    photo.material = clonedMaterial;
                }

                return clonedMaterial;
            }
        }

        public void FixUnityStencilBug()
        {
            // https://forum.unity.com/threads/masked-ui-elements-shader-not-updating.371542/
            MaskUtilities.NotifyStencilStateChanged(mask);
        }

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

        public delegate void Selected(Persona persona, int childIndex);
        public static event Selected OnSelected;
        private void OnSelectClicked()
        {
            OnSelected?.Invoke(Persona, Index);
        }

        public void Refresh(Texture2D texture, Persona persona)
        {
            avatarService = EmergenceServices.GetService<IAvatarService>();
            
            Persona = persona;
            Index = transform.GetSiblingIndex();
            nameText.transform.parent.gameObject.SetActive(false);
            nameText.text = persona.name;

            if (persona.AvatarImage == null)
            {
                persona.AvatarImage = texture;
            }

            photo.texture = persona.AvatarImage;

            var personaService = EmergenceServices.GetService<IPersonaService>();
            if (personaService.GetCurrentPersona(out var currentPersona))
            {
                var isCurrentPersona = Persona.id == currentPersona.id;
                unselectedBorder.SetActive(!isCurrentPersona);
                selectedBorder.SetActive(isCurrentPersona);
            }

            if (!string.IsNullOrEmpty(persona.avatarId))
            {
                avatarService.AvatarById(persona.avatarId, avatar =>
                {
                    
                    // Add fetched avatar to persona
                    Persona.avatar = avatar;
                    waitingForImageRequest = true;

                    if (!RequestImage.Instance.AskForImage(avatar.meta?.content?.First()?.url))
                    {
                        waitingForImageRequest = false;
                        OnImageCompleted?.Invoke(persona, false);
                    }
                }, EmergenceLogger.LogError);
            }
            else
            {
                OnImageCompleted?.Invoke(persona, false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            nameText.transform.parent.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            nameText.transform.parent.gameObject.SetActive(false);
        }

        private void Instance_OnImageReady(string url, Texture2D texture)
        {
            if (waitingForImageRequest && url == Persona.avatar.meta.content.First().url)
            {
                Persona.AvatarImage = texture;
                photo.texture = Persona.AvatarImage;
                waitingForImageRequest = false;
                OnImageCompleted?.Invoke(Persona, true);
            }
        }
    }
}