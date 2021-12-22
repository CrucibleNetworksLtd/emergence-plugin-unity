using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Emergence
{
    public class DashboardScreen : MonoBehaviour
    {
        [Header("UI References")]
        public Transform personaScrollContents;
        public Button addPersonaButton;

        [Header("Utilities")]
        [SerializeField]
        private Pool personaButtonPool;

        private Persona currentPersona;

        public static DashboardScreen Instance;

        public TextMeshProUGUI titleText;
        public TextMeshProUGUI contentsText;

        private int numberOfPersonas = 0;

        private void Awake()
        {
            Instance = this;
            addPersonaButton.onClick.AddListener(OnCreatePersona);

            PersonaScrollItem.OnSelected += PersonaScrollItem_OnSelected;
            PersonaScrollItem.OnUsePersonaAsCurrent += PersonaScrollItem_OnUsePersonaAsCurrent;
        }

        private void OnDestroy()
        {
            PersonaScrollItem.OnSelected -= PersonaScrollItem_OnSelected;
            PersonaScrollItem.OnUsePersonaAsCurrent -= PersonaScrollItem_OnUsePersonaAsCurrent;
        }

        public void Refresh()
        {

            titleText.text = "Hello,";
            contentsText.text = "Your wallet has been connected successfully. \n Now, you will neeed to create a persona so you can interact with your friends.";

            Modal.Instance.Show("Loading Personas...");
            HeaderScreen.Instance.Show();
            while (personaScrollContents.childCount > 0)
            {
                personaButtonPool.ReturnUsedObject(personaScrollContents.GetChild(0).gameObject);
            }

            NetworkManager.Instance.GetPersonas((personas, currentPersona) =>
            {
                this.currentPersona = currentPersona;

                for (int i = 0; i < personas.Count; i++)
                {
                    GameObject go = personaButtonPool.GetNewObject();

                    go.transform.SetParent(personaScrollContents);
                    go.transform.localScale = Vector3.one; // Sometimes unity breaks the size when reparenting

                    // Fill with persona info
                    PersonaScrollItem psi = go.GetComponent<PersonaScrollItem>();

                    Persona persona = personas[i];
                    bool selected = currentPersona.id.Equals(persona.id);
                    psi.Refresh(persona, selected);

                    if (selected)
                    {
                        go.transform.SetAsFirstSibling();
                    }
                }

                numberOfPersonas = personas.Count;

                if (personas.Count>0)
                {
                    titleText.text = "Which persona are you going to use?";
                    contentsText.text = "You can use it as you want to interact with friends and games.";
                }
                Modal.Instance.Hide();

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
            NetworkManager.Instance.SetCurrentPersona(persona, () =>
            {
                Refresh();
            },
            (error, code) =>
            {
                Debug.LogError("[" + code + "] " + error);
            });
        }
    }
}
