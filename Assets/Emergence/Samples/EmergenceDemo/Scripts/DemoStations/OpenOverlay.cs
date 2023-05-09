using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public class OpenOverlay : DemoStation<OpenOverlay>, IDemoStation
    {
        private IPersonaService personaService;
        private IAvatarService avatarService;

        public bool IsReady
        {
            get => isReady;
            set
            {
                InstructionsText.text = value ? ActiveInstructions : InactiveInstructions;
                isReady = value;
            }
        }

        private void Start()
        {
            personaService = EmergenceServices.GetService<IPersonaService>();
            personaService.OnCurrentPersonaUpdated += OnPersonaUpdated;
            avatarService = EmergenceServices.GetService<IAvatarService>();
            
            instructionsGO.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            instructionsGO.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            instructionsGO.SetActive(false);
        }

        private void Update()
        {
            if (HasBeenActivated())
            {
                EmergenceSingleton.Instance.OpenEmergenceUI();
            }
        }

        public void OnPersonaUpdated(Persona persona) 
        {
            Debug.Log("Changing avatar");
            if (persona != null && !string.IsNullOrEmpty(persona.avatarId))
            {
                avatarService.AvatarById(persona.avatarId, (avatar =>
                {
                    DemoAvatarManager.Instance.SwapAvatars(avatar.meta.content[1].url);
                
                }), EmergenceLogger.LogError);
            }
            else
            {
                DemoAvatarManager.Instance.SetDefaultAvatar();
            }
        }
    }
}