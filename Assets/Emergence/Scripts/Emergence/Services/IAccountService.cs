using Cysharp.Threading.Tasks;
using EmergenceSDK.Types;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAccountService : IEmergenceService
    {
        string CurrentAccessToken { get; set; }
        bool HasAccessToken { get; }
        bool DisconnectInProgress { get; }
        
        Expiration Expiration { get; }

        /// <summary>
        /// Sets the expiration object from the expiration message
        /// </summary>
        void ProcessExpiration(string expirationMessage);

        /// <summary>
        /// Attempts to check if the user is connected to Emergence, connection status is provided in the IsConnectedSuccess callback.
        /// </summary>
        UniTask IsConnected(IsConnectedSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to create a key store, the success callback will fire if successful
        /// </summary>
        UniTask CreateKeyStore(string privateKey, string password, string publicKey, string path,
            CreateKeyStoreSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to load an account, the success callback will fire if successful
        /// </summary>
        UniTask LoadAccount(Account account, LoadAccountSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to get an access token, the success callback will fire with the token if successful
        /// </summary>
        UniTask GetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to validate an access token, the success callback will fire with the validation result if the call is successful
        /// </summary>
        UniTask ValidateAccessToken(ValidateAccessTokenSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to validate a signed message, the success callback will fire with the validation result if the call is successful
        /// </summary>
        UniTask ValidateSignedMessage(string message, string signedMessage, string address,
            ValidateSignedMessageSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to disconnect the user from Emergence, the success callback will fire if successful
        /// </summary>
        UniTask Disconnect(DisconnectSuccess success, ErrorCallback errorCallback);
        
        //TODO: Fill this in
        UniTask Finish(SuccessFinish success, ErrorCallback errorCallback);
    }
}
