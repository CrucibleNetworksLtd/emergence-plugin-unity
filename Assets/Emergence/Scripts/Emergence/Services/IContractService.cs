using EmergenceSDK.Internal.Utils;

namespace EmergenceSDK.Services
{
    public interface IContractService
    {
        void LoadContract(string contractAddress, string ABI, string network, LoadContractSuccess success,
            ErrorCallback errorCallback);

        void GetTransactionStatus<T>(string transactionHash, string nodeURL,
            GetTransactionStatusSuccess<T> success, ErrorCallback errorCallback);

        void GetBlockNumber<T, U>(string transactionHash, string nodeURL, U body,
            GetBlockNumberSuccess<T> success, ErrorCallback errorCallback);

        void ReadMethod<T, U>(ContractInfo contractInfo, U body,
            ReadMethodSuccess<T> success, ErrorCallback errorCallback);

        public void WriteMethod<T, U>(ContractInfo contractInfo, string localAccountName, string gasPrice, string value,
            U body, WriteMethodSuccess<T> success, ErrorCallback errorCallback);
    }
}