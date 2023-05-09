using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine;

namespace EmergenceSDK.Samples.Examples
{
    public class GettingCurrentPersona : MonoBehaviour
    {
        private IPersonaService personaService;

        private void Awake()
        {
            // Initialize the personaService variable by getting the IPersonaService instance from the EmergenceServices class
            personaService = EmergenceServices.GetService<IPersonaService>();
        }

        private void Start()
        {
            GetCurrentPersonaAsync();
        }

        private async void GetCurrentPersonaAsync()
        {
            // Waits for the personaService to return the current persona and then calls the GetPersonaSuccess method
            await personaService.GetCurrentPersona(GetPersonaSuccess, EmergenceLogger.LogError);
        }

        // This method is called when the personaService successfully returns the current persona
        private void GetPersonaSuccess(Persona currentpersona)
        {
            // Logs the current persona to the console
            Debug.Log($"Found persona: {currentpersona}");
            
            // Calls the GetCachedPersona method to get the cached persona
            GetCachedPersona();
        }

        // This method gets the cached persona from the personaService instance
        private void GetCachedPersona()
        {
            // Checks if there is a cached persona and logs it to the console if there is
            if (personaService.GetCurrentPersona(out Persona persona))
            {
                Debug.Log($"Found cached persona: {persona}");
            }
        }
    }
}
