using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.UI.Screens;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Avatar = EmergenceSDK.Types.Avatar;

namespace EmergenceSDK.Internal.UI.Personas
{
    public class DashboardScreen : MonoBehaviour
    {
        [FormerlySerializedAs("personasScrollPanel")] [Header("UI References")]
        public GameObject PersonasScrollPanel;
        [FormerlySerializedAs("personaScrollContents")] public Transform PersonaScrollContents;
        [FormerlySerializedAs("addPersonaButton")] public Button AddPersonaButton;

        [FormerlySerializedAs("addPersonaSidebarButton1")] [Header("UI Sidebar References")]
        // Sidebar New persona for when the user has no personas
        public Button AddPersonaSidebarButton1;

        // Sidebar Avatar and New persona for when the user has at least one persona
        [FormerlySerializedAs("activePersonaButton")] public Button ActivePersonaButton;
        [FormerlySerializedAs("addPersonaSidebarButton2")] public Button AddPersonaSidebarButton2;
        [FormerlySerializedAs("sidebarAvatar")] public RawImage SidebarAvatar;
        [FormerlySerializedAs("sidebarWithPersonas")] public GameObject SidebarWithPersonas;

        [FormerlySerializedAs("detailsPanel")] [Header("UI Detail Panel References")]
        public GameObject DetailsPanel;
        [FormerlySerializedAs("detailsTitleText")] public TextMeshProUGUI DetailsTitleText;
        [FormerlySerializedAs("detailsBioText")] public TextMeshProUGUI DetailsBioText;
        [FormerlySerializedAs("detailsActivateButton")] public Button DetailsActivateButton;
        [FormerlySerializedAs("detailsActivateButtonText")] public TextMeshProUGUI DetailsActivateButtonText;
        [FormerlySerializedAs("detailsEditButton")] public Button DetailsEditButton;
        [FormerlySerializedAs("detailsDeleteButton")] public Button DetailsDeleteButton;

        [FormerlySerializedAs("defaultTexture")] [Header("Default Texture")]
        public Texture2D DefaultTexture;


        [FormerlySerializedAs("personaButtonPool")]
        [Header("Utilities")]
        [SerializeField]
        private Pool PersonaButtonPool;

        private Persona activePersona;
        private Persona selectedPersona;
        private int selectedPersonaIndex = 0;
        private List<Persona> personasList = new List<Persona>();

        public static DashboardScreen Instance;

        private HashSet<string> imagesRefreshing = new HashSet<string>();
        private bool requestingInProgress = false;

        private IPersonaService personaService;

        private void Awake()
        {
            Instance = this;
            AddPersonaButton.onClick.AddListener(OnCreatePersona);
            AddPersonaSidebarButton1.onClick.AddListener(OnCreatePersona);
            AddPersonaSidebarButton2.onClick.AddListener(OnCreatePersona);
            ActivePersonaButton.onClick.AddListener(OnActivePersonaClicked);
            DetailsActivateButton.onClick.AddListener(OnUsePersonaAsCurrent);
            DetailsEditButton.onClick.AddListener(OnEditPersona);
            DetailsDeleteButton.onClick.AddListener(OnDeletePersona);
            PersonaScrollItem.OnSelected += PersonaScrollItem_OnSelected;
            PersonaScrollItem.OnImageCompleted += PersonaScrollItem_OnImageCompleted;
            PersonaCarousel.OnArrowClicked += PersonaCarousel_OnArrowClicked;
            SidebarAvatar.texture = DefaultTexture;
            DetailsPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            AddPersonaButton.onClick.RemoveListener(OnCreatePersona);
            AddPersonaSidebarButton1.onClick.RemoveListener(OnCreatePersona);
            AddPersonaSidebarButton2.onClick.RemoveListener(OnCreatePersona);
            ActivePersonaButton.onClick.RemoveListener(OnActivePersonaClicked);
            DetailsActivateButton.onClick.RemoveListener(OnUsePersonaAsCurrent);
            DetailsEditButton.onClick.RemoveListener(OnEditPersona);
            DetailsDeleteButton.onClick.RemoveListener(OnDeletePersona);
            PersonaScrollItem.OnSelected -= PersonaScrollItem_OnSelected;
            PersonaScrollItem.OnImageCompleted -= PersonaScrollItem_OnImageCompleted;
        }

        private void PersonaCarousel_OnArrowClicked(int index)
        {
            selectedPersona = personasList[index];
            PersonaScrollItem_OnSelected(selectedPersona, -1);
        }

        private void OnActivePersonaClicked()
        {
            if (activePersona == null)
            {
                return;
            }

            PersonaScrollItem_OnSelected(activePersona, -1);
            PersonaCarousel.Instance.GoToActivePersona();
        }

        private void Start()
        {
            HideUI();
        }

        public async UniTask Refresh()
        {
            personaService = EmergenceServices.GetService<IPersonaService>();
            
            HideUI();
            DetailsPanel.SetActive(false);
            HeaderScreen.Instance.Show();
            while (PersonaScrollContents.childCount > 0)
            {
                PersonaButtonPool.ReturnUsedObject(PersonaScrollContents.GetChild(0).gameObject);
            }

            Modal.Instance.Show("Loading Personas...");

            
            var personas = await personaService.GetPersonasAsync();
            if (personas.Success)
            {
                GetPersonasSuccess(personas.Result0, personas.Result1);
            }
            else
            {
                ModalPromptOK.Instance.Show("Error retrieving personas");
            }
        }

        private void GetPersonasSuccess(List<Persona> personas, Persona currentPersona)
        {
            Modal.Instance.Show("Retrieving avatars...");

            try
            {
                activePersona = currentPersona;

                requestingInProgress = true;
                imagesRefreshing.Clear();

                selectedPersonaIndex = personas.IndexOf(currentPersona);
                selectedPersonaIndex = personas.FindIndex(p => p.id == currentPersona.id);
                personasList = personas;
            
                //Update scroll items
                List<PersonaScrollItem> scrollItems = CreateScrollItems(personas, currentPersona);
                if(scrollItems.Count > 0)
                    RefreshScrollItems(personas, scrollItems);

                PersonaCarousel.Instance.Refresh(Mathf.FloorToInt((float)personas.Count / 2));
                ShowUI();
                PersonaScrollItem_OnSelected(activePersona, -1);
                requestingInProgress = false;
            }
            finally
            {
                Modal.Instance.Hide();
            }
        }
        
        private List<PersonaScrollItem> CreateScrollItems(List<Persona> personas, Persona currentPersona)
        {
            List<PersonaScrollItem> scrollItems = new List<PersonaScrollItem>();
            selectedPersonaIndex = 0;

            for (int i = 0; i < personas.Count; i++)
            {
                // Creation and setup of scroll item
                GameObject go = PersonaButtonPool.GetNewObject();
                go.transform.SetParent(PersonaScrollContents);
                go.transform.localScale = Vector3.one;

                scrollItems.Add(PersonaScrollContents.GetChild(i).GetComponent<PersonaScrollItem>());

                Persona persona = personas[i];

                // Avatar validation logic
                if (persona.avatar != null)
                {
                    if (string.IsNullOrEmpty(persona.avatar.avatarId) || string.IsNullOrEmpty(persona.avatar.meta.content.First().url))
                    {
                        persona.avatar = null;
                    }
                }
            }

            return scrollItems;
        }

        private void RefreshScrollItems(List<Persona> personas, List<PersonaScrollItem> scrollItems)
        {
            scrollItems[selectedPersonaIndex].transform.SetSiblingIndex(Mathf.FloorToInt((float)scrollItems.Count / 2));
            for (int i = 0; i < personas.Count; i++)
            {
                Persona persona = personas[i];
                imagesRefreshing.Add(persona.id);
                scrollItems[i].Refresh(DefaultTexture, persona, i == selectedPersonaIndex);
            }
        }

        private void OnCreatePersona()
        {
            Persona persona = new Persona()
            {
                id = string.Empty,
                name = string.Empty,
                bio = string.Empty,
                avatar = new Avatar()
                {
                    avatarId = string.Empty,
                },
                AvatarImage = null,
            };

            EditPersonaScreen.Instance.Refresh(persona, true, true);
            ScreenManager.Instance.ShowEditPersona();
        }

        private void PersonaScrollItem_OnSelected(Persona persona, int position)
        {
            if (persona == null)
            {
                return;
            }

            DetailsPanel.SetActive(true);

            selectedPersona = persona;

            bool active = activePersona != null && persona.id == activePersona.id;

            DetailsTitleText.text = persona.name;
            DetailsBioText.text = persona.bio;
            DetailsDeleteButton.interactable = !active;
            DetailsActivateButton.interactable = !active;
            DetailsActivateButtonText.text = active ? "ACTIVE" : "ACTIVATE";
        }

        private async void OnUsePersonaAsCurrent()
        {
            Modal.Instance.Show("Loading Personas...");
            var setPersonaResponse = await personaService.SetCurrentPersonaAsync(selectedPersona);
            if (setPersonaResponse.Success)
            {
                Refresh().Forget();
            }
            Modal.Instance.Hide();
            if(!setPersonaResponse.Success)
            {
                ModalPromptOK.Instance.Show("Error setting current persona");
            }
        }

        private void OnEditPersona()
        {
            EditPersonaScreen.Instance.Refresh(selectedPersona, activePersona.id == selectedPersona.id);
            ScreenManager.Instance.ShowEditPersona();
        }

        private void OnDeletePersona() => ModalPromptYESNO.Instance.Show($"Deleting persona: {selectedPersona.name}", "Are you sure?", DeleteSelectedPersona);

        private async void DeleteSelectedPersona()
        {
            Modal.Instance.Show("Deleting Persona...");
            var personaServiceResult = await personaService.DeletePersonaAsync(selectedPersona);
            if (personaServiceResult.Success)
            {
                Refresh().Forget();
            }
            Modal.Instance.Hide();
        }

        private void PersonaScrollItem_OnImageCompleted(Persona persona, bool success)
        {
            if (activePersona != null)
            {
                if (activePersona.id.Equals(persona.id))
                {
                    SidebarAvatar.texture = persona.AvatarImage;
                }
            }

            if (imagesRefreshing.Contains(persona.id))
            {
                imagesRefreshing.Remove(persona.id);
                if (imagesRefreshing.Count <= 0 && !requestingInProgress)
                {
                    Modal.Instance.Hide();
                }
            }
        }

        private void ShowUI()
        {
            var personaListEmpty = personasList.Count == 0;
            AddPersonaButton.transform.parent.gameObject.SetActive(personaListEmpty);
            AddPersonaSidebarButton1.gameObject.SetActive(personaListEmpty);
            SidebarWithPersonas.SetActive(!personaListEmpty);
            PersonasScrollPanel.SetActive(!personaListEmpty);
        }

        private void HideUI()
        {
            AddPersonaButton.transform.parent.gameObject.SetActive(false);
            AddPersonaSidebarButton1.gameObject.SetActive(false);
            SidebarWithPersonas.SetActive(false);
            PersonasScrollPanel.SetActive(false);
        }
    }
}
