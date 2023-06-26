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
        UniTask GetTransactionStatus(string transactionHash, string nodeURL, GetTransactionStatusSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Gets the highest block number of the chain. If successful, the success callback will be called.
        /// <remarks>This can be compared with a transaction block number to get further information</remarks>
        /// </summary>
        UniTask GetHighestBlockNumber(string nodeURL, GetBlockNumberSuccess success, ErrorCallback errorCallback);
    }
}