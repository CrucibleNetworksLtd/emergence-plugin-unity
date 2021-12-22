using System;
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
        public Button deleteButton;

        public TMP_InputField nameIF;
        public TMP_InputField bioIF;
        public RawImage personaAvatar;

        public Toggle availableOnSearchesToggle;
        public Toggle showingMyStatusToggle;
        public Toggle receiveContactRequestsToggle;

        public Toggle useThisPersonaAsDefaultToggle;

        public static EditPersonaScreen Instance;

        public Texture2D defaultImage;

        private Persona currentPersona;

        private Dictionary<string, Texture2D> avatarsCache = new Dictionary<string, Texture2D>();

        private void Awake()
        {
            Instance = this;
            createButton.onClick.AddListener(OnCreateClicked);
            deleteButton.onClick.AddListener(OnDeleteClicked);
            backButton.onClick.AddListener(OnBackClicked);
            AvatarScrollItem.OnAvatarSelected += AvatarScrollItem_OnAvatarSelected;
        }

        private void OnDestroy()
        {
            AvatarScrollItem.OnAvatarSelected -= AvatarScrollItem_OnAvatarSelected;
        }

        private string currentAvatarId = string.Empty;
        private void AvatarScrollItem_OnAvatarSelected(string id)
        {
            personaAvatar.texture = avatarsCache[id];
            currentAvatarId = id;
        }

        public void Refresh(Persona persona, bool isDefault, bool isNew = false)
        {
            currentPersona = persona;

            nameIF.text = persona.name;
            bioIF.text = persona.bio;

            availableOnSearchesToggle.SetIsOnWithoutNotify(persona.settings.availableOnSearch);
            showingMyStatusToggle.SetIsOnWithoutNotify(persona.settings.showStatus);
            receiveContactRequestsToggle.SetIsOnWithoutNotify(persona.settings.receiveContactRequest);

            useThisPersonaAsDefaultToggle.SetIsOnWithoutNotify(isDefault);
            useThisPersonaAsDefaultToggle.interactable = !isNew;

            if (persona.AvatarImage)
            {
                personaAvatar.texture = persona.AvatarImage;
            }
            else
            {
                personaAvatar.texture = defaultImage;
            }

            // Clear scroll area
            while (avatarScrollRoot.childCount > 0)
            {
                GameObject child = avatarScrollRoot.GetChild(0).gameObject;
                avatarScrollItemsPool.ReturnUsedObject(child);
            }

            NetworkManager.Instance.GetAvatars((avatars) =>
            {
                avatarsCache.Clear();

                for (int i = 0; i < avatars.Count; i++)
                {
                    GameObject go = avatarScrollItemsPool.GetNewObject();
                    go.transform.SetParent(avatarScrollRoot);
                    go.transform.localScale = Vector3.one;

                    AvatarScrollItem asi = go.GetComponent<AvatarScrollItem>();

                    Persona.Avatar avatar = avatars[i];

                    asi.Refresh(defaultImage, string.Empty, String.Empty);

                    RequestImage.Instance.AskForImage(avatar.url, (url, imageTexture2D) =>
                    {
                        asi.Refresh(imageTexture2D, avatar.id, avatar.url);

                        avatarsCache.Add(avatar.id, imageTexture2D);
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

        private void OnDeleteClicked()
        {
            // TODO delete persona
            ModalPromptYESNO.Instance.Show("Delete " + currentPersona.name, "are you sure?", () => {
                NetworkManager.Instance.DeletePersona(currentPersona, () =>
                {
                    //exit
                    Debug.Log("Deleting Persona");
                    EmergenceManager.Instance.ShowDashboard();
                },
                (error, code) =>
                {
                    Debug.LogError("[" + code + "] " + error);
                });
            });
        }
        private void OnCreateClicked()
        {
           
            //currentPersona.id = "";

            currentPersona.name = nameIF.text;
            currentPersona.bio = bioIF.text;
            //newPersona.settings = new Persona.PersonaSettings();
            currentPersona.settings.availableOnSearch = availableOnSearchesToggle.isOn;
            currentPersona.settings.receiveContactRequest = receiveContactRequestsToggle.isOn;
            currentPersona.settings.showStatus = showingMyStatusToggle.isOn;
            currentPersona.avatar.id = currentAvatarId;
            currentPersona.avatar.url = "";//currentAvatarURL;

            NetworkManager.Instance.SavePersona(currentPersona, () =>
            {
                //exit
                Debug.Log("Saving Persona");
                EmergenceManager.Instance.ShowDashboard();
            },
            (error, code) =>
            {
                Debug.LogError("[" + code + "] " + error);
            });

        }

        private void OnBackClicked()
        {
            EmergenceManager.Instance.ShowDashboard();
        }
    }
}
