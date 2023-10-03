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
        // Fields and Properties
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
        [SerializeField]
        private TextMeshProUGUI debugText;

        public Persona Persona { get; private set; }
        public int Index { get; private set; }
        internal bool IsActive;

        private bool waitingForImageRequest;
        private Material clonedMaterial;
        private IAvatarService avatarService;

        public delegate void ImageCompleted(Persona persona, bool success);
        public static event ImageCompleted OnImageCompleted;

        public delegate void Selected(Persona persona, int childIndex);
        public static event Selected OnSelected;

        // Unity Lifecycle Methods
        private void Awake()
        {
            RegisterEventListeners();
        }

        private void OnDestroy()
        {
            UnregisterEventListeners();
        }

        // Public Methods
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

        public void RecalculateMasking()
        {
            MaskUtilities.NotifyStencilStateChanged(mask);
        }

        public void Refresh(Texture2D texture, Persona persona)
        {
            InitialiseServicesAndProperties(texture, persona);
            UpdateUI(texture, persona);
            FetchAvatar(persona);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ToggleNameTextVisibility(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToggleNameTextVisibility(false);
        }

        public void DisplayDebugText(string text)
        {
            debugText.text = text;
        }
        
        // Private Methods
        private void RegisterEventListeners()
        {
            selectButton.onClick.AddListener(OnSelectClicked);
            RequestImage.Instance.OnImageReady += Instance_OnImageReady;
        }

        private void UnregisterEventListeners()
        {
            selectButton.onClick.RemoveListener(OnSelectClicked);
            RequestImage.Instance.OnImageReady -= Instance_OnImageReady;
        }

        private void OnSelectClicked()
        {
            OnSelected?.Invoke(Persona, Index);
        }

        private void InitialiseServicesAndProperties(Texture2D texture, Persona persona)
        {
            avatarService = EmergenceServices.GetService<IAvatarService>();
            Persona = persona;
            Index = transform.GetSiblingIndex();
        }

        private void UpdateUI(Texture2D texture, Persona persona)
        {
            nameText.transform.parent.gameObject.SetActive(false);
            nameText.text = persona.name;
            photo.texture = persona.AvatarImage ? persona.AvatarImage : texture;
            UpdateBorder();
        }

        private void UpdateBorder()
        {
            var personaService = EmergenceServices.GetService<IPersonaService>();
            if (personaService.GetCurrentPersona(out var currentPersona))
            {
                var isCurrentPersona = Persona.id == currentPersona.id;
                unselectedBorder.SetActive(!isCurrentPersona);
                selectedBorder.SetActive(isCurrentPersona);
            }
        }

        private void FetchAvatar(Persona persona)
        {
            if (!string.IsNullOrEmpty(persona.avatarId))
            {
                RequestAvatar(persona);
            }
            else
            {
                OnImageCompleted?.Invoke(persona, false);
            }
        }

        private void RequestAvatar(Persona persona)
        {
            avatarService.AvatarById(persona.avatarId, avatar =>
            {
                Persona.avatar = avatar;
                waitingForImageRequest = true;

                if (!RequestImage.Instance.AskForImage(avatar.meta?.content?.First()?.url))
                {
                    waitingForImageRequest = false;
                    OnImageCompleted?.Invoke(persona, false);
                }
            }, EmergenceLogger.LogError);
        }

        private void ToggleNameTextVisibility(bool isVisible)
        {
            nameText.transform.parent.gameObject.SetActive(isVisible);
        }

        private void Instance_OnImageReady(string url, Texture2D texture)
        {
            if (IsImageRequestMatching(url))
            {
                UpdateAvatarImage(texture);
            }
        }

        private bool IsImageRequestMatching(string url)
        {
            return waitingForImageRequest && url == Persona.avatar.meta.content.First().url;
        }

        private void UpdateAvatarImage(Texture2D texture)
        {
            Persona.AvatarImage = texture;
            photo.texture = Persona.AvatarImage;
            waitingForImageRequest = false;
            OnImageCompleted?.Invoke(Persona, true);
        }
    }
}
