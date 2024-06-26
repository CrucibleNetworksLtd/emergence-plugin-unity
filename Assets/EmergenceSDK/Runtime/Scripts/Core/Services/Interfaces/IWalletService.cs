using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Delegates;

namespace EmergenceSDK.Runtime.Services
{
    /// <summary>
    /// Service for interacting with the wallet API.
    /// </summary>
    public interface IWalletService : IEmergenceService
    {
        /// <summary>
        /// Address of the wallet that is currently logged in
        /// </summary>
        string WalletAddress { get; }
        
        /// <summary>
        /// Checksummed address of the wallet that is currently logged in
        /// </summary>
        string ChecksummedWalletAddress { get; }
        
        /// <summary>
        /// Whether an address is currently logged in, checks WalletAddress and ChecksummedWalletAddress to not be empty
        /// </summary>
        bool IsValidWallet { get; }
        
        /// <summary>
        /// Attempts to sign a message using the walletconnect protocol, the success callback will return the signed message
        /// </summary>
        /// <param name="messageToSign">Message to sign</param>
        /// <param name="success">Delegate of type <see cref="RequestToSignSuccess"/>, called in case of success</param>
        /// <param name="errorCallback">Delegate of type <see cref="ErrorCallback"/>, called in case of failure</param>
        UniTask RequestToSign(string messageToSign, RequestToSignSuccess success, ErrorCallback errorCallback);
        
        /// <summary>
        /// Attempts to sign a message using the walletconnect protocol.
        /// </summary>
        /// <param name="messageToSign">Message to sign</param>
        /// <returns>The signed message, wrapped within a <see cref="ServiceResponse{T}"/> object</returns>
        UniTask<ServiceResponse<string>> RequestToSignAsync(string messageToSign);

        /// <summary>
        /// Attempts to get the balance of the wallet, the success callback will fire with the balance if successful
        /// </summary>
        /// <param name="success">Delegate of type <see cref="BalanceSuccess"/>, called in case of success</param>
        /// <param name="errorCallback">Delegate of type <see cref="ErrorCallback"/>, called in case of failure</param>
        UniTask GetBalance(BalanceSuccess success, ErrorCallback errorCallback);
        
        /// <summary>
        /// Attempts to get the balance of the currently logged wallet.
        /// </summary>
        /// <returns>The balance, wrapped within a <see cref="ServiceResponse{T}"/> object</returns>
        UniTask<ServiceResponse<string>> GetBalanceAsync();
        

        /// <summary>
        /// Attempts to validate a signed message, the success callback will fire with the validation result if the call is successful
        /// </summary>
        /// <param name="message">Message that has been signed</param>
        /// <param name="signedMessage">Message signature</param>
        /// <param name="address">Wallet address that signed the <paramref name="message"/></param>
        /// <param name="success">Delegate of type <see cref="ValidateSignedMessageSuccess"/>, called in case of success</param>
        /// <param name="errorCallback">Delegate of type <see cref="ErrorCallback"/>, called in case of failure</param>
        UniTask ValidateSignedMessage(string message, string signedMessage, string address, ValidateSignedMessageSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to validate a signed message.
        /// </summary>
        /// <param name="message">Message that has been signed</param>
        /// <param name="signedMessage">Message signature</param>
        /// <param name="address">Wallet address that signed the <paramref name="message"/></param>
        /// <returns>A boolean representing whether the message was validated, wrapped within a <see cref="ServiceResponse{T}"/> object</returns>
        UniTask<ServiceResponse<bool>> ValidateSignedMessageAsync(string message, string signedMessage, string address);
        
        /// <summary>
        /// Attempts to validate a signed message.
        /// </summary>
        /// <param name="data">A <see cref="ValidateSignedMessageRequest"/> object</param>
        /// <returns>A boolean representing whether the message was validated, wrapped within a <see cref="ServiceResponse{T}"/> object</returns>
        UniTask<ServiceResponse<bool>> ValidateSignedMessageAsync(ValidateSignedMessageRequest data);
    }
}