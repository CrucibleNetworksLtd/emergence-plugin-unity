using Cysharp.Threading.Tasks;
using EmergenceSDK.Types;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Service for interacting with the current Wallet Connect Session.
    /// </summary>
    public interface ISessionService : IEmergenceService
    {
        bool DisconnectInProgress { get; }
        
        /// <summary>
        /// Attempts to get the login QR code, it will return the QR code as a texture in the success callback
        /// </summary>
        UniTask GetQRCode(QRCodeSuccess success, ErrorCallback errorCallback);
        
        /// <summary>
        /// Attempts to check if the user is connected to Emergence, connection status is provided in the IsConnectedSuccess callback.
        /// </summary>
        UniTask IsConnected(IsConnectedSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to disconnect the user from Emergence, the success callback will fire if successful
        /// </summary>
        UniTask Disconnect(DisconnectSuccess success, ErrorCallback errorCallback);
    }
}
