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

        void ReadMethod<T, U>(string contractAddress, string methodName, string network, string nodeUrl, U body,
            ReadMethodSuccess<T> success, ErrorCallback errorCallback);

        public void WriteMethod<T, U>(string contractAddress, string methodName, string localAccountName,
            string gasPrice, string network, string nodeUrl, string value, U body, WriteMethodSuccess<T> success,
            ErrorCallback errorCallback);
    }
}