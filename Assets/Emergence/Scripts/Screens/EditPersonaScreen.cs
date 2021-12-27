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

        private HashSet<string> imagesRefreshing = new HashSet<string>();
        private bool requestingInProgress = false;

        private void Awake()
        {
            Instance = this;
            saveButton.onClick.AddListener(OnSaveClicked);
            deleteButton.onClick.AddListener(OnDeleteClicked);
            backButton.onClick.AddListener(OnBackClicked);
            useThisPersonaAsDefaultToggle.onValueChanged.AddListener(OnUseThisPersonaAsDefaultToggled);
            AvatarScrollItem.OnAvatarSelected += AvatarScrollItem_OnAvatarSelected;
            AvatarScrollItem.OnImageCompleted += AvatarScrollItem_OnImageCompleted;
        }

        private void OnDestroy()
        {
            AvatarScrollItem.OnAvatarSelected -= AvatarScrollItem_OnAvatarSelected;
            AvatarScrollItem.OnImageCompleted -= AvatarScrollItem_OnImageCompleted;
        }

        private string currentAvatarId = string.Empty;
        private void AvatarScrollItem_OnAvatarSelected(Persona.Avatar avatar)
        {
            // Image at this point is already cached
            RequestImage.Instance.AskForImage(avatar.url, (url, texture) =>
            {
                personaAvatar.texture = texture;
            }, 
            (uri, error, errorCode) =>
            {
            });

            currentAvatarId = avatar.id;
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

            Modal.Instance.Show("Retrieving avatar data...");

            NetworkManager.Instance.GetAvatars((avatars) =>
            {
                Modal.Instance.Show("Retrieving avatar images...");
                requestingInProgress = true;
                imagesRefreshing.Clear();
                for (int i = 0; i < avatars.Count; i++)
                {
                    GameObject go = avatarScrollItemsPool.GetNewObject();
                    go.transform.SetParent(avatarScrollRoot);
                    go.transform.localScale = Vector3.one;

                    imagesRefreshing.Add(avatars[i].id);
                    go.GetComponent<AvatarScrollItem>().Refresh(defaultImage, avatars[i]);
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

        private void AvatarScrollItem_OnImageCompleted(Persona.Avatar avatar, bool success)
        {
            if (imagesRefreshing.Contains(avatar.id))
            {
                imagesRefreshing.Remove(avatar.id);
            }
            else if (imagesRefreshing.Count > 0)
            {
                Debug.LogWarning("Image completed but not accounted for: [" + avatar.id + "][" + avatar.url + "][" + success + "]");
            }
                
            if (imagesRefreshing.Count <= 0 && !requestingInProgress)
            {
                Modal.Instance.Hide();
            }
        }
    }
}
