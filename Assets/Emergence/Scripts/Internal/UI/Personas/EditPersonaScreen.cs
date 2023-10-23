using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.UI.Screens;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine;
using UnityEngine.Serialization;
using Avatar = EmergenceSDK.Types.Avatar;

namespace EmergenceSDK.Internal.UI.Personas
{
    public class EditPersonaScreen : MonoBehaviour
    {
        public static EditPersonaScreen Instance;
        public Texture2D DefaultImage;

        public PersonaCreationFooter Footer;
        [FormerlySerializedAs("AvatarDisplayScreen")] public AvatarSelectionScreen AvatarSelectionScreen;        
        public PersonaInfoPanel PersonaInfo;
        public PersonaCreationEditingStatusWidget StatusWidget;
        private Persona currentPersona;
        private IPersonaService PersonaService => EmergenceServices.GetService<IPersonaService>();
        private bool isNew;

        private void Awake()
        {
            Instance = this;
            Footer.OnNextClicked.AddListener(OnNextButtonClicked);
            Footer.OnBackClicked.AddListener(OnBackClicked);
            PersonaInfo.OnDeleteClicked.AddListener(OnDeleteClicked);
            AvatarSelectionScreen.ReplaceAvatarClicked.AddListener(OnReplaceAvatarClicked);
        }
        
        private void OnDestroy() 
        {
            Footer.OnNextClicked.RemoveListener(OnNextButtonClicked);
            Footer.OnBackClicked.RemoveListener(OnBackClicked);
            PersonaInfo.OnDeleteClicked.RemoveListener(OnDeleteClicked);
            AvatarSelectionScreen.ReplaceAvatarClicked.RemoveListener(OnReplaceAvatarClicked);
        }
        
        public void OnCreatePersonaClicked()
        {
            Persona persona = new Persona()
            {
                id = string.Empty,
                name = string.Empty,
                bio = string.Empty,
                avatar = new Avatar()
                {
                    avatarId = string.Empty,
                },
                AvatarImage = null,
            };
            Refresh(persona, true, true);
            ToggleAvatarSelectionAndPersonaInfo(false);
            StatusWidget.SetStepVisible(true, true);
        }
        
        public void OnEditPersonaClicked(Persona persona, bool isDefault)
        {
            Refresh(persona, isDefault);
            ToggleAvatarSelectionAndPersonaInfo(true);
            StatusWidget.SetStepVisible(true, false);
            Footer.SetBackButtonText("Back");
            Footer.SetNextButtonText("Save Changes");
        }

        public void Refresh(Persona persona, bool isActivePersona, bool isNew = false)
        {
            this.isNew = isNew;
            PersonaInfo.SetDeleteButtonActive(!isNew && !isActivePersona);

            currentPersona = persona;
            AvatarSelectionScreen.CurrentAvatar = persona.avatar;
            AvatarSelectionScreen.CurrentAvatar = persona.avatar;
            PersonaInfo.PersonaName = persona.name;
            PersonaInfo.PersonaBio = persona.bio;
            
            AvatarSelectionScreen.RefreshAvatarDisplay(persona.AvatarImage ?? DefaultImage);
        }

        private void OnNextButtonClicked()
        {
            StatusWidget.SetStepVisible(false, isNew);
            if(PersonaInfo.isActiveAndEnabled)
            {
                OnSavePersona();
            }
            else
            {
                OnSaveAvatar();
            }
        }

        private void OnSaveAvatar()
        {
            ToggleAvatarSelectionAndPersonaInfo(true);
        }
        
        private void OnSavePersona()
        {
            if (PersonaInfo.PersonaName.Length < 3)
            {
                ModalPromptOK.Instance.Show("Persona name must be at least 3 characters");
                return;
            }
            UpdateSelectedPersona();
            ScreenManager.Instance.ShowDashboard();
            if (isNew)
            {
                CreateNewPersona().Forget();
            }
            else
            {
                EditPersona().Forget();
            }
        }

        private void OnBackClicked()
        {
            StatusWidget.SetStepVisible(true, isNew);
            if (PersonaInfo.isActiveAndEnabled && !isNew) //Go back to Dashboard if editing existing persona
            {
                ClearCurrentPersona();
                ScreenManager.Instance.ShowDashboard();
            }
            else if (isNew && !PersonaInfo.isActiveAndEnabled) //Go back to dashboard if creating new persona and on avatar selection
            {
                ScreenManager.Instance.ShowDashboard();
            }
            else if(!isNew && !PersonaInfo.isActiveAndEnabled) //Go back to avatar selection if editing existing persona and on avatar selection
            {
                ToggleAvatarSelectionAndPersonaInfo(true);
            }
            else //Go back to avatar selection if creating new persona and on persona info
            {
                ToggleAvatarSelectionAndPersonaInfo(false);
            }
        }
        
        private void ToggleAvatarSelectionAndPersonaInfo(bool displayPersonaInfo)
        {
            AvatarSelectionScreen.AvatarScrollRoot.gameObject.SetActive(!displayPersonaInfo);
            AvatarSelectionScreen.SetButtonActive(displayPersonaInfo);
            PersonaInfo.gameObject.SetActive(displayPersonaInfo);
        }

        private async UniTask CreateNewPersona()
        {
            var response = await PersonaService.CreatePersonaAsync(currentPersona);
            if (response.Success)
            {
                EmergenceLogger.LogInfo($"New persona {currentPersona.name} created");
                ClearCurrentPersona();
                await PersonaUIManager.Instance.Refresh();
            }
            else
            {
                EmergenceLogger.LogError("Error creating persona");
                ModalPromptOK.Instance.Show("Error creating persona");
            }
        }
        
        private async UniTask EditPersona()
        {
            var response = await PersonaService.EditPersonaAsync(currentPersona);
            if (response.Success)
            {
                EmergenceLogger.LogInfo("Changes to Persona saved");
                ClearCurrentPersona();
                await PersonaUIManager.Instance.Refresh();
            }
            else
            {
                EmergenceLogger.LogError("Error editing persona");
                ModalPromptOK.Instance.Show("Error editing persona");
            }
        }

        private void UpdateSelectedPersona()
        {
            currentPersona.name = PersonaInfo.PersonaName;
            currentPersona.bio = PersonaInfo.PersonaBio;
            currentPersona.avatar = AvatarSelectionScreen.CurrentAvatar;
        }

        private void OnDeleteClicked()
        {
            ModalPromptYESNO.Instance.Show("Delete " + currentPersona.name, "are you sure?", () =>
            {
                Modal.Instance.Show("Deleting Persona...");
                PersonaService.DeletePersona(currentPersona, () =>
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

        private void OnReplaceAvatarClicked()
        {
            Footer.SetBackButtonText("Cancel");
            Footer.SetNextButtonText("Confirm Avatar");
            Footer.TogglePanelInformation(false);
            StatusWidget.SetStepVisible(false, false);
        }

        private void ClearCurrentPersona()
        {
            AvatarSelectionScreen.CurrentAvatar = null;
        }
    }
}
