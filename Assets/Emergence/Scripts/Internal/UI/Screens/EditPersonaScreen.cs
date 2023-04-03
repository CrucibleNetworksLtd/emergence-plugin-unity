using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Avatar = EmergenceSDK.Types.Avatar;

namespace EmergenceSDK.Internal.UI.Screens
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
        public Button deleteButton;

        [Header("UI References Edit / Create")]
        public GameObject[] editGOs;
        public GameObject[] createGOs;

        public Texture2D defaultImage;

        private Persona currentPersona;

        private HashSet<string> imagesRefreshing = new HashSet<string>();
        private bool requestingInProgress = false;

        private IPersonaService personaService;
        private IAvatarService avatarService;

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

        private void Update()
        {
            switch (state)
            {
                case States.CreateAvatar:
                    nextButton.interactable = true;
                    break;
                case States.CreateInformation:
                    nextButton.interactable = nameIF.text.Length >= 3;
                    break;
                case States.EditInformation:
                    nextButton.interactable = nameIF.text.Length >= 3;
                    break;
                case States.EditAvatar:
                    nextButton.interactable = true;
                    break;
            }
        }

        private Avatar currentAvatar = null;
        private Avatar existingAvatar;
        private void AvatarScrollItem_OnAvatarSelected(Avatar avatar)
        {
            currentAvatar = avatar;
            // Debug.Log("Avatar selected: " + avatar.meta.name);

            if (currentAvatar == null)
            {
                personaAvatar.texture = defaultImage;
                personaAvatarBackground.texture = defaultImage;
                return;
            }

            // Image at this point is already cached
            // Assuming image is the first in the list - TODO: need to check for MimeType
            RequestImage.Instance.AskForImage(avatar.meta.content.First().url, (url, texture) =>
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
            personaService = EmergenceServices.GetService<IPersonaService>();
            avatarService = EmergenceServices.GetService<IAvatarService>();
            
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

            nextButtonText.text = isNew ? "Persona Information" : "Save Changes";
            backButtonText.text = isNew ? "Back" : "Cancel";
            deleteButton.gameObject.SetActive(!isNew && !isDefault);

            currentPersona = persona;
            currentAvatar = currentPersona.avatar;
            existingAvatar = persona.avatar;
            nameIF.text = persona.name;
            bioIF.text = persona.bio;

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

            avatarService.AvatarsByOwner(EmergenceSingleton.Instance.GetCachedAddress(), (avatars) =>
            {
                Modal.Instance.Show("Retrieving avatar images...");
                requestingInProgress = true;
                imagesRefreshing.Clear();
                for (int i = 0; i < avatars.Count; i++)
                {
                    go = avatarScrollItemsPool.GetNewObject();
                    go.transform.SetParent(avatarScrollRoot);
                    go.transform.localScale = Vector3.one;

                    imagesRefreshing.Add(avatars[i].avatarId);
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
                personaService.DeletePersona(currentPersona, () =>
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

            switch (state)
            {
                case States.CreateAvatar:
                    backButtonText.text = "Select Avatar";
                    nextButtonText.text = "Create Persona";
                    currentPersona.avatar = currentAvatar;
                    panelAvatar.SetActive(false);
                    panelInformation.SetActive(true);
                    state = States.CreateInformation;
                    break;
                case States.CreateInformation:
                    ModalPromptYESNO.Instance.Show("", "Are you sure?", () =>
                    {
                        Modal.Instance.Show("Saving Changes...");

                        personaService.CreatePersona(currentPersona, () =>
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

                        personaService.EditPersona(currentPersona, () =>
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
                            ModalPromptOK.Instance.Show("Error editing persona");
                        });
                    });
                    break;
                case States.EditAvatar:
                    currentPersona.avatar = currentAvatar;
                    panelAvatar.SetActive(false);
                    panelInformation.SetActive(true);
                    replaceAvatarButton.gameObject.SetActive(true);
                    backButtonText.text = "Back";
                    nextButtonText.text = "Save Changes";
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
                    backButtonText.text = "Back";
                    nextButtonText.text = "Persona Information";
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
                    backButtonText.text = "Cancel";
                    nextButtonText.text = "Save Changes";
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
            nextButtonText.text = "Confirm Avatar";
            panelAvatar.SetActive(true);
            panelInformation.SetActive(false);
            state = States.EditAvatar;
        }

        private void ClearCurrentPersona()
        {
            currentAvatar = null;
        }

        private void AvatarScrollItem_OnImageCompleted(Avatar avatar, bool success)
        {
            if (!success)
            {
                if (avatar != null && imagesRefreshing.Contains(avatar.avatarId))
                {
                    imagesRefreshing.Remove(avatar.avatarId);
                    if (imagesRefreshing.Count <= 1 && !requestingInProgress)
                    {
                        Modal.Instance.Hide();
                    }
                }
            }
            else
            {
                if (imagesRefreshing.Contains(avatar.avatarId))
                {
                    imagesRefreshing.Remove(avatar.avatarId);
                    if (imagesRefreshing.Count <= 1 && !requestingInProgress)
                    {
                        Modal.Instance.Hide();
                    }
                }
                else if (imagesRefreshing.Count > 0)
                {
                    Debug.LogWarning("Image completed but not accounted for: [" + avatar.avatarId + "][" + avatar.meta.content.First().url + "][" + success + "]");
                }
            }
        }
    }
}
