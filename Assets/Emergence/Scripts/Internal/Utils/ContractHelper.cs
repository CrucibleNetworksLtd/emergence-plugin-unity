using EmergenceSDK.Services;
using UnityEngine;

namespace EmergenceSDK.Internal.Utils
{
    public static class ContractHelper
    {
        public delegate void GenericError(string message, long code);

        public delegate void LoadContractSuccess();
        public static void LoadContract(string contractAddress, string ABI, string network, LoadContractSuccess success, GenericError error)
        {
            EmergenceServices.Instance.IsConnected(IsConnectedSuccess, (errorMessage, code) => error?.Invoke(errorMessage, code));
            
            void IsConnectedSuccess(bool connected)
            {
                if (connected)
                {
                    EmergenceServices.Instance.LoadContract(contractAddress, ABI, network,
                        () => success?.Invoke(), (errorMessage, code) => error?.Invoke(errorMessage, code));
                }
                else
                {
                    error?.Invoke("Wallet not connected", 1);
                }
            }
        }

        public delegate void ReadContractSuccess<T>(T response);
        public static void ReadMethod<T, U>(string contractAddress, string methodName, string network, string nodeUrl, U body, ReadContractSuccess<T> success, GenericError error)
        {
            EmergenceServices.Instance.IsConnected(IsConnectedSuccess,(errorMessage, code) => error?.Invoke(errorMessage, code));
            
            void IsConnectedSuccess(bool connected)
            {
                if (connected)
                {
                    EmergenceServices.Instance.ReadMethod<T, U>(contractAddress, methodName, network, nodeUrl, body, 
                        (response) => success?.Invoke(response), (errorMessage, code) => error?.Invoke(errorMessage, code));
                }
                else
                {
                    error?.Invoke("Wallet not connected", 1);
                }
            }
        }

        public delegate void WriteContractSuccess<T>(T response);
        public static void WriteMethod<T, U>(string contractAddress, string methodName, string localAccountName, string gasprice, string network, string nodeUrl, U body, WriteContractSuccess<T> success, GenericError error, string value = "0")
        {
            EmergenceServices.Instance.IsConnected(IsConnectedSuccess, (errorMessage, code) => error?.Invoke(errorMessage, code));
            
            void IsConnectedSuccess(bool connected)
            {
                if (connected)
                {
                    EmergenceServices.Instance.WriteMethod<T, U>(contractAddress, methodName, localAccountName, gasprice, network, nodeUrl, value, body,
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
            EmergenceServices.Instance.IsConnected(IsConnectedSuccess, (errorMessage, code) => error?.Invoke(errorMessage, code));
            
            void IsConnectedSuccess(bool connected)
            {
                if (connected)
                {
                    EmergenceServices.Instance.GetTransactionStatus<T>(transactionHash, nodeURL, 
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