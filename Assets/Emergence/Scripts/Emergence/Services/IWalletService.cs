using Cysharp.Threading.Tasks;

namespace EmergenceSDK.Services
{
    public interface IWalletService : IEmergenceService
    {
        public string WalletAddress { get; }
        
        /// <summary>
        /// TODO: FILL THIS IN
        /// </summary>
        UniTask ReinitializeWalletConnect(ReinitializeWalletConnectSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to sign a message using the walletconnect protocol, the success callback will return the signed message
        /// </summary>
        UniTask RequestToSign(string messageToSign, RequestToSignSuccess success, ErrorCallback errorCallback);
        
        /// <summary>
        /// Attempts to handshake with the Emergence server, retrieving the wallet address if successful.
        /// </summary>
        UniTask Handshake(HandshakeSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to create a new wallet, the success callback will fire if successful
        /// </summary>
        UniTask CreateWallet(string path, string password, CreateWalletSuccess success,
            ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to get the balance of the wallet, the success callback will fire with the balance if successful
        /// </summary>
        UniTask GetBalance(BalanceSuccess success, ErrorCallback errorCallback);
    }
}