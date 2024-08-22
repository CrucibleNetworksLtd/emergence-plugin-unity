using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Delegates;
using EmergenceSDK.Runtime.Types.Responses;

namespace EmergenceSDK.Runtime.Services
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
        /// <param name="contractInfo">A properly populated <see cref="ContractInfo"/> object</param>
        /// <param name="parameters">The parameters to call the method with</param>
        /// <param name="success">Delegate of type <see cref="ReadMethodSuccess"/>, called in case of success</param>
        /// <param name="errorCallback">Delegate of type <see cref="ErrorCallback"/>, called in case of failure</param>
        /// <typeparam name="T">The type of the body to serialize</typeparam>
        UniTask ReadMethod<T>(ContractInfo contractInfo, T parameters, ReadMethodSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Calls a "read" method on the given contract.
        /// </summary>
        /// <param name="contractInfo">A properly populated <see cref="ContractInfo"/> object</param>
        /// <param name="parameters">The parameters to call the method with</param>
        /// <typeparam name="T">The type of the body to serialize</typeparam>
        /// <returns>A <see cref="ReadContractResponse"/> wrapped within a <see cref="ServiceResponse{T}"/> object</returns>
        UniTask<ServiceResponse<ReadContractResponse>> ReadMethodAsync<T>(ContractInfo contractInfo, T parameters);

        /// <summary>
        /// Calls a "write" method on the given contract.
        /// </summary>
        /// <param name="contractInfo">A properly populated <see cref="ContractInfo"/> object</param>
        /// <param name="value">The amount to transfer from sender to recipient (in Wei, or equivalent).</param>
        /// <param name="parameters">The parameters to call the method with</param>
        /// <param name="success">Delegate of type <see cref="WriteMethodSuccess"/>, called in case of success</param>
        /// <param name="errorCallback">Delegate of type <see cref="ErrorCallback"/>, called in case of failure</param>
        /// <typeparam name="T">The type of the body to serialize</typeparam>
        UniTask WriteMethod<T>(ContractInfo contractInfo, string value, T parameters, WriteMethodSuccess success, ErrorCallback errorCallback);

        /// <summary>
        /// Calls a "write" method on the given contract.
        /// </summary>
        /// <param name="contractInfo">A properly populated <see cref="ContractInfo"/> object</param>
        /// <param name="value">The amount to transfer from sender to recipient (in Wei, or equivalent).</param>
        /// <param name="parameters">The parameters to call the method with</param>
        /// <typeparam name="T">The type of the body to serialize</typeparam>
        /// <returns>A <see cref="WriteContractResponse"/> wrapped within a <see cref="ServiceResponse{T}"/> object</returns>
        UniTask<ServiceResponse<WriteContractResponse>> WriteMethodAsync<T>(ContractInfo contractInfo, string value, T parameters);
    }
}