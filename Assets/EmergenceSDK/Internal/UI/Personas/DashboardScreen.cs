using System;
using EmergenceSDK.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI.Personas
{
    public class DashboardScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private GameObject PersonasScrollPanel;
        [SerializeField]
        private Button AddPersonaButton;

        [Header("UI Sidebar References")]
        [SerializeField]
        private Button AddPersonaSidebarButton1;
        // Sidebar Avatar and New persona for when the user has at least one persona
        [SerializeField]
        private Button ActivePersonaButton;
        [SerializeField]
        private Button AddPersonaSidebarButton2;
        [SerializeField]
        private RawImage SidebarAvatar;
        [SerializeField]
        private GameObject SidebarWithPersonas;

        [Header("UI Detail Panel References")]
        [SerializeField]
        private GameObject DetailsPanel;
        [SerializeField]
        private TextMeshProUGUI DetailsTitleText;
        [SerializeField]
        private TextMeshProUGUI DetailsBioText;
        [SerializeField]
        private Button DetailsActivateButton;
        [SerializeField]
        private TextMeshProUGUI DetailsActivateButtonText;
        [SerializeField]
        private Button DetailsEditButton;
        [SerializeField]
        private Button DetailsDeleteButton;

        [Header("Default Texture")]
        [SerializeField]
        private Texture2D DefaultTexture;
        
        public Action CreatePersonaClicked;
        public Action ActivePersonaClicked;
        public Action UsePersonaAsCurrentClicked;
        public Action EditPersonaClicked;
        public Action DeletePersonaClicked;

        private void Awake()
        {
            BindUI();
            SidebarAvatar.texture = DefaultTexture;
            DetailsPanel.SetActive(false);
        }
        
        private void OnCreatePersona() => CreatePersonaClicked?.Invoke();
        private void OnActivePersonaClicked() => ActivePersonaClicked?.Invoke();
        private void OnUsePersonaAsCurrent() => UsePersonaAsCurrentClicked?.Invoke();
        private void OnEditPersona() => EditPersonaClicked?.Invoke();
        private void OnDeletePersona() => DeletePersonaClicked?.Invoke();
        private void BindUI()
        {
            AddPersonaButton.onClick.AddListener(OnCreatePersona);
            AddPersonaSidebarButton1.onClick.AddListener(OnCreatePersona);
            AddPersonaSidebarButton2.onClick.AddListener(OnCreatePersona);
            ActivePersonaButton.onClick.AddListener(OnActivePersonaClicked);
            DetailsActivateButton.onClick.AddListener(OnUsePersonaAsCurrent);
            DetailsEditButton.onClick.AddListener(OnEditPersona);
            DetailsDeleteButton.onClick.AddListener(OnDeletePersona);
        }

        private void UnBindUI()
        {
            AddPersonaButton.onClick.RemoveListener(OnCreatePersona);
            AddPersonaSidebarButton1.onClick.RemoveListener(OnCreatePersona);
            AddPersonaSidebarButton2.onClick.RemoveListener(OnCreatePersona);
            ActivePersonaButton.onClick.RemoveListener(OnActivePersonaClicked);
            DetailsActivateButton.onClick.RemoveListener(OnUsePersonaAsCurrent);
            DetailsEditButton.onClick.RemoveListener(OnEditPersona);
            DetailsDeleteButton.onClick.RemoveListener(OnDeletePersona);
        }
        
        private void OnDestroy()
        {
            UnBindUI();
        }
        
        internal void ShowUI(bool personaListEmpty)
        {
            AddPersonaButton.transform.parent.gameObject.SetActive(personaListEmpty);
            AddPersonaSidebarButton1.gameObject.SetActive(personaListEmpty);
            SidebarWithPersonas.SetActive(!personaListEmpty);
            PersonasScrollPanel.SetActive(!personaListEmpty);
        }

        internal void HideUI()
        {
            AddPersonaButton.transform.parent.gameObject.SetActive(false);
            AddPersonaSidebarButton1.gameObject.SetActive(false);
            SidebarWithPersonas.SetActive(false);
            PersonasScrollPanel.SetActive(false);
        }

        private void Start()
        {
            HideUI();
        }
        
        internal void ShowDetailsPanel(Persona persona, bool active)
        {
            DetailsPanel.SetActive(true);
            DetailsTitleText.text = persona.name;
            DetailsBioText.text = persona.bio;
            DetailsDeleteButton.interactable = !active;
            DetailsActivateButton.interactable = !active;
            DetailsActivateButtonText.text = active ? "ACTIVE" : "ACTIVATE";
        }
        
        internal void HideDetailsPanel()
        {
            DetailsPanel.SetActive(false);
        }
        
        internal void ShowAvatar(Texture2D texture)
        {
            SidebarAvatar.texture = texture;
        }
    }
}
