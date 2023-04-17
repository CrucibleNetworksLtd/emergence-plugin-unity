using Cysharp.Threading.Tasks;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Service that gives access to the QR code for login
    /// </summary>
    public interface IQRCodeService : IEmergenceService
    {
        /// <summary>
        /// Attempts to get the login QR code, it will return the QR code as a texture in the success callback
        /// </summary>
        UniTask GetQRCode(QRCodeSuccess success, ErrorCallback errorCallback);
    }
}