using Cysharp.Threading.Tasks;
using EmergenceSDK.Types;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Service for interacting with the wallet API.
    /// </summary>
    public interface IWalletService : IEmergenceService
    {
        /// <summary>
        /// Address of the wallet that is currently logged in
        /// </summary>
        public string WalletAddress { get; }

        /// <summary>
        /// Attempts to sign a message using the walletconnect protocol, the success callback will return the signed message
        /// </summary>
        UniTask RequestToSign(string messageToSign, RequestToSignSuccess success, ErrorCallback errorCallback);
        /// <summary>
        /// Attempts to sign a message using the walletconnect protocol.
        /// </summary>
        UniTask<ServiceResponse<string>> RequestToSignAsync(string messageToSign);
        
        /// <summary>
        /// Attempts to handshake with the Emergence server, retrieving the wallet address if successful.
        /// </summary>
        UniTask Handshake(HandshakeSuccess success, ErrorCallback errorCallback);
        /// <summary>
        /// Attempts to handshake with the Emergence server.
        /// </summary>
        UniTask<ServiceResponse<string>> HandshakeAsync();

        /// <summary>
        /// Attempts to get the balance of the wallet, the success callback will fire with the balance if successful
        /// </summary>
        UniTask GetBalance(BalanceSuccess success, ErrorCallback errorCallback);
        /// <summary>
        /// Attempts to get the balance of the wallet.
        /// </summary>
        UniTask<ServiceResponse<string>> GetBalanceAsync();
        

        /// <summary>
        /// Attempts to validate a signed message, the success callback will fire with the validation result if the call is successful
        /// </summary>
        UniTask ValidateSignedMessage(string message, string signedMessage, string address, ValidateSignedMessageSuccess success, ErrorCallback errorCallback);
        /// <summary>
        /// Attempts to validate a signed message.
        /// </summary>
        UniTask<ServiceResponse<bool>> ValidateSignedMessageAsync(ValidateSignedMessageRequest data);
    }
}