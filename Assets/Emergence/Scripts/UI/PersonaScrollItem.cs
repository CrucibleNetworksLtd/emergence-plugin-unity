using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Emergence
{
    public class PersonaScrollItem : MonoBehaviour
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

        private void Awake()
        {
            selectButton.onClick.AddListener(OnSelectClicked);
        }

        public delegate void Selected(Persona persona);
        public static event Selected OnSelected;

        private void OnSelectClicked()
        {
            OnSelected?.Invoke(persona);
        }

        public void Refresh(Persona persona, bool selected)
        {
            this.persona = persona;

            nameText.text = persona.name;
            photo.texture = persona.AvatarImage;
            unselectedBorder.SetActive(!selected);
            selectedBorder.SetActive(selected);
        }
    }
}