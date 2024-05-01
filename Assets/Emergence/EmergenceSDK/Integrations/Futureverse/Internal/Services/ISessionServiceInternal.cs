using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;
using UnityEngine;

namespace EmergenceSDK.Integrations.Futureverse.Internal.Services
{
    internal interface ISessionServiceInternal : IEmergenceService
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
        UniTask GetQrCode(QRCodeSuccess success, ErrorCallback errorCallback, CancellationCallback cancellationCallback = default, CancellationToken ct = default);
        /// <summary>
        /// Attempts to get the login QR code
        /// </summary>
        UniTask<ServiceResponse<Texture2D>> GetQrCodeAsync(CancellationToken ct = default);

        /// <summary>
        /// Attempts to disconnect the user from Emergence, the success callback will fire if successful
        /// </summary>
        UniTask Disconnect(DisconnectSuccess success, ErrorCallback errorCallback);
        /// <summary>
        /// Attempts to disconnect the user from Emergence
        /// </summary>
        UniTask<ServiceResponse> DisconnectAsync();
        
        /// <summary>
        /// Current Persona's access token.
        /// <remarks>This token should be kept completely private</remarks>
        /// </summary>
        string CurrentAccessToken { get; }
        
        /// <summary>
        /// Attempts to get an access token, the success callback will fire with the token if successful
        /// </summary>
        UniTask GetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback);
        
        /// <summary>
        /// Attempts to get an access token
        /// </summary>
        UniTask<ServiceResponse<string>> GetAccessTokenAsync();
    }
}