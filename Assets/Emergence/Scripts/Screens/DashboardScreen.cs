using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace EmergenceSDK
{
    public class DashboardScreen : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject personasScrollPanel;
        public Transform personaScrollContents;
        public Button addPersonaButton;

        [Header("UI Sidebar References")]
        // Sidebar New persona for when the user has no personas
        public Button addPersonaSidebarButton1;

        // Sidebar Avatar and New persona for when the user has at least one persona
        public Button activePersonaButton;
        public Button addPersonaSidebarButton2;
        public RawImage sidebarAvatar;
        public GameObject sidebarWithPersonas;

        [Header("UI Detail Panel References")]
        public GameObject detailsPanel;
        public TextMeshProUGUI detailsTitleText;
        public TextMeshProUGUI detailsBioText;
        public Button detailsActivateButton;
        public TextMeshProUGUI detailsActivateButtonText;
        public Button detailsEditButton;
        public Button detailsDeleteButton;

        [Header("Default Texture")]
        public Texture2D defaultTexture;


        [Header("Utilities")]
        [SerializeField]
        private Pool personaButtonPool;

        private Persona activePersona;
        private Persona selectedPersona;
        private List<Persona> personasList = new List<Persona>();

        public static DashboardScreen Instance;

        private HashSet<string> imagesRefreshing = new HashSet<string>();
        private bool requestingInProgress = false;

        private void Awake()
        {
            Instance = this;
            addPersonaButton.onClick.AddListener(OnCreatePersona);
            addPersonaSidebarButton1.onClick.AddListener(OnCreatePersona);
            addPersonaSidebarButton2.onClick.AddListener(OnCreatePersona);
            activePersonaButton.onClick.AddListener(OnActivePersonaClicked);
            detailsActivateButton.onClick.AddListener(OnUsePersonaAsCurrent);
            detailsEditButton.onClick.AddListener(OnEditPersona);
            detailsDeleteButton.onClick.AddListener(OnDeletePersona);
            PersonaScrollItem.OnSelected += PersonaScrollItem_OnSelected;
            PersonaScrollItem.OnImageCompleted += PersonaScrollItem_OnImageCompleted;
            PersonaCarousel.OnArrowClicked += PersonaCarousel_OnArrowClicked;
            sidebarAvatar.texture = defaultTexture;
            detailsPanel.SetActive(false);

            UIForNoPersonas();
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

        private void OnDestroy()
        {
            addPersonaButton.onClick.RemoveListener(OnCreatePersona);
            addPersonaSidebarButton1.onClick.RemoveListener(OnCreatePersona);
            addPersonaSidebarButton2.onClick.RemoveListener(OnCreatePersona);
            activePersonaButton.onClick.RemoveListener(OnActivePersonaClicked);
            detailsActivateButton.onClick.RemoveListener(OnUsePersonaAsCurrent);
            detailsEditButton.onClick.RemoveListener(OnEditPersona);
            detailsDeleteButton.onClick.RemoveListener(OnDeletePersona);
            PersonaScrollItem.OnSelected -= PersonaScrollItem_OnSelected;
            PersonaScrollItem.OnImageCompleted -= PersonaScrollItem_OnImageCompleted;
        }

        public void Refresh()
        {
            detailsPanel.SetActive(false);
            HeaderScreen.Instance.Show();
            while (personaScrollContents.childCount > 0)
            {
                personaButtonPool.ReturnUsedObject(personaScrollContents.GetChild(0).gameObject);
            }

            Modal.Instance.Show("Loading Personas...");

            Services.Instance.GetPersonas((personas, currentPersona) =>
            {
                Modal.Instance.Show("Retrieving avatar images...");

                this.activePersona = currentPersona;

                requestingInProgress = true;
                imagesRefreshing.Clear();

                List<PersonaScrollItem> scrollItems = new List<PersonaScrollItem>();
                int selectedIndex = 0;
                for (int i = 0; i < personas.Count; i++)
                {
                    GameObject go = personaButtonPool.GetNewObject();

                    go.transform.SetParent(personaScrollContents);
                    go.transform.localScale = Vector3.one;

                    scrollItems.Add(personaScrollContents.GetChild(i).GetComponent<PersonaScrollItem>());

                    Persona persona = personas[i];
                    if (persona.avatar != null)
                    {
                        if (string.IsNullOrEmpty(persona.avatar.id))
                        {
                            persona.avatar = null;
                        }
                        else if (string.IsNullOrEmpty(persona.avatar.url))
                        {
                            persona.avatar = null;
                        }
                    }

                    bool selected = false;
                    if (currentPersona != null)
                    {
                        selected = currentPersona.id.Equals(persona.id);

                        if (selected)
                        {
                            selectedIndex = i;
                        }
                    }
                }

                scrollItems[selectedIndex].transform.SetSiblingIndex(Mathf.FloorToInt(personas.Count / 2));
                for (int i = 0; i < personas.Count; i++)
                {
                    Persona persona = personas[i];
                    imagesRefreshing.Add(persona.id);
                    scrollItems[i].Refresh(defaultTexture, persona, i == selectedIndex);
                }

                personasList.Clear();
                for (int i = 0; i < personaScrollContents.childCount; i++)
                {
                    personasList.Add(personaScrollContents.GetChild(i).GetComponent<PersonaScrollItem>().Persona);
                }

                PersonaCarousel.Instance.Refresh(Mathf.FloorToInt(personas.Count / 2));

                if (personas.Count > 0)
                {
                    UIForPersonas();
                }
                else
                {
                    UIForNoPersonas();
                }

                PersonaScrollItem_OnSelected(activePersona, -1);

                requestingInProgress = false;

                // In case images were already cached
                if (imagesRefreshing.Count <= 0)
                {
                    Modal.Instance.Hide();
                }
            },
            (error, code) =>
            {
                Debug.LogError("[" + code + "] " + error);
                Modal.Instance.Hide();
            });
        }

        private void OnCreatePersona()
        {
            Persona persona = new Persona()
            {
                id = string.Empty,
                name = string.Empty,
                bio = string.Empty,
                avatar = new Persona.Avatar()
                {
                    id = string.Empty,
                    url = string.Empty,
                },
                settings = new Persona.PersonaSettings()
                {
                    availableOnSearch = true,
                    receiveContactRequest = true,
                    showStatus = true,
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
            // TODO put information on screen instead of edit
            detailsPanel.SetActive(true);

            selectedPersona = persona;

            bool active = persona.id == activePersona.id;

            detailsTitleText.text = persona.name;
            detailsBioText.text = persona.bio;
            detailsDeleteButton.interactable = !active;
            detailsActivateButton.interactable = !active;
            detailsActivateButtonText.text = active ? "ACTIVE" : "ACTIVATE";
        }

        private void OnUsePersonaAsCurrent()
        {
            Modal.Instance.Show("Loading Personas...");
            Services.Instance.SetCurrentPersona(selectedPersona, () =>
            {
                Refresh();
            },
            (error, code) =>
            {
                Debug.LogError("[" + code + "] " + error);
                Modal.Instance.Hide();
            });
        }

        private void OnEditPersona()
        {
            EditPersonaScreen.Instance.Refresh(selectedPersona, activePersona.id == selectedPersona.id);
            ScreenManager.Instance.ShowEditPersona();
        }

        private void OnDeletePersona()
        {
            ModalPromptYESNO.Instance.Show("Delete " + selectedPersona.name, "are you sure?", () =>
            {
                Modal.Instance.Show("Deleting Persona...");
                Services.Instance.DeletePersona(selectedPersona, () =>
                {
                    Debug.Log("Deleting Persona");
                    Modal.Instance.Hide();
                    Refresh();
                },
                (error, code) =>
                {
                    Debug.LogError("[" + code + "] " + error);
                    Modal.Instance.Hide();
                });
            });
        }

        private void PersonaScrollItem_OnImageCompleted(Persona persona, bool success)
        {
            if (activePersona != null)
            {
                if (activePersona.id.Equals(persona.id))
                {
                    sidebarAvatar.texture = persona.AvatarImage;
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
            else if (imagesRefreshing.Count > 0)
            {
                Debug.LogWarning("Image completed but not accounted for: [" + persona.id + "][" + persona.avatar.id + "][" + persona.avatar.url + "][" + success + "]");
            }
        }

        private void UIForNoPersonas()
        {
            addPersonaButton.transform.parent.gameObject.SetActive(true);
            addPersonaSidebarButton1.gameObject.SetActive(true);
            sidebarWithPersonas.SetActive(false);
            personasScrollPanel.SetActive(false);
        }

        private void UIForPersonas()
        {
            addPersonaButton.transform.parent.gameObject.SetActive(false);
            addPersonaSidebarButton1.gameObject.SetActive(false);
            sidebarWithPersonas.SetActive(true);
            personasScrollPanel.SetActive(true);
        }
    }
}
