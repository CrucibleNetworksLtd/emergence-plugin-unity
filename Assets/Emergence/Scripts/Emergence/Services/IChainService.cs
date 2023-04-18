using Cysharp.Threading.Tasks;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Service for interacting with the chain API.
    /// </summary>
    public interface IChainService : IEmergenceService
    {
        /// <summary>
        /// Gets the status of a transaction. If successful, the success callback will be called.
        /// </summary>
        UniTask GetTransactionStatus<T>(string transactionHash, string nodeURL, GetTransactionStatusSuccess<T> success, ErrorCallback errorCallback);

        /// <summary>
        /// Gets the block number of a transaction. If successful, the success callback will be called.
        /// </summary>
        UniTask GetBlockNumber<T, U>(string transactionHash, string nodeURL, U body, GetBlockNumberSuccess<T> success, ErrorCallback errorCallback);
    }
}