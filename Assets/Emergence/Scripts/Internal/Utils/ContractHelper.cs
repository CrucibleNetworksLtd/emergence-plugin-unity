using EmergenceSDK.Services;
using UnityEngine;

namespace EmergenceSDK.Internal.Utils
{
    public class ContractHelper
    {
        public delegate void GenericError(string message, long code);

        public delegate void LoadContractSuccess();
        
        private static ContractHelper Instance => instance ??= new ContractHelper();
        private static ContractHelper instance;
        private readonly IAccountService accountService;
        private readonly IContractService contractService;

        public ContractHelper()
        {
            accountService = EmergenceServices.GetService<IAccountService>();
            contractService = EmergenceServices.GetService<IContractService>();
        }

        public static void LoadContract(string contractAddress, string ABI, string network, LoadContractSuccess success, GenericError error)
        {
            Instance.accountService.IsConnected(IsConnectedSuccess, (errorMessage, code) => error?.Invoke(errorMessage, code));
            
            void IsConnectedSuccess(bool connected)
            {
                if (connected)
                {
                    Instance.contractService.LoadContract(contractAddress, ABI, network,
                        () => success?.Invoke(), (errorMessage, code) => error?.Invoke(errorMessage, code));
                }
                else
                {
                    error?.Invoke("Wallet not connected", 1);
                }
            }
        }

        public delegate void ReadContractSuccess<T>(T response);
        public static void ReadMethod<T, U>(ContractInfo contractInfo, U body, ReadContractSuccess<T> success, GenericError error)
        {
            Instance.accountService.IsConnected(IsConnectedSuccess,(errorMessage, code) => error?.Invoke(errorMessage, code));
            
            void IsConnectedSuccess(bool connected)
            {
                if (connected)
                {
                    Instance.contractService.ReadMethod<T, U>(contractInfo, body, 
                        (response) => success?.Invoke(response),
                        (errorMessage, code) => error?.Invoke(errorMessage, code));
                }
                else
                {
                    error?.Invoke("Wallet not connected", 1);
                }
            }
        }

        public delegate void WriteContractSuccess<T>(T response);
        public static void WriteMethod<T, U>(ContractInfo contractInfo, string localAccountName, string gasprice,  U body, WriteContractSuccess<T> success, GenericError error, string value = "0")
        {
            Instance.accountService.IsConnected(IsConnectedSuccess, (errorMessage, code) => error?.Invoke(errorMessage, code));
            
            void IsConnectedSuccess(bool connected)
            {
                if (connected)
                {
                    Instance.contractService.WriteMethod<T, U>(contractInfo, localAccountName, gasprice, value, body,
                        (response) => success?.Invoke(response), (errorMessage, code) => error?.Invoke(errorMessage, code));
                }
                else
                {
                    error?.Invoke("Wallet not connected", 1);
                }
            }
        }

        public delegate void GetTransactionStatusSuccess<T>(T response);
        public static void GetTransactionStatus<T>(string transactionHash, string nodeURL,  GetTransactionStatusSuccess<T> success, GenericError error)
        {
            Instance.accountService.IsConnected(IsConnectedSuccess, (errorMessage, code) => error?.Invoke(errorMessage, code));
            
            void IsConnectedSuccess(bool connected)
            {
                if (connected)
                {
                    Instance.contractService.GetTransactionStatus<T>(transactionHash, nodeURL, 
                        (response) => success?.Invoke(response), (errorMessage, code) => error?.Invoke(errorMessage, code));
                }
                else
                {
                    error?.Invoke("Wallet not connected", 1);
                }
            }
        }

    }
}