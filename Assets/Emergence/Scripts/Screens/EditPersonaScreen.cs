using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Emergence
{
    public class EditPersonaScreen : MonoBehaviour
    {
        [Header("UI References")]
        public Button backButton;
        public Pool avatarScrollItemsPool;
        public Transform avatarScrollRoot;
        public TextMeshProUGUI title;
        public Button createButton;

        public TMP_InputField nameIF;
        public TMP_InputField bioIF;
        public RawImage personaAvatar;

        public Toggle availableOnSearchesToggle;
        public Toggle showingMyStatusToggle;
        public Toggle receiveContactRequestsToggle;

        private void Awake()
        {
            createButton.onClick.AddListener(OnCreateClicked);
            backButton.onClick.AddListener(OnBackClicked);
        }

        public void Refresh(Persona persona)
        {
            if (persona != null)
            {
                nameIF.text = persona.name;
                bioIF.text = persona.bio;
                
                availableOnSearchesToggle.SetIsOnWithoutNotify(persona.settings.availableOnSearch);
                showingMyStatusToggle.SetIsOnWithoutNotify(persona.settings.showStatus);
                receiveContactRequestsToggle.SetIsOnWithoutNotify(persona.settings.receiveContactRequest);

                if (persona.AvatarImage)
                {
                    personaAvatar.texture = persona.AvatarImage;
                }
                 else
                {
                    // TODO default image
                }
            }

            // Clear scroll area
            while (avatarScrollRoot.childCount > 0)
            {
                GameObject child = avatarScrollRoot.GetChild(0).gameObject;
                avatarScrollItemsPool.ReturnUsedObject(child);
            }

            // TODO all avatar images
            GameObject go = avatarScrollItemsPool.GetNewObject();
            go.transform.SetParent(avatarScrollRoot);
            go.transform.localScale = Vector3.one;
        }

        private void OnCreateClicked()
        {
            // TODO Save persona
        }

        private void OnBackClicked()
        {
            EmergenceManager.Instance.ShowDashboard();
        }
    }
}
