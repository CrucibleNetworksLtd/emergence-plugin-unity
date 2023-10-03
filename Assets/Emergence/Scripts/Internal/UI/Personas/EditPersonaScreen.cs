using System.Collections.Generic;
using EmergenceSDK.Internal.UI.Screens;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine;
using Avatar = EmergenceSDK.Types.Avatar;

namespace EmergenceSDK.Internal.UI.Personas
{
    public class EditPersonaScreen : MonoBehaviour
    {
        public static EditPersonaScreen Instance;
        public Texture2D DefaultImage;

        public PersonaCreationFooter Footer;
        public AvatarDisplayScreen AvatarDisplayScreen;        
        public PersonaInfoPanel PersonaInfo;
        public PersonaCreationEditingStatusWidget StatusWidget;
        
        private Avatar existingAvatar;
        private Persona currentPersona;
        private HashSet<string> imagesRefreshing = new HashSet<string>();
        private IPersonaService personaService;

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
            Footer.OnNextClicked.AddListener(OnNextClicked);
            Footer.OnBackClicked.AddListener(OnBackClicked);
            PersonaInfo.OnDeleteClicked.AddListener(OnDeleteClicked);
            AvatarDisplayScreen.OnReplaceAvatarClicked.AddListener(OnReplaceAvatarClicked);
        }

        private void OnDestroy()
        {
            Footer.OnNextClicked.RemoveListener(OnNextClicked);
            Footer.OnBackClicked.RemoveListener(OnBackClicked);
            PersonaInfo.OnDeleteClicked.RemoveListener(OnDeleteClicked);
            AvatarDisplayScreen.OnReplaceAvatarClicked.RemoveListener(OnReplaceAvatarClicked);
        }

        private void OnStateUpdated()
        {
            switch (State)
            {
                case States.CreateAvatar:
                    Footer.SetNextButtonInteractable(true);
                    break;
                case States.CreateInformation:
                    Footer.SetNextButtonInteractable(PersonaInfo.PersonaName.Length >= 3);
                    break;
                case States.EditInformation:
                    Footer.SetNextButtonInteractable(PersonaInfo.PersonaName.Length >= 3);
                    break;
                case States.EditAvatar:
                    Footer.SetNextButtonInteractable(true);
                    break;
            }
        }

        public void Refresh(Persona persona, bool isDefault, bool isNew = false)
        {
            personaService = EmergenceServices.GetService<IPersonaService>();
            
            // Redesigned flow State
            State = isNew ? States.CreateAvatar : States.EditInformation;

            StatusWidget.SetVisible(isNew);

            // If creating, first show avatar selection
            Footer.TogglePanelInformation(isNew);

            Footer.SetNextButtonText(isNew ? "Persona Information" : "Save Changes");
            Footer.SetBackButtonText(isNew ? "Back" : "Cancel");
            PersonaInfo.SetDeleteButtonActive(!isNew && !isDefault);

            currentPersona = persona;
            AvatarDisplayScreen.currentAvatar = currentPersona.avatar;
            existingAvatar = persona.avatar;
            PersonaInfo.PersonaName = persona.name;
            PersonaInfo.PersonaBio = persona.bio;
            
            AvatarDisplayScreen.RefreshAvatarDisplay(persona.AvatarImage ?? DefaultImage);
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
                    if (string.IsNullOrEmpty(PersonaInfo.PersonaName))
                    {
                        return;
                    }
                    break;
            }

            currentPersona.name = PersonaInfo.PersonaName;
            currentPersona.bio = PersonaInfo.PersonaBio;

            switch (State)
            {
                case States.CreateAvatar:
                    Footer.SetBackButtonText("Select Avatar");
                    Footer.SetNextButtonText("Create Persona");
                    currentPersona.avatar = AvatarDisplayScreen.currentAvatar;
                    Footer.TogglePanelInformation(true);
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
                    currentPersona.avatar = AvatarDisplayScreen.currentAvatar;
                    Footer.TogglePanelInformation(true);
                    AvatarDisplayScreen.SetButtonActive(true);
                    Footer.SetBackButtonText("Back");
                    Footer.SetNextButtonText("Save Changes");
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
                    Footer.SetBackButtonText("Back");
                    Footer.SetNextButtonText("Persona Information");
                    Footer.TogglePanelInformation(false);
                    State = States.CreateAvatar;
                    break;
                case States.EditInformation:
                    ClearCurrentPersona();
                    ScreenManager.Instance.ShowDashboard();
                    break;
                case States.EditAvatar:
                    currentPersona.avatar = existingAvatar;
                    AvatarDisplayScreen.OnAvatarSelected(existingAvatar);
                    AvatarDisplayScreen.SetButtonActive(true);
                    Footer.SetBackButtonText("Cancel");
                    Footer.SetNextButtonText("Save Changes");
                    Footer.TogglePanelInformation(true);
                    State = States.EditInformation;
                    break;
            }
        }

        private void OnReplaceAvatarClicked()
        {
            AvatarDisplayScreen.SetButtonActive(false);
            Footer.SetBackButtonText("Cancel");
            Footer.SetNextButtonText("Confirm Avatar");
            Footer.TogglePanelInformation(false);
            State = States.EditAvatar;
        }

        private void ClearCurrentPersona()
        {
            AvatarDisplayScreen.currentAvatar = null;
        }
    }
}
