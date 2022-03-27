using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
        public Button addPersonaSidebarButton2;
        public RawImage sidebarAvatar;
        public GameObject sidebarWithPersonas;

        [Header("UI Detail Panel References")]
        public GameObject detailsPanel;

        [Header("Default Texture")]
        public Texture2D defaultTexture;


        [Header("Utilities")]
        [SerializeField]
        private Pool personaButtonPool;

        private Persona currentPersona;

        public static DashboardScreen Instance;

        private HashSet<string> imagesRefreshing = new HashSet<string>();
        private bool requestingInProgress = false;

        private void Awake()
        {
            Instance = this;
            addPersonaButton.onClick.AddListener(OnCreatePersona);
            addPersonaSidebarButton1.onClick.AddListener(OnCreatePersona);
            addPersonaSidebarButton2.onClick.AddListener(OnCreatePersona);

            PersonaScrollItem.OnSelected += PersonaScrollItem_OnSelected;
            PersonaScrollItem.OnImageCompleted += PersonaScrollItem_OnImageCompleted;

            sidebarAvatar.texture = defaultTexture;
            detailsPanel.SetActive(false);

            UIForNoPersonas();
        }

        private void OnDestroy()
        {
            addPersonaButton.onClick.RemoveListener(OnCreatePersona);
            addPersonaSidebarButton1.onClick.RemoveListener(OnCreatePersona);
            addPersonaSidebarButton2.onClick.RemoveListener(OnCreatePersona);

            PersonaScrollItem.OnSelected -= PersonaScrollItem_OnSelected;
            PersonaScrollItem.OnImageCompleted -= PersonaScrollItem_OnImageCompleted;
        }

        public void Refresh()
        {
            HeaderScreen.Instance.Show();
            while (personaScrollContents.childCount > 0)
            {
                personaButtonPool.ReturnUsedObject(personaScrollContents.GetChild(0).gameObject);
            }

            Modal.Instance.Show("Loading Personas...");

            Services.Instance.GetPersonas((personas, currentPersona) =>
            {
                Modal.Instance.Show("Retrieving avatar images...");

                this.currentPersona = currentPersona;

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

                PersonaCarousel.Instance.Refresh(Mathf.FloorToInt(personas.Count / 2));

                if (personas.Count > 0)
                {
                    UIForPersonas();
                }
                else
                {
                    UIForNoPersonas();
                }

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
            // TODO put information on screen instead of edit
            return;
            EditPersonaScreen.Instance.Refresh(persona, currentPersona.id == persona.id);
            ScreenManager.Instance.ShowEditPersona();
        }

        private void PersonaScrollItem_OnUsePersonaAsCurrent(Persona persona)
        {
            Modal.Instance.Show("Loading Personas...");
            Services.Instance.SetCurrentPersona(persona, () =>
            {
                Refresh();
            },
            (error, code) =>
            {
                Debug.LogError("[" + code + "] " + error);
                Modal.Instance.Hide();
            });
        }

        private void PersonaScrollItem_OnImageCompleted(Persona persona, bool success)
        {
            if (currentPersona != null)
            {
                if (currentPersona.id.Equals(persona.id))
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
