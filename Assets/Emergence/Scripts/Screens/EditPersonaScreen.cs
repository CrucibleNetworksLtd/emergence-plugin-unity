using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Emergence
{
    public class EditPersonaScreen : MonoBehaviour
    {
        public static EditPersonaScreen Instance;

        [Header("UI References")]
        public Button backButton;
        public Pool avatarScrollItemsPool;
        public Transform avatarScrollRoot;
        public TextMeshProUGUI title;
        public TextMeshProUGUI welcomeText;
        public Button saveButton;
        public TextMeshProUGUI saveButtonText;
        public Button deleteButton;
        public GameObject deleteTooltip;

        public TMP_InputField nameIF;
        public TMP_InputField bioIF;
        public RawImage personaAvatar;

        public Toggle availableOnSearchesToggle;
        public Toggle showingMyStatusToggle;
        public Toggle receiveContactRequestsToggle;
        public Toggle useThisPersonaAsDefaultToggle;

        public Texture2D defaultImage;

        private Persona currentPersona;
        private int avatarCounter = 0;
        private Dictionary<string, Texture2D> avatarsCache = new Dictionary<string, Texture2D>();

        private void Awake()
        {
            Instance = this;
            saveButton.onClick.AddListener(OnSaveClicked);
            deleteButton.onClick.AddListener(OnDeleteClicked);
            backButton.onClick.AddListener(OnBackClicked);
            useThisPersonaAsDefaultToggle.onValueChanged.AddListener(OnUseThisPersonaAsDefaultToggled);
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
            saveButtonText.text = isNew ? "Create" : "Save";

            title.gameObject.SetActive(isNew);
            welcomeText.gameObject.SetActive(isNew);

            deleteButton.gameObject.SetActive(!isNew);
            deleteTooltip.gameObject.SetActive(!isNew);
            useThisPersonaAsDefaultToggle.interactable = !isNew && !isDefault;

            currentPersona = persona;

            nameIF.text = persona.name;
            bioIF.text = persona.bio;

            availableOnSearchesToggle.SetIsOnWithoutNotify(persona.settings.availableOnSearch);
            showingMyStatusToggle.SetIsOnWithoutNotify(persona.settings.showStatus);
            receiveContactRequestsToggle.SetIsOnWithoutNotify(persona.settings.receiveContactRequest);

            useThisPersonaAsDefaultToggle.SetIsOnWithoutNotify(isDefault);

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
            avatarCounter = 0;
            Modal.Instance.Show("Retrieving avatar data...");
            NetworkManager.Instance.GetAvatars((avatars) =>
            {
                avatarsCache.Clear();

                bool requesting = true;
                for (int i = 0; i < avatars.Count; i++)
                {
                    GameObject go = avatarScrollItemsPool.GetNewObject();
                    go.transform.SetParent(avatarScrollRoot);
                    go.transform.localScale = Vector3.one;

                    AvatarScrollItem asi = go.GetComponent<AvatarScrollItem>();
                    Persona.Avatar avatar = avatars[i];
                    asi.Refresh(defaultImage, string.Empty, String.Empty);

                    avatarCounter++;

                    RequestImage.Instance.AskForImage(avatar.url, (url, imageTexture2D) =>
                    {
                        asi.Refresh(imageTexture2D, avatar.id, avatar.url);

                        avatarsCache.Add(avatar.id, imageTexture2D);

                        // If this is the last image returned, close the modal
                        avatarCounter--;
                        if (avatarCounter <= 0 && !requesting)
                        {
                            Modal.Instance.Hide();
                        }
                    },
                    (url, error, errorCode) =>
                    {
                        Debug.LogError("[" + url + "] " + error + " " + errorCode);

                        // If this is the last image returned, close the modal
                        avatarCounter--;
                        if (avatarCounter <= 0 && !requesting)
                        {
                            Modal.Instance.Hide();
                        }
                    });
                }
                requesting = false;

                // In case all images were readily returned due to caching
                if (avatarCounter <= 0)
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

        private void OnDeleteClicked()
        {
            ModalPromptYESNO.Instance.Show("Delete " + currentPersona.name, "are you sure?", () =>
            {
                NetworkManager.Instance.DeletePersona(currentPersona, () =>
                {
                    Debug.Log("Deleting Persona");
                    EmergenceManager.Instance.ShowDashboard();
                },
                (error, code) =>
                {
                    Debug.LogError("[" + code + "] " + error);
                });
            });
        }

        private void OnSaveClicked()
        {
            if (string.IsNullOrEmpty(nameIF.text))
            {
                return;
            }
            Modal.Instance.Show("Saving Changes...");

            currentPersona.name = nameIF.text;
            currentPersona.bio = bioIF.text;
            currentPersona.settings.availableOnSearch = availableOnSearchesToggle.isOn;
            currentPersona.settings.receiveContactRequest = receiveContactRequestsToggle.isOn;
            currentPersona.settings.showStatus = showingMyStatusToggle.isOn;
            currentPersona.avatar.id = currentAvatarId;

            if (string.IsNullOrEmpty(currentPersona.id))
            {
                NetworkManager.Instance.CreatePersona(currentPersona, () =>
                {
                    //exit
                    Debug.Log("Saving Persona");
                    Modal.Instance.Hide();
                    EmergenceManager.Instance.ShowDashboard();
                },
                (error, code) =>
                {
                    Debug.LogError("[" + code + "] " + error);
                    Modal.Instance.Hide();
                });
                return;
            }

            NetworkManager.Instance.EditPersona(currentPersona, () =>
            {
                Debug.Log("Saving Changes to Persona");
                Modal.Instance.Hide();
                EmergenceManager.Instance.ShowDashboard();
            },
            (error, code) =>
            {
                Debug.LogError("[" + code + "] " + error);
                Modal.Instance.Hide();
            });

        }

        private void OnBackClicked()
        {
            EmergenceManager.Instance.ShowDashboard();
        }

        private void OnUseThisPersonaAsDefaultToggled(bool isOn)
        {
            Modal.Instance.Show("Saving Changes...");
            NetworkManager.Instance.SetCurrentPersona(currentPersona, () =>
            {
                Debug.Log("Successfully SetCurrentPersona to " + currentPersona.name);
                useThisPersonaAsDefaultToggle.interactable = false;
                Modal.Instance.Hide();
            },
            (error, code) =>
            {
                Debug.LogError("[" + code + "] " + error);
                Modal.Instance.Hide();
            });
        }
    }
}
