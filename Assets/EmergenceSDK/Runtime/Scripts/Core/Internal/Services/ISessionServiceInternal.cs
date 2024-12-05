using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Delegates;
using UnityEngine;

namespace EmergenceSDK.Runtime.Internal.Services
{
    internal interface ISessionServiceInternal : IEmergenceService
    {
        /// <summary>
        /// Attempts to get the login QR code, it will return the QR code as a texture in the success callback
        /// </summary>
        UniTask GetQrCode(QRCodeSuccess success, ErrorCallback errorCallback, CancellationCallback cancellationCallback = default, CancellationToken ct = default);
        /// <summary>
        /// Attempts to get the login QR code
        /// </summary>
        UniTask<ServiceResponse<Texture2D, string>> GetQrCodeAsync(CancellationToken ct = default);

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
        string EmergenceAccessToken { get; }

        Dictionary<string, string> EmergenceAccessTokenHeader => new() { { "Authorization", EmergenceAccessToken } };

        /// <summary>
        /// Attempts to get an access token, the success callback will fire with the token if successful
        /// </summary>
        UniTask GetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback);
        
        /// <summary>
        /// Attempts to get an access token
        /// </summary>
        UniTask<ServiceResponse<string>> GetAccessTokenAsync();
        
        /// <summary>
        /// Attempts to get an access token using an external custodial login service.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        UniTask<ServiceResponse<string>> GetCustodialAccessToken(CancellationToken ct);

        void RunConnectionEvents(LoginSettings loginSettings);
        
        void RunDisconnectionEvents();

    }
}