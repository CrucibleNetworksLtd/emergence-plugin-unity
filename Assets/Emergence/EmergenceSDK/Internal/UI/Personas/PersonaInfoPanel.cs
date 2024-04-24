using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI.Personas
{
    public class PersonaInfoPanel : MonoBehaviour 
    {
        [SerializeField]
        private TMP_InputField NameInputField;
        [SerializeField]
        private TMP_InputField BioIf;
        [SerializeField]
        private Button DeleteButton;
        
        public Button.ButtonClickedEvent OnDeleteClicked => DeleteButton.onClick;
        
        public void SetDeleteButtonActive(bool active) => DeleteButton.gameObject.SetActive(active);
        
        public string PersonaName
        {
            get => NameInputField.text;
            set => NameInputField.text = value;
        }

        public string PersonaBio
        {
            get => BioIf.text;
            set => BioIf.text = value;
        }
    }
}