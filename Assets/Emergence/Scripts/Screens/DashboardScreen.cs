using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Emergence
{
    public class DashboardScreen : MonoBehaviour
    {
        [Header("UI References")]
        public Transform personaScrollContents;
        public Button addPersonaButton;
        public Texture2D defaultTexture;

        [Header("Utilities")]
        [SerializeField]
        private Pool personaButtonPool;

        private Persona currentPersona;

        public static DashboardScreen Instance;

        public TextMeshProUGUI titleText;
        public TextMeshProUGUI contentsText;

        private HashSet<string> imagesRefreshing = new HashSet<string>();
        private bool requestingInProgress = false;

        private void Awake()
        {
            Instance = this;
            addPersonaButton.onClick.AddListener(OnCreatePersona);

            PersonaScrollItem.OnSelected += PersonaScrollItem_OnSelected;
            PersonaScrollItem.OnUsePersonaAsCurrent += PersonaScrollItem_OnUsePersonaAsCurrent;
            PersonaScrollItem.OnImageCompleted += PersonaScrollItem_OnImageCompleted;
        }

        private void OnDestroy()
        {
            PersonaScrollItem.OnSelected -= PersonaScrollItem_OnSelected;
            PersonaScrollItem.OnUsePersonaAsCurrent -= PersonaScrollItem_OnUsePersonaAsCurrent;
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

            NetworkManager.Instance.GetPersonas((personas, currentPersona) =>
            {
                Modal.Instance.Show("Retrieving avatar images...");

                this.currentPersona = currentPersona;

                requestingInProgress = true;
                imagesRefreshing.Clear();
                for (int i = 0; i < personas.Count; i++)
                {
                    GameObject go = personaButtonPool.GetNewObject();

                    go.transform.SetParent(personaScrollContents);
                    go.transform.localScale = Vector3.one;

                    PersonaScrollItem psi = go.GetComponent<PersonaScrollItem>();

                    Persona persona = personas[i];
                    bool selected = false;
                    if (currentPersona != null)
                    {
                        selected = currentPersona.id.Equals(persona.id);
                    }

                    imagesRefreshing.Add(persona.id);
                    psi.Refresh(defaultTexture, persona, selected);

                    if (selected)
                    {
                        go.transform.SetAsFirstSibling();
                    }
                }

                if (personas.Count > 0)
                {
                    titleText.text = "Which persona are you going to use?";
                    contentsText.text = "You can use it as you want to interact with friends and games.";
                }
                else
                {
                    titleText.text = "Hello,";
                    contentsText.text = "Your wallet has been connected successfully. \n Now, you will neeed to create a persona so you can interact with your friends.";
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
            EmergenceManager.Instance.ShowEditPersona();
        }

        private void PersonaScrollItem_OnSelected(Persona persona)
        {
            EditPersonaScreen.Instance.Refresh(persona, currentPersona.id == persona.id);
            EmergenceManager.Instance.ShowEditPersona();
        }
        private void PersonaScrollItem_OnUsePersonaAsCurrent(Persona persona)
        {
            Modal.Instance.Show("Loading Personas...");
            NetworkManager.Instance.SetCurrentPersona(persona, () =>
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
    }
}
