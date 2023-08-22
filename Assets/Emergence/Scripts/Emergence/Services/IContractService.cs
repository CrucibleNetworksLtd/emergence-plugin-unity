using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Provides access to the contract API. 
    /// </summary>
    public interface IContractService : IEmergenceService
    {
        /// <summary>
        /// Event fired when a contract write is successful.
        /// </summary>
        event WriteMethodSuccess WriteMethodConfirmed;
        
        /// <summary>
        /// Calls a "read" method on the given contract.
        /// </summary>
        UniTask ReadMethod<T>(ContractInfo contractInfo, T body, ReadMethodSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Calls a "read" method on the given contract.
        /// </summary>
        UniTask<ServiceResponse<ReadContractResponse>> ReadMethodAsync<T>(ContractInfo contractInfo, T body);
        
        /// <summary>
        /// Calls a "write" method on the given contract.
        /// </summary>
        UniTask WriteMethod<T>(ContractInfo contractInfo, string localAccountNameIn, string gasPriceIn, string value, T body, WriteMethodSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Calls a "write" method on the given contract.
        /// </summary>
        UniTask<ServiceResponse<WriteContractResponse>> WriteMethodAsync<T>(ContractInfo contractInfo, string localAccountNameIn, string gasPriceIn, string value, T body);
    }
}