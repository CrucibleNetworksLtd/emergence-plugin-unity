using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Types;
using UnityEngine;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Service for interacting with the current Wallet Connect Session.
    /// </summary>
    public interface ISessionService : IEmergenceService
    {
        /// <summary>
        /// Set to true when mid way through a disconnect, disconnection can take a few seconds so this is useful for disabling UI elements for example
        /// </summary>
        bool DisconnectInProgress { get; }
        
        /// <summary>
        /// Fired when the session is disconnected
        /// </summary>
        event Action OnSessionDisconnected;
        
        /// <summary>
        /// Attempts to get the login QR code, it will return the QR code as a texture in the success callback
        /// </summary>
        UniTask GetQRCode(QRCodeSuccess success, ErrorCallback errorCallback);
        /// <summary>
        /// Attempts to get the login QR code
        /// </summary>
        UniTask<ServiceResponse<Texture2D>> GetQRCodeAsync();

        /// <summary>
        /// Attempts to disconnect the user from Emergence, the success callback will fire if successful
        /// </summary>
        UniTask Disconnect(DisconnectSuccess success, ErrorCallback errorCallback);
        /// <summary>
        /// Attempts to disconnect the user from Emergence
        /// </summary>
        UniTask<ServiceResponse> DisconnectAsync();
    }
}
