using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Provides access to the contract API. 
    /// </summary>
    public interface IContractService : IEmergenceService
    {
        /// <summary>
        /// Loads a contract into the local server's memory. If successful, the success callback will be called.
        /// </summary>
        UniTask LoadContract(string contractAddress, string ABI, string network, LoadContractSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Gets the status of a transaction. If successful, the success callback will be called.
        /// </summary>
        UniTask GetTransactionStatus<T>(string transactionHash, string nodeURL, GetTransactionStatusSuccess<T> success, ErrorCallback errorCallback);

        /// <summary>
        /// Gets the block number of a transaction. If successful, the success callback will be called.
        /// </summary>
        UniTask GetBlockNumber<T, U>(string transactionHash, string nodeURL, U body, GetBlockNumberSuccess<T> success, ErrorCallback errorCallback);

        /// <summary>
        /// Calls a "read" method on the given contract.
        /// </summary>
        UniTask ReadMethod<T, U>(ContractInfo contractInfo, U body, ReadMethodSuccess<T> success, ErrorCallback errorCallback);

        /// <summary>
        /// Calls a "write" method on the given contract.
        /// </summary>
        UniTask WriteMethod<T, U>(ContractInfo contractInfo, string localAccountName, string gasPrice, string value, U body, WriteMethodSuccess<T> success, ErrorCallback errorCallback);
    }
}