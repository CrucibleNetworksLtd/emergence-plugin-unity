using EmergenceSDK.Types;

namespace EmergenceSDK.Services
{
    public interface IPersonaService
    {
        Persona CurrentPersona { get; }
        
        event PersonaUpdated OnCurrentPersonaUpdated;

        /// <summary>
        /// Attempts to create a new persona and confirms it was successful if the SuccessCreatePersona delegate is called
        /// </summary>
        void CreatePersona(Persona persona, SuccessCreatePersona success, ErrorCallback errorCallback);
        
        /// <summary>
        /// Attempts to get the current persona from the cache. Returns true if it was found, false otherwise.
        /// </summary>
        bool GetCurrentPersona(out Persona currentPersona);

        /// <summary>
        /// Attempts to get the current persona from the web service and returns it in the SuccessGetCurrentPersona delegate
        /// </summary>
        void GetCurrentPersona(SuccessGetCurrentPersona success, ErrorCallback errorCallback);
        
        /// <summary>
        /// Attempts to returns a list of personas and the current persona (if any) in the SuccessPersonas delegate
        /// </summary>
        void GetPersonas(SuccessPersonas success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to edit a persona and confirms it was successful if the SuccessEditPersona delegate is called
        /// </summary>
        void EditPersona(Persona persona, SuccessEditPersona success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to delete a persona and confirms it was successful if the SuccessDeletePersona delegate is called
        /// </summary>
        void DeletePersona(Persona persona, SuccessDeletePersona success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to set the current persona and confirms it was successful if the SuccessSetCurrentPersona delegate is called
        /// </summary>
        void SetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, ErrorCallback errorCallback);
    }
}