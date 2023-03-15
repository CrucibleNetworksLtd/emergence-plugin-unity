namespace EmergenceSDK.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAccountService
    {
        string CurrentAccessToken { get; set; }
        bool HasAccessToken { get; }
        bool DisconnectInProgress { get; }

        /// <summary>
        /// Attempts to check if the user is connected to Emergence, connection status is provided in the IsConnectedSuccess callback.
        /// </summary>
        void IsConnected(IsConnectedSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to create a key store, the success callback will fire if successful
        /// </summary>
        void CreateKeyStore(string privateKey, string password, string publicKey, string path,
            CreateKeyStoreSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to load an account, the success callback will fire if successful
        /// </summary>
        public void LoadAccount(string name, string password, string path, string nodeURL, string chainId,
            LoadAccountSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to get an access token, the success callback will fire with the token if successful
        /// </summary>
        void GetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to validate an access token, the success callback will fire with the validation result if the call is successful
        /// </summary>
        void ValidateAccessToken(ValidateAccessTokenSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to validate a signed message, the success callback will fire with the validation result if the call is successful
        /// </summary>
        void ValidateSignedMessage(string message, string signedMessage, string address, ValidateSignedMessageSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to disconnect the user from Emergence, the success callback will fire if successful
        /// </summary>
        void Disconnect(DisconnectSuccess success, ErrorCallback errorCallback);
        
        //TODO: Fill this in
        void Finish(SuccessFinish success, ErrorCallback errorCallback);
    }
}
