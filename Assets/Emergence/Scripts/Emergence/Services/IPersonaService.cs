using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Types;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Service for interacting with the persona API. This service is off chain.
    /// </summary>
    public interface IPersonaService : IEmergenceService
    {
        /// <summary>
        /// Current Persona's access token.
        /// <remarks>This token should be kept completely private</remarks>
        /// </summary>
        string CurrentAccessToken { get; }
        
        /// <summary>
        /// Whether or not the current persona has an access token.
        /// <remarks>This can be used to determine if you are connected to a session</remarks>
        /// </summary>
        bool HasAccessToken { get; }
        
        /// <summary>
        /// Event fired when the current persona is updated.
        /// </summary>
        event PersonaUpdated OnCurrentPersonaUpdated;
        
        /// <summary>
        /// Attempts to get the current persona from the cache. Returns true if it was found, false otherwise.
        /// </summary>
        bool GetCurrentPersona(out Persona currentPersona);
        
        /// <summary>
        /// Attempts to get an access token, the success callback will fire with the token if successful
        /// </summary>
        UniTask GetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback);
        /// <summary>
        /// Attempts to get an access token
        /// </summary>
        UniTask<ServiceResponse<string>> GetAccessTokenAsync();
        
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