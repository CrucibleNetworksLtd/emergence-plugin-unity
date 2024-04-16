using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;

namespace EmergenceSDK.Integrations.Futureverse.Internal.Services
{
    public interface IPersonaServiceInternal : IEmergenceService
    {
                /// <summary>
        /// Attempts to create a new persona and confirms it was successful if the SuccessCreatePersona delegate is called
        /// </summary>
        UniTask CreatePersona(Persona persona, SuccessCreatePersona success, ErrorCallback errorCallback);
        /// <summary>
        /// Attempts to create a new persona
        /// </summary>
        UniTask<ServiceResponse> CreatePersonaAsync(Persona persona);
        
        /// <summary>
        /// Attempts to get the current persona from the web service and returns it in the SuccessGetCurrentPersona delegate
        /// </summary>
        UniTask GetCurrentPersona(SuccessGetCurrentPersona success, ErrorCallback errorCallback);
        /// <summary>
        /// Attempts to get the current persona from the web service
        /// </summary>
        UniTask<ServiceResponse<Persona>> GetCurrentPersonaAsync();
        
        /// <summary>
        /// Attempts to returns a list of personas and the current persona (if any) in the SuccessPersonas delegate
        /// </summary>
        UniTask GetPersonas(SuccessPersonas success, ErrorCallback errorCallback);
        /// <summary>
        /// Attempts to returns a list of personas and the current persona.
        /// </summary>
        UniTask<ServiceResponse<List<Persona>, Persona>> GetPersonasAsync();

        /// <summary>
        /// Attempts to edit a persona and confirms it was successful if the SuccessEditPersona delegate is called
        /// </summary>
        UniTask EditPersona(Persona persona, SuccessEditPersona success, ErrorCallback errorCallback);
        /// <summary>
        /// Attempts to edit a persona
        /// </summary>
        UniTask<ServiceResponse> EditPersonaAsync(Persona persona);

        /// <summary>
        /// Attempts to delete a persona and confirms it was successful if the SuccessDeletePersona delegate is called
        /// </summary>
        UniTask DeletePersona(Persona persona, SuccessDeletePersona success, ErrorCallback errorCallback);
        /// <summary>
        /// Attempts to delete a persona
        /// </summary>
        UniTask<ServiceResponse> DeletePersonaAsync(Persona persona);

        /// <summary>
        /// Attempts to set the current persona and confirms it was successful if the SuccessSetCurrentPersona delegate is called
        /// </summary>
        UniTask SetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, ErrorCallback errorCallback);
        /// <summary>
        /// Attempts to set the current persona
        /// </summary>
        UniTask<ServiceResponse> SetCurrentPersonaAsync(Persona persona);
    }
}