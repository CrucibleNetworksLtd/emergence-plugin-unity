using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Delegates;
using EmergenceSDK.Runtime.Types.Responses;

namespace EmergenceSDK.Runtime.Services
{
    /// <summary>
    /// Service for interacting with the chain API.
    /// </summary>
    public interface IChainService : IEmergenceService
    {
        /// <summary>
        /// Gets the status of a transaction.
        /// </summary>
        /// <param name="transactionHash">The hash of the transaction to check</param>
        /// <param name="nodeURL">The URL of the chain node</param>
        /// <returns></returns>
        UniTask<ServiceResponse<GetTransactionStatusResponse>> GetTransactionStatusAsync(string transactionHash, string nodeURL);
        
        /// <summary>
        /// Gets the status of a transaction. If successful, the success callback will be called.
        /// </summary>
        /// <param name="transactionHash">The hash of the transaction to check</param>
        /// <param name="nodeURL">The URL of the chain node</param>
        /// <param name="success">Delegate of type <see cref="GetTransactionStatusSuccess"/>, called in case of success</param>
        /// <param name="errorCallback">Delegate of type <see cref="ErrorCallback"/>, called in case of failure</param>
        UniTask GetTransactionStatus(string transactionHash, string nodeURL, GetTransactionStatusSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Gets the highest block number of the chain.
        /// </summary>
        /// <param name="nodeURL">The URL of the chain node</param>
        /// <returns><see cref="GetBlockNumberResponse"/> object wrapped within a <see cref="ServiceResponse{T}"/></returns>
        UniTask<ServiceResponse<GetBlockNumberResponse>> GetHighestBlockNumberAsync(string nodeURL);
        
        /// <summary>
        /// Gets the highest block number of the chain. If successful, the success callback will be called.
        /// <remarks>This can be compared with a transaction block number to get further information</remarks>
        /// </summary>
        /// <param name="nodeURL">The URL of the chain node</param>
        /// <param name="success">Delegate of type <see cref="GetBlockNumberSuccess"/>, called in case of success</param>
        /// <param name="errorCallback">Delegate of type <see cref="ErrorCallback"/>, called in case of failure</param>
        /// <returns></returns>
        UniTask GetHighestBlockNumber(string nodeURL, GetBlockNumberSuccess success, ErrorCallback errorCallback);
    }
}