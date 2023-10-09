using Cysharp.Threading.Tasks;
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
        private Persona currentPersona;
        private IPersonaService PersonaService => EmergenceServices.GetService<IPersonaService>();
        private bool isNew;

        private void Awake()
        {
            Instance = this;
            Footer.OnNextClicked.AddListener(OnNextButtonClicked);
            Footer.OnBackClicked.AddListener(OnBackClicked);
            PersonaInfo.OnDeleteClicked.AddListener(OnDeleteClicked);
            AvatarDisplayScreen.OnReplaceAvatarClicked.AddListener(OnReplaceAvatarClicked);
        }
        
        private void OnDestroy() 
        {
            Footer.OnNextClicked.RemoveListener(OnNextButtonClicked);
            Footer.OnBackClicked.RemoveListener(OnBackClicked);
            PersonaInfo.OnDeleteClicked.RemoveListener(OnDeleteClicked);
            AvatarDisplayScreen.OnReplaceAvatarClicked.RemoveListener(OnReplaceAvatarClicked);
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
            AvatarDisplayScreen.gameObject.SetActive(true);
            AvatarDisplayScreen.SetButtonActive(false);
            PersonaInfo.gameObject.SetActive(false);
            StatusWidget.SetVisible(isNew);
        }
        
        public void OnEditPersonaClicked(Persona persona, bool isDefault)
        {
            Refresh(persona, isDefault);
            AvatarDisplayScreen.gameObject.SetActive(false);
            AvatarDisplayScreen.SetButtonActive(true);
            PersonaInfo.gameObject.SetActive(true);
            StatusWidget.SetVisible(isNew);
            Footer.SetBackButtonText("Back");
            Footer.SetNextButtonText("Save Changes");
        }

        public void Refresh(Persona persona, bool isActivePersona, bool isNew = false)
        {
            this.isNew = isNew;
            PersonaInfo.SetDeleteButtonActive(!isNew && !isActivePersona);

            currentPersona = persona;
            AvatarDisplayScreen.CurrentAvatar = persona.avatar;
            AvatarDisplayScreen.CurrentAvatar = persona.avatar;
            PersonaInfo.PersonaName = persona.name;
            PersonaInfo.PersonaBio = persona.bio;
            
            AvatarDisplayScreen.RefreshAvatarDisplay(persona.AvatarImage ?? DefaultImage);
        }

        private void OnNextButtonClicked()
        {
            if(PersonaInfo.isActiveAndEnabled)
                OnSavePersona();
            else
                OnSaveAvatar();
        }

        private void OnSaveAvatar()
        {
            AvatarDisplayScreen.CurrentAvatar = AvatarDisplayScreen.CurrentAvatar;
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
            if(PersonaInfo.isActiveAndEnabled)
                BackFromPersonaInfo();
            else
                ToggleAvatarSelectionAndPersonaInfo(true);
        }

        private void BackFromPersonaInfo()
        {
            ClearCurrentPersona();
            ScreenManager.Instance.ShowDashboard();
        }
        
        private void ToggleAvatarSelectionAndPersonaInfo(bool displayPersonaInfo)
        {
            AvatarDisplayScreen.AvatarScrollRoot.gameObject.SetActive(!displayPersonaInfo);
            AvatarDisplayScreen.SetButtonActive(displayPersonaInfo);
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
            currentPersona.avatar = AvatarDisplayScreen.CurrentAvatar;
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
            AvatarDisplayScreen.SetButtonActive(false);
            Footer.SetBackButtonText("Cancel");
            Footer.SetNextButtonText("Confirm Avatar");
            Footer.TogglePanelInformation(false);
        }

        private void ClearCurrentPersona()
        {
            AvatarDisplayScreen.CurrentAvatar = null;
        }
    }
}
