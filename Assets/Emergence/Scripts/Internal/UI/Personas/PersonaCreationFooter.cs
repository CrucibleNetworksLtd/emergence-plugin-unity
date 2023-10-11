using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI.Personas
{
    public class PersonaCreationFooter : MonoBehaviour
    {
        [SerializeField]
        private GameObject PanelInformation;
        [SerializeField]
        private GameObject PanelAvatar;
        [SerializeField]
        private Button BackButton;
        [SerializeField]
        private Button NextButton;
        [SerializeField]
        private TextMeshProUGUI NextButtonText;
        [SerializeField]
        private TextMeshProUGUI BackButtonText;
        
        public Button.ButtonClickedEvent OnNextClicked => NextButton.onClick;
        public Button.ButtonClickedEvent OnBackClicked => BackButton.onClick;
        
        public void SetNextButtonInteractable(bool interactable) => NextButton.interactable = interactable;
        
        public void SetNextButtonText(string text) => NextButtonText.text = text;
        public void SetBackButtonText(string text) => BackButtonText.text = text;
        
        /// <remarks>The PanelAvatar object's active property will always be set to the opposite of active</remarks>
        public void TogglePanelInformation(bool active)
        {
            PanelInformation.SetActive(active);
            PanelAvatar.SetActive(!active);
        }
    }
}