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
        /// Calls a "read" method on the given contract.
        /// <remarks>The contract in question must be loaded using <see cref="LoadContract"/></remarks>
        /// </summary>
        UniTask ReadMethod<T>(ContractInfo contractInfo, T body, ReadMethodSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Calls a "write" method on the given contract.
        /// <remarks>The contract in question must be loaded using <see cref="LoadContract"/></remarks>
        /// </summary>
        UniTask WriteMethod<T>(ContractInfo contractInfo, string localAccountNameIn, string gasPriceIn, string value, T body, WriteMethodSuccess success, ErrorCallback errorCallback);
    }
}