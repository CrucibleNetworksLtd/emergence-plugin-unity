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
        /// Calls a "read" method on the given contract.
        /// <remarks>The contract in question must be loaded using <see cref="LoadContract"/></remarks>
        /// </summary>
        UniTask ReadMethod<T, U>(ContractInfo contractInfo, U body, ReadMethodSuccess<T> success, ErrorCallback errorCallback);

        /// <summary>
        /// Calls a "write" method on the given contract.
        /// <remarks>The contract in question must be loaded using <see cref="LoadContract"/></remarks>
        /// </summary>
        UniTask WriteMethod<T, U>(ContractInfo contractInfo, string localAccountName, string gasPrice, string value, U body, WriteMethodSuccess<T> success, ErrorCallback errorCallback);
    }
}