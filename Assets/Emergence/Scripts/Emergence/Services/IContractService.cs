using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;

namespace EmergenceSDK.Services
{
    public interface IContractService : IEmergenceService
    {
        UniTask LoadContract(string contractAddress, string ABI, string network, LoadContractSuccess success,
            ErrorCallback errorCallback);

        UniTask GetTransactionStatus<T>(string transactionHash, string nodeURL,
            GetTransactionStatusSuccess<T> success, ErrorCallback errorCallback);

        UniTask GetBlockNumber<T, U>(string transactionHash, string nodeURL, U body,
            GetBlockNumberSuccess<T> success, ErrorCallback errorCallback);

        UniTask ReadMethod<T, U>(ContractInfo contractInfo, U body,
            ReadMethodSuccess<T> success, ErrorCallback errorCallback);

        UniTask WriteMethod<T, U>(ContractInfo contractInfo, string localAccountName, string gasPrice, string value,
            U body, WriteMethodSuccess<T> success, ErrorCallback errorCallback);
    }
}