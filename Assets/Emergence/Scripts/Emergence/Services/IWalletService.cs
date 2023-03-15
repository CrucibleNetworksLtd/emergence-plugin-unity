namespace EmergenceSDK.Services
{
    public interface IWalletService
    {
        public string WalletAddress { get; }
        
        /// <summary>
        /// TODO: FILL THIS IN
        /// </summary>
        void ReinitializeWalletConnect(ReinitializeWalletConnectSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to sign a message using the walletconnect protocol, the success callback will return the signed message
        /// </summary>
        void RequestToSign(string messageToSign, RequestToSignSuccess success, ErrorCallback errorCallback);
        
        /// <summary>
        /// Attempts to handshake with the Emergence server, retrieving the wallet address if successful.
        /// </summary>
        public void Handshake(HandshakeSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to create a new wallet, the success callback will fire if successful
        /// </summary>
        public void CreateWallet(string path, string password, CreateWalletSuccess success,
            ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to get the balance of the wallet, the success callback will fire with the balance if successful
        /// </summary>
        void GetBalance(BalanceSuccess success, ErrorCallback errorCallback);
    }
}