using EmergenceSDK.Services;

namespace EmergenceSDK.Internal.Services
{
    /// <summary>
    /// Used for services that rely on some kind of connection that can be disconnected, like a session.
    /// </summary>
    internal interface IDisconnectableService : IEmergenceService
    {
        /// <summary>
        /// The handler for when this service should be disconnected
        /// <remarks>Usually used for cleanup, and/or to set the service as disconnected and prepare for a new connection</remarks>
        /// </summary>
        void HandleDisconnection();
    }
}