using Cysharp.Threading.Tasks;

namespace EmergenceSDK.Services
{
    public interface IQRCodeService
    {
        /// <summary>
        /// Attempts to get the login QR code, it will return the QR code as a texture in the success callback
        /// </summary>
        UniTask GetQRCode(QRCodeSuccess success, ErrorCallback errorCallback);
    }
}