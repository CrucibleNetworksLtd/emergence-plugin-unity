using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK
{
    public class EditPersonaScreen : MonoBehaviour
    {
        public static EditPersonaScreen Instance;

        [Header("UI References Footer")]
        public GameObject panelInformation;
        public GameObject panelAvatar;
        public Button backButton;
        public Button nextButton;
        public TextMeshProUGUI nextButtonText;
        public TextMeshProUGUI backButtonText;

        public Button replaceAvatarButton;
        public RawImage personaAvatarBackground;
        public RawImage personaAvatar;

        [Header("UI References Avatar Panel")]
        public Transform avatarScrollRoot;
        public Pool avatarScrollItemsPool;

        [Header("UI References Information Panel")]
        public TMP_InputField nameIF;
        public TMP_InputField bioIF;
        public Toggle availableOnSearchesToggle;
        public Toggle showingMyStatusToggle;
        public Toggle receiveContactRequestsToggle;
        public Button deleteButton;

        [Header("UI References Edit / Create")]
        public GameObject[] editGOs;
        public GameObject[] createGOs;

        public Texture2D defaultImage;

        private Persona currentPersona;

        private HashSet<string> imagesRefreshing = new HashSet<string>();
        private bool requestingInProgress = false;

        private enum States
        {
            CreateAvatar,
            CreateInformation,
            EditInformation,
            EditAvatar,
        }

        private States state;

        private void Awake()
        {
            Instance = this;
            nextButton.onClick.AddListener(OnNextClicked);
            backButton.onClick.AddListener(OnBackClicked);
            deleteButton.onClick.AddListener(OnDeleteClicked);
            replaceAvatarButton.onClick.AddListener(OnReplaceAvatarClicked);
            AvatarScrollItem.OnAvatarSelected += AvatarScrollItem_OnAvatarSelected;
            AvatarScrollItem.OnImageCompleted += AvatarScrollItem_OnImageCompleted;
        }

        private void OnDestroy()
        {
            nextButton.onClick.RemoveListener(OnNextClicked);
            backButton.onClick.RemoveListener(OnBackClicked);
            deleteButton.onClick.RemoveListener(OnDeleteClicked);
            replaceAvatarButton.onClick.RemoveListener(OnReplaceAvatarClicked);
            AvatarScrollItem.OnAvatarSelected -= AvatarScrollItem_OnAvatarSelected;
            AvatarScrollItem.OnImageCompleted -= AvatarScrollItem_OnImageCompleted;
        }

        private Persona.Avatar currentAvatar = null;
        private Persona.Avatar existingAvatar;
        private void AvatarScrollItem_OnAvatarSelected(Persona.Avatar avatar)
        {
            currentAvatar = avatar;

            if (currentAvatar == null)
            {
                personaAvatar.texture = defaultImage;
                personaAvatarBackground.texture = defaultImage;
                return;
            }

            // Image at this point is already cached
            RequestImage.Instance.AskForImage(avatar.url, (url, texture) =>
            {
                personaAvatar.texture = texture;
                personaAvatarBackground.texture = texture;
            },
            (url, error, errorCode) =>
            {
                Debug.LogError("[" + url + "] " + error + " " + errorCode);
            });
        }

        public void Refresh(Persona persona, bool isDefault, bool isNew = false)
        {
            // Redesigned flow state
            state = isNew ? States.CreateAvatar : States.EditInformation;

            for (int i = 0; i < createGOs.Length; i++)
            {
                createGOs[i].SetActive(isNew);
            }

            for (int i = 0; i < editGOs.Length; i++)
            {
                editGOs[i].SetActive(!isNew);
            }


            // If creating, first show avatar selection
            panelAvatar.SetActive(isNew);
            panelInformation.SetActive(!isNew);

            nextButtonText.text = isNew ? "Next" : "Save";
            deleteButton.gameObject.SetActive(!isNew && !isDefault);

            currentPersona = persona;
            currentAvatar = currentPersona.avatar;
            existingAvatar = persona.avatar;
            nameIF.text = persona.name;
            bioIF.text = persona.bio;


            availableOnSearchesToggle.SetIsOnWithoutNotify(persona.settings.availableOnSearch);
            showingMyStatusToggle.SetIsOnWithoutNotify(persona.settings.showStatus);
            receiveContactRequestsToggle.SetIsOnWithoutNotify(persona.settings.receiveContactRequest);

            if (persona.AvatarImage)
            {
                personaAvatar.texture = persona.AvatarImage;
                personaAvatarBackground.texture = persona.AvatarImage;
            }
            else
            {
                personaAvatar.texture = defaultImage;
                personaAvatarBackground.texture = defaultImage;
            }

            // Clear scroll area
            while (avatarScrollRoot.childCount > 0)
            {
                GameObject child = avatarScrollRoot.GetChild(0).gameObject;
                avatarScrollItemsPool.ReturnUsedObject(child);
            }

            Modal.Instance.Show("Retrieving avatar data...");

            // Default avatar
            GameObject go = avatarScrollItemsPool.GetNewObject();
            go.transform.SetParent(avatarScrollRoot);
            go.transform.localScale = Vector3.one;

            go.GetComponent<AvatarScrollItem>().Refresh(defaultImage, null);

            Services.Instance.GetAvatars((avatars) =>
            {
                Modal.Instance.Show("Retrieving avatar images...");
                requestingInProgress = true;
                imagesRefreshing.Clear();
                for (int i = 0; i < avatars.Count; i++)
                {
                    go = avatarScrollItemsPool.GetNewObject();
                    go.transform.SetParent(avatarScrollRoot);
                    go.transform.localScale = Vector3.one;

                    imagesRefreshing.Add(avatars[i].id);
                    go.GetComponent<AvatarScrollItem>().Refresh(defaultImage, avatars[i]);
                }
                requestingInProgress = false;
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
                Modal.Instance.Show("Deleting Persona...");
                Services.Instance.DeletePersona(currentPersona, () =>
                {
                    Debug.Log("Deleting Persona");
                    Modal.Instance.Hide();
                    ScreenManager.Instance.ShowDashboard();
                },
                (error, code) =>
                {
                    Debug.LogError("[" + code + "] " + error);
                    Modal.Instance.Hide();
                });
            });
        }

        private void OnNextClicked()
        {
            switch (state)
            {
                case States.CreateInformation:
                case States.EditInformation:
                    if (string.IsNullOrEmpty(nameIF.text))
                    {
                        return;
                    }
                    break;
            }

            currentPersona.name = nameIF.text;
            currentPersona.bio = bioIF.text;
            currentPersona.settings.availableOnSearch = availableOnSearchesToggle.isOn;
            currentPersona.settings.receiveContactRequest = receiveContactRequestsToggle.isOn;
            currentPersona.settings.showStatus = showingMyStatusToggle.isOn;

            switch (state)
            {
                case States.CreateAvatar:
                    currentPersona.avatar = currentAvatar;
                    panelAvatar.SetActive(false);
                    panelInformation.SetActive(true);
                    state = States.CreateInformation;
                    break;
                case States.CreateInformation:
                    ModalPromptYESNO.Instance.Show("", "Are you sure?", () =>
                    {
                        Modal.Instance.Show("Saving Changes...");

                        Services.Instance.CreatePersona(currentPersona, () =>
                        {
                            Debug.Log("New Persona saved");
                            Modal.Instance.Hide();
                            Debug.Log(currentPersona);
                            ClearCurrentPersona();
                            ScreenManager.Instance.ShowDashboard();
                        },
                        (error, code) =>
                        {
                            Debug.LogError("[" + code + "] " + error);
                            Modal.Instance.Hide();
                            ModalPromptOK.Instance.Show("Error creating persona");
                        });
                    });
                    break;
                case States.EditInformation:
                    ModalPromptYESNO.Instance.Show("", "Are you sure?", () =>
                    {
                        Modal.Instance.Show("Saving Changes...");

                        Services.Instance.EditPersona(currentPersona, () =>
                        {
                            Debug.Log("Changes to Persona saved");
                            Modal.Instance.Hide();
                            ClearCurrentPersona();
                            ScreenManager.Instance.ShowDashboard();
                        },
                        (error, code) =>
                        {
                            Debug.LogError("[" + code + "] " + error);
                            Modal.Instance.Hide();
                            ModalPromptOK.Instance.Show("Error creating persona");
                        });
                    });
                    break;
                case States.EditAvatar:
                    currentPersona.avatar = currentAvatar;
                    panelAvatar.SetActive(false);
                    panelInformation.SetActive(true);
                    replaceAvatarButton.gameObject.SetActive(true);
                    backButtonText.text = "Back";
                    nextButtonText.text = "Save";
                    state = States.EditInformation;
                    break;
            }
        }

        private void OnBackClicked()
        {
            switch (state)
            {
                case States.CreateAvatar:
                    ClearCurrentPersona();
                    ScreenManager.Instance.ShowDashboard();
                    break;
                case States.CreateInformation:
                    panelAvatar.SetActive(true);
                    panelInformation.SetActive(false);
                    state = States.CreateAvatar;
                    break;
                case States.EditInformation:
                    ClearCurrentPersona();
                    ScreenManager.Instance.ShowDashboard();
                    break;
                case States.EditAvatar:
                    currentPersona.avatar = existingAvatar;
                    AvatarScrollItem_OnAvatarSelected(existingAvatar);
                    replaceAvatarButton.gameObject.SetActive(true);
                    backButtonText.text = "Back";
                    nextButtonText.text = "Save";
                    panelAvatar.SetActive(false);
                    panelInformation.SetActive(true);
                    state = States.EditInformation;
                    break;
            }
        }

        private void OnReplaceAvatarClicked()
        {
            replaceAvatarButton.gameObject.SetActive(false);
            backButtonText.text = "Cancel";
            nextButtonText.text = "Change";
            panelAvatar.SetActive(true);
            panelInformation.SetActive(false);
            state = States.EditAvatar;
        }

        private void OnReplaceAvatarClicked()
        {

        }

        private void ClearCurrentPersona()
        {
            currentAvatar = null;
        }

        /*
        private void OnUseThisPersonaAsDefaultToggled(bool isOn)
        {
            Modal.Instance.Show("Saving Changes...");
            Services.Instance.SetCurrentPersona(currentPersona, () =>
            {
                Debug.Log("Successfully SetCurrentPersona to " + currentPersona.name);
                deleteButton.gameObject.SetActive(false);
                deleteTooltip.gameObject.SetActive(false);
                Modal.Instance.Hide();
            },
            (error, code) =>
            {
                Debug.LogError("[" + code + "] " + error);
                Modal.Instance.Hide();
            });
        }*/

        private void AvatarScrollItem_OnImageCompleted(Persona.Avatar avatar, bool success)
        {
            if (!success)
            {
                if (avatar != null && imagesRefreshing.Contains(avatar.id))
                {
                    imagesRefreshing.Remove(avatar.id);
                    if (imagesRefreshing.Count <= 1 && !requestingInProgress)
                    {
                        Modal.Instance.Hide();
                    }
                }
            }
            else
            {
                if (imagesRefreshing.Contains(avatar.id))
                {
                    imagesRefreshing.Remove(avatar.id);
                    if (imagesRefreshing.Count <= 1 && !requestingInProgress)
                    {
                        Modal.Instance.Hide();
                    }
                }
                else if (imagesRefreshing.Count > 0)
                {
                    Debug.LogWarning("Image completed but not accounted for: [" + avatar.id + "][" + avatar.url + "][" + success + "]");
                }
            }
        }
    }
}
