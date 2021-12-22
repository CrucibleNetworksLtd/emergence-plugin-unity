using UnityEngine;
using UnityEngine.UI;

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
            while(personaScrollContents.childCount > 0)
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

                    // Loading images
                    RequestImage.Instance.AskForImage(persona.avatar.url, (url, imageTexture2D) =>
                    {//key is null
                        persona.AvatarImage = imageTexture2D;
                        psi.Refresh(persona, selected);
                    },
                    (url, error, errorCode) =>
                    {
                        Debug.LogError("[" + url + "] " + error + " " + errorCode);
                    });
                }
            },
            (error, code) =>
            {
                Debug.LogError("[" + code + "] " + error);
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
