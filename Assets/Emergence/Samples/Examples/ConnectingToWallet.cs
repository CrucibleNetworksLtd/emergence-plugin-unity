using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using UnityEngine;

namespace EmergenceSDK.Samples.Examples
{
    public class ConnectingToWallet : MonoBehaviour
    {
        private IQRCodeService qrService;
        private IWalletService walletService;

        private void Awake()
        {
            qrService = EmergenceServices.GetService<IQRCodeService>();
            walletService = EmergenceServices.GetService<IWalletService>();
        }

        private void Start()
        {
            qrService.GetQRCode(GotQRCode, ErrorLogger.LogError);
        }

        private void GotQRCode(Texture2D qrcode, string deviceid)
        {
            //Present the QR code to the user for scanning

            //.....

            //Wait for the user to connect to the wallet
            walletService.Handshake(OnHandshakeSuccess, ErrorLogger.LogError);
        }

        private void OnHandshakeSuccess(string walletaddress)
        {
            //The user has connected to the wallet
            //You can now use the wallet address for whatever you need it for
            Debug.Log($"Wallet address: {walletaddress}");
        }
    }
}