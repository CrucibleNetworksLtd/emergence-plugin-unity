using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Delegates;

namespace EmergenceSDK.Runtime.Services
{
    /// <summary>
    /// Service for interacting with the persona API. This service is off chain.
    /// </summary>
    public interface IPersonaService : IEmergenceService
    {
        /// <summary>
        /// Event fired when the current persona is updated.
        /// </summary>
        event PersonaUpdated OnCurrentPersonaUpdated;
        
        /// <summary>
        /// Attempts to get the current persona from the cache.
        /// </summary>
        /// <param name="currentPersona">The retrieved persona</param>
        /// <returns>True if it was found, false otherwise</returns>
        bool GetCachedPersona(out Persona currentPersona);
        
        /// <summary>
        /// Attempts to get the current persona from the web service and returns it in the SuccessGetCurrentPersona delegate
        /// </summary>
        UniTask GetCurrentPersona(SuccessGetCurrentPersona success, ErrorCallback errorCallback);
    }
}