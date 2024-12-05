using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Internal.Types;
using EmergenceSDK.Runtime.Internal.Utils;

namespace EmergenceSDK.Runtime.Services
{
    /// <summary>
    /// Service for performing custodial write operations (transaction encoding, signing and sending).
    /// </summary>
    public interface ICustodialWriteService : IEmergenceService
    {
        /// <summary>
        /// Perform the custodial write method by encoding data, requesting a transaction signature, and sending the transaction.
        /// </summary>
        /// <param name="contractInfo">The contract information.</param>
        /// <param name="inputValue">The value to send with the transaction.</param>
        /// <param name="dataToEncode">The data to encode for the transaction.</param>
        /// <returns>A task representing the asynchronous operation, with a WebResponse as the result.</returns>
        UniTask<WebResponse> PerformCustodialWriteMethod(ContractInfo contractInfo, string inputValue,
            string dataToEncode);
    }
}