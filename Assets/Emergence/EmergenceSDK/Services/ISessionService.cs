using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Implementations.Login.Types;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;
using UnityEngine;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Service for interacting with the current Wallet Connect Session.
    /// </summary>
    public interface ISessionService : IEmergenceService
    {
        /// <summary>
        /// This variable is only true when a full login has been completed, right before <see cref="OnSessionConnected"/> is called.
        /// It will also become false right before <see cref="OnSessionDisconnected"/> is called.
        /// </summary>
        bool IsLoggedIn { get; }
        
        /// <summary>
        /// The <see cref="LoginSettings"/> for the current session. Always null when <see cref="ISessionService.IsLoggedIn"/> is false. 
        /// </summary>
        LoginSettings? LoginSettings { get; }
        
        /// <summary>
        /// Set to true when mid way through a disconnect, disconnection can take a few seconds so this is useful for disabling UI elements for example
        /// </summary>
        bool DisconnectInProgress { get; }
        
        /// <summary>
        /// Fired when the session is disconnected, useful for handling any disconnection-related business logic.
        /// </summary>
        event Action OnSessionDisconnected;
        
        /// <summary>
        /// Fired when the login flow is completed and the session is connected, useful for handling any connection-related business logic.
        /// </summary>
        event Action OnSessionConnected;

        bool HasLoginSettings(LoginSettings enableFuturepass);
    }
}
