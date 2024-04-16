using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Service for interacting with the persona API. This service is off chain.
    /// </summary>
    public interface IPersonaService : IEmergenceService
    {
        /// <summary>
        /// Whether or not the current persona has an access token.
        /// <remarks>This can be used to determine if you are connected to a session</remarks>
        /// </summary>
        
        /// <summary>
        /// Event fired when the current persona is updated.
        /// </summary>
        event PersonaUpdated OnCurrentPersonaUpdated;
        
        /// <summary>
        /// Attempts to get the current persona from the cache. Returns true if it was found, false otherwise.
        /// </summary>
        bool GetCachedPersona(out Persona currentPersona);
    }
}