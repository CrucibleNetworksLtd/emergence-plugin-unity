using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Internal.UI.Screens;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Avatar = EmergenceSDK.Types.Avatar;

namespace EmergenceSDK.Internal.UI.Personas
{
    public class EditPersonaScreen : MonoBehaviour
    {
        public static EditPersonaScreen Instance;
        public Texture2D DefaultImage;

        [Header("UI References Footer")]
        public GameObject PanelInformation;
        public GameObject PanelAvatar;
        public Button BackButton;
        public Button NextButton;
        public TextMeshProUGUI NextButtonText;
        public TextMeshProUGUI BackButtonText;

        public Button ReplaceAvatarButton;
        public RawImage PersonaAvatarBackground;
        public RawImage PersonaAvatar;

        [Header("UI References Avatar Panel")]
        public Transform AvatarScrollRoot;
        public Pool AvatarScrollItemsPool;

        [FormerlySerializedAs("nameIF")] 
        public TMP_InputField NameInputField;
        public TMP_InputField BioIf;
        public Button DeleteButton;

        [Header("UI References Edit / Create")]
        public GameObject[] EditGOs;
        public GameObject[] CreateGOs;
        
        private Avatar currentAvatar = null;
        private Avatar existingAvatar;
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

        private States State
        {
            get => state;
            set
            {
                OnStateUpdated();
                state = value;
            }
        }
        private States state;

        private void Awake()
        {
            Instance = this;
            NextButton.onClick.AddListener(OnNextClicked);
            BackButton.onClick.AddListener(OnBackClicked);
            DeleteButton.onClick.AddListener(OnDeleteClicked);
            ReplaceAvatarButton.onClick.AddListener(OnReplaceAvatarClicked);
            AvatarScrollItem.OnAvatarSelected += AvatarScrollItem_OnAvatarSelected;
            AvatarScrollItem.OnImageCompleted += AvatarScrollItem_OnImageCompleted;
        }

        private void OnDestroy()
        {
            NextButton.onClick.RemoveListener(OnNextClicked);
            BackButton.onClick.RemoveListener(OnBackClicked);
            DeleteButton.onClick.RemoveListener(OnDeleteClicked);
            ReplaceAvatarButton.onClick.RemoveListener(OnReplaceAvatarClicked);
            AvatarScrollItem.OnAvatarSelected -= AvatarScrollItem_OnAvatarSelected;
            AvatarScrollItem.OnImageCompleted -= AvatarScrollItem_OnImageCompleted;
        }

        private void OnStateUpdated()
        {
            switch (State)
            {
                case States.CreateAvatar:
                    NextButton.interactable = true;
                    break;
                case States.CreateInformation:
                    NextButton.interactable = NameInputField.text.Length >= 3;
                    break;
                case States.EditInformation:
                    NextButton.interactable = NameInputField.text.Length >= 3;
                    break;
                case States.EditAvatar:
                    NextButton.interactable = true;
                    break;
            }
        }

        private void AvatarScrollItem_OnAvatarSelected(Avatar avatar)
        {
            currentAvatar = avatar;

            if (currentAvatar == null)
            {
                PersonaAvatar.texture = DefaultImage;
                PersonaAvatarBackground.texture = DefaultImage;
                return;
            }

            // Image at this point is already cached
            // Assuming image is the first in the list - TODO: need to check for MimeType
            RequestImage.Instance.AskForImage(avatar.meta.content.First().url, (url, texture) =>
            {
                PersonaAvatar.texture = texture;
                PersonaAvatarBackground.texture = texture;
            },
            (url, error, errorCode) =>
            {
                EmergenceLogger.LogError("[" + url + "] " + error + " " + errorCode);
            });
        }

        public void Refresh(Persona persona, bool isDefault, bool isNew = false)
        {
            personaService = EmergenceServices.GetService<IPersonaService>();
            avatarService = EmergenceServices.GetService<IAvatarService>();
            
            // Redesigned flow State
            State = isNew ? States.CreateAvatar : States.EditInformation;

            for (int i = 0; i < CreateGOs.Length; i++)
            {
                CreateGOs[i].SetActive(isNew);
            }

            for (int i = 0; i < EditGOs.Length; i++)
            {
                EditGOs[i].SetActive(!isNew);
            }


            // If creating, first show avatar selection
            PanelAvatar.SetActive(isNew);
            PanelInformation.SetActive(!isNew);

            NextButtonText.text = isNew ? "Persona Information" : "Save Changes";
            BackButtonText.text = isNew ? "Back" : "Cancel";
            DeleteButton.gameObject.SetActive(!isNew && !isDefault);

            currentPersona = persona;
            currentAvatar = currentPersona.avatar;
            existingAvatar = persona.avatar;
            NameInputField.text = persona.name;
            BioIf.text = persona.bio;

            if (persona.AvatarImage)
            {
                PersonaAvatar.texture = persona.AvatarImage;
                PersonaAvatarBackground.texture = persona.AvatarImage;
            }
            else
            {
                PersonaAvatar.texture = DefaultImage;
                PersonaAvatarBackground.texture = DefaultImage;
            }

            // Clear scroll area
            while (AvatarScrollRoot.childCount > 0)
            {
                GameObject child = AvatarScrollRoot.GetChild(0).gameObject;
                AvatarScrollItemsPool.ReturnUsedObject(child);
            }

            Modal.Instance.Show("Retrieving avatar data...");

            // Default avatar
            GameObject go = AvatarScrollItemsPool.GetNewObject();
            go.transform.SetParent(AvatarScrollRoot);
            go.transform.localScale = Vector3.one;

            go.GetComponent<AvatarScrollItem>().Refresh(DefaultImage, null);

            avatarService.AvatarsByOwner(EmergenceSingleton.Instance.GetCachedAddress(), (avatars) =>
            {
                Modal.Instance.Show("Retrieving avatar images...");
                requestingInProgress = true;
                imagesRefreshing.Clear();
                for (int i = 0; i < avatars.Count; i++)
                {
                    go = AvatarScrollItemsPool.GetNewObject();
                    go.transform.SetParent(AvatarScrollRoot);
                    go.transform.localScale = Vector3.one;

                    imagesRefreshing.Add(avatars[i].avatarId);
                    go.GetComponent<AvatarScrollItem>().Refresh(DefaultImage, avatars[i]);
                }
                requestingInProgress = false;
                if (imagesRefreshing.Count <= 0)
                {
                    Modal.Instance.Hide();
                }
            },
            (error, code) =>
            {
                EmergenceLogger.LogError(error, code);
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
                    EmergenceLogger.LogInfo("Deleting Persona");
                    Modal.Instance.Hide();
                    ScreenManager.Instance.ShowDashboard();
                },
                (error, code) =>
                {
                    EmergenceLogger.LogError(error, code);
                    Modal.Instance.Hide();
                });
            });
        }

        private void OnNextClicked()
        {
            switch (State)
            {
                case States.CreateInformation:
                case States.EditInformation:
                    if (string.IsNullOrEmpty(NameInputField.text))
                    {
                        return;
                    }
                    break;
            }

            currentPersona.name = NameInputField.text;
            currentPersona.bio = BioIf.text;

            switch (State)
            {
                case States.CreateAvatar:
                    BackButtonText.text = "Select Avatar";
                    NextButtonText.text = "Create Persona";
                    currentPersona.avatar = currentAvatar;
                    PanelAvatar.SetActive(false);
                    PanelInformation.SetActive(true);
                    State = States.CreateInformation;
                    break;
                case States.CreateInformation:
                    ModalPromptYESNO.Instance.Show("", "Are you sure?", () =>
                    {
                        Modal.Instance.Show("Saving Changes...");

                        personaService.CreatePersona(currentPersona, () =>
                        {
                            EmergenceLogger.LogInfo("New Persona saved");
                            Modal.Instance.Hide();
                            EmergenceLogger.LogInfo(currentPersona.ToString());
                            ClearCurrentPersona();
                            ScreenManager.Instance.ShowDashboard();
                        },
                        (error, code) =>
                        {
                            EmergenceLogger.LogError(error, code);
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
                            EmergenceLogger.LogInfo("Changes to Persona saved");
                            Modal.Instance.Hide();
                            ClearCurrentPersona();
                            ScreenManager.Instance.ShowDashboard();
                        },
                        (error, code) =>
                        {
                            EmergenceLogger.LogError(error, code);
                            Modal.Instance.Hide();
                            ModalPromptOK.Instance.Show("Error editing persona");
                        });
                    });
                    break;
                case States.EditAvatar:
                    currentPersona.avatar = currentAvatar;
                    PanelAvatar.SetActive(false);
                    PanelInformation.SetActive(true);
                    ReplaceAvatarButton.gameObject.SetActive(true);
                    BackButtonText.text = "Back";
                    NextButtonText.text = "Save Changes";
                    State = States.EditInformation;
                    break;
            }
        }
        
        private void OnBackClicked()
        {
            switch (State)
            {
                case States.CreateAvatar:
                    ClearCurrentPersona();
                    ScreenManager.Instance.ShowDashboard();
                    break;
                case States.CreateInformation:
                    BackButtonText.text = "Back";
                    NextButtonText.text = "Persona Information";
                    PanelAvatar.SetActive(true);
                    PanelInformation.SetActive(false);
                    State = States.CreateAvatar;
                    break;
                case States.EditInformation:
                    ClearCurrentPersona();
                    ScreenManager.Instance.ShowDashboard();
                    break;
                case States.EditAvatar:
                    currentPersona.avatar = existingAvatar;
                    AvatarScrollItem_OnAvatarSelected(existingAvatar);
                    ReplaceAvatarButton.gameObject.SetActive(true);
                    BackButtonText.text = "Cancel";
                    NextButtonText.text = "Save Changes";
                    PanelAvatar.SetActive(false);
                    PanelInformation.SetActive(true);
                    State = States.EditInformation;
                    break;
            }
        }

        private void OnReplaceAvatarClicked()
        {
            ReplaceAvatarButton.gameObject.SetActive(false);
            BackButtonText.text = "Cancel";
            NextButtonText.text = "Confirm Avatar";
            PanelAvatar.SetActive(true);
            PanelInformation.SetActive(false);
            State = States.EditAvatar;
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
                    EmergenceLogger.LogWarning("Image completed but not accounted for: [" + avatar.avatarId + "][" + avatar.meta.content.First().url + "][" + success + "]");
                }
            }
        }
    }
}
