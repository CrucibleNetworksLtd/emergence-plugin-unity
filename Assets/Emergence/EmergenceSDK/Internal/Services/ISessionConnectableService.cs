using EmergenceSDK.Services;

namespace EmergenceSDK.Internal.Services
{
    /// <summary>
    /// Used by services that rely on <see cref="ISessionService"/>'s session connection and need to handle connection and disconnection
    /// </summary>
    internal interface ISessionConnectableService : IEmergenceService
    {
        /// <summary>
        /// The handler for when this service should be disconnected
        /// <remarks>Usually used for cleanup, and/or to set the service as disconnected and prepare for a new connection</remarks>
        /// </summary>
        void HandleDisconnection(ISessionService sessionService);
        
        /// <summary>
        /// The handler for when this service should be disconnected
        /// <remarks>Usually used for cleanup, and/or to set the service as disconnected and prepare for a new connection</remarks>
        /// </summary>
        void HandleConnection(ISessionService sessionService);
    }
}