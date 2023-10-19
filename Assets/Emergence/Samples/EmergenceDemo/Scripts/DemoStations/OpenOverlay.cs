using System.IO;
using EmergenceSDK.Internal.Services;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine;
using UnityEngine.Networking;

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
            EmergenceLogger.LogInfo("Changing avatar", true);
            if (persona != null && !string.IsNullOrEmpty(persona.avatarId))
            {
                
                avatarService.AvatarById(persona.avatarId, (async avatar =>
                {
                    var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, Helpers.InternalIPFSURLToHTTP(avatar.tokenURI), EmergenceLogger.LogError);
                    var token = Newtonsoft.Json.JsonConvert.DeserializeObject<EASMetadata[]>(response.Response);
                    DemoAvatarManager.Instance.SwapAvatars(Helpers.InternalIPFSURLToHTTP(token[0].UriBase));
                
                }), EmergenceLogger.LogError);
            }
            else
            {
                DemoAvatarManager.Instance.SetDefaultAvatar();
            }
        }
    }
}