using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public class OpenOverlay : DemoStation<OpenOverlay>, IDemoStation
    {
        private void OnEnable() 
        {
            EmergenceServices.Instance.PersonaService.OnCurrentPersonaUpdated += OnPersonaUpdated;
        }

        private void Start()
        {
            instructions.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            instructions.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            instructions.SetActive(false);
        }

        private void Update()
        {
            if (HasBeenActivated() && IsReady)
            {
                EmergenceSingleton.Instance.OpenEmergenceUI();
            }
        }

        public void OnPersonaUpdated(Persona persona) 
        {
            Debug.Log("Changing avatar");
            if (persona != null && !string.IsNullOrEmpty(persona.avatarId))
            {
                EmergenceServices.Instance.AvatarById(persona.avatarId, (avatar =>
                {
                    DemoAvatarManager.Instance.SwapAvatars(avatar.meta.content[1].url);
                
                }), (message, code) =>
                {
                    Debug.LogError("Error fetching Avatar by id: " + message);
                });
            }
            else
            {
                DemoAvatarManager.Instance.SetDefaultAvatar();
            }
        }

        public bool IsReady { get; set; }
    }
}