using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using UnityEngine;

namespace EmergenceSDK.Samples.Examples
{
    public class ConnectingToWallet : MonoBehaviour
    {
        private ISessionServiceInternal sessionServiceInternal;
        private IWalletServiceInternal walletServiceInternal;

        private void Awake()
        {
            sessionServiceInternal = EmergenceServiceProvider.GetService<ISessionServiceInternal>();
            walletServiceInternal = EmergenceServiceProvider.GetService<IWalletServiceInternal>();
        }

        private void Start()
        {
            sessionServiceInternal.GetQRCode(GotQRCode, EmergenceLogger.LogError);
        }

        private void GotQRCode(Texture2D qrcode)
        {
            //Present the QR code to the user for scanning

            //.....

            //Wait for the user to connect to the wallet
            walletServiceInternal.Handshake(OnHandshakeSuccess, EmergenceLogger.LogError);
        }

        private void OnHandshakeSuccess(string walletaddress)
        {
            //The user has connected to the wallet
            //You can now use the wallet address for whatever you need it for
            Debug.Log($"Wallet address: {walletaddress}");
        }
    }
}