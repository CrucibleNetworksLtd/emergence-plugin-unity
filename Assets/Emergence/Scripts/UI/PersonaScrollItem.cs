using System.Linq;
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

        private Material clonedMaterial;
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
            OnSelected?.Invoke(Persona, Index);
        }

        public void Refresh(Texture2D texture, Persona persona, bool selected)
        {
            this.Persona = persona;
            Index = this.transform.GetSiblingIndex();
            nameText.transform.parent.gameObject.SetActive(false);
            nameText.text = persona.name;

            if (persona.AvatarImage == null)
            {
                persona.AvatarImage = texture;
            }

            photo.texture = persona.AvatarImage;

            unselectedBorder.SetActive(!selected);
            selectedBorder.SetActive(selected);

            if (!string.IsNullOrEmpty(persona.avatarId))
            {
                Services.Instance.AvatarById(persona.avatarId, (avatar =>
                {
                    // trigger swapping of the avatar
                    Services.Instance.SwapAvatars(avatar.meta.content[1].url, () =>
                    {
                        // Add fetched avatar to persona
                        Persona.avatar = avatar;
                        waitingForImageRequest = true;

                        if (!RequestImage.Instance.AskForImage(avatar.meta.content.First().url))
                        {
                            waitingForImageRequest = false;
                            OnImageCompleted?.Invoke(persona, false);
                        }
                    }, (message, code) => {});
                    
                }), (message, code) =>
                {
                    Debug.LogError("Error fetching Avatar by id: " + message);
                });
            }
            else
            {
                OnImageCompleted?.Invoke(persona, false);
            }

            // if (persona.avatar != null && persona.avatar.meta.content.First().url != null)
            // {
            //     waitingForImageRequest = true;
            //     if (!RequestImage.Instance.AskForImage(persona.avatar.meta.content.First().url))
            //     {
            //         waitingForImageRequest = false;
            //         OnImageCompleted?.Invoke(persona, false);
            //     }
            // }
            // else
            // {
            //     OnImageCompleted?.Invoke(persona, false);
            // }
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

        private void Instance_OnImageFailed(string url, string error, long errorCode)
        {
            if (waitingForImageRequest && url == Persona.avatar.meta.content.First().url)
            {
                waitingForImageRequest = false;
                Debug.LogError("[" + url + "] [" + errorCode + "] " + error);
                OnImageCompleted?.Invoke(Persona, false);
            }
        }
    }
}