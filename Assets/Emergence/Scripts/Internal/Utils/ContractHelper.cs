using UnityEngine;

namespace EmergenceSDK
{
    public static class ContractHelper
    {
        public delegate void GenericError(string message, long code);

        public delegate void LoadContractSuccess();
        public static void LoadContract(string contractAddress, string ABI, string network, LoadContractSuccess success, GenericError error)
        {
            Services.Instance.IsConnected((connected) =>
            {
                if (connected)
                {
                    Services.Instance.LoadContract(contractAddress, ABI, network,() =>
                        {
                            success?.Invoke();
                        },
                        (errorMessage, code) =>
                        {
                            Debug.LogError("[" + code + "] " + errorMessage);
                            error?.Invoke(errorMessage, code);
                        });
                }
                else
                {
                    Debug.LogError("Read contract wallet not connected");
                    error?.Invoke("Wallet not connected", 1);
                }
            },
            (errorMessage, code) =>
            {
                Debug.LogError("[" + code + "] " + errorMessage);
                error?.Invoke(errorMessage, code);
            });
        }

        public delegate void ReadContractSuccess<T>(T response);
        public static void ReadMethod<T, U>(string contractAddress, string methodName, string network, string nodeUrl, U body, ReadContractSuccess<T> success, GenericError error)
        {
            Services.Instance.IsConnected((connected) =>
            {
                if (connected)
                {
                    Services.Instance.ReadMethod<T, U>(contractAddress, methodName, network, nodeUrl, body, (response) =>
                    {
                        success?.Invoke(response);
                    },
                    (errorMessage, code) =>
                    {
                        error?.Invoke(errorMessage, code);
                    });
                }
                else
                {
                    Debug.LogError("Read Method wallet not connected");
                    error?.Invoke("Wallet not connected", 1);
                }
            },
            (errorMessage, code) =>
            {
                error?.Invoke(errorMessage, code);
            });
        }

        public delegate void WriteContractSuccess<T>(T response);
        public static void WriteMethod<T, U>(string contractAddress, string methodName, string localAccountName, string gasprice, string network, string nodeUrl, U body, WriteContractSuccess<T> success, GenericError error, string value = "0")
        {
            Services.Instance.IsConnected((connected) =>
            {
                if (connected)
                {
                    Services.Instance.WriteMethod<T, U>(contractAddress, methodName, localAccountName, gasprice, network, nodeUrl, value, body, (response) =>
                    {
                        success?.Invoke(response);
                    },
                    (errorMessage, code) =>
                    {
                        error?.Invoke(errorMessage, code);
                    });
                }
                else
                {
                    Debug.LogError("Write Method wallet not connected");
                    error?.Invoke("Wallet not connected", 1);
                }
            },
            (errorMessage, code) =>
            {
                error?.Invoke(errorMessage, code);
            });
        }

        public delegate void GetTransactionStatusSuccess<T>(T response);
        public static void GetTransactionStatus<T>(string transactionHash, string nodeURL,  GetTransactionStatusSuccess<T> success, GenericError error)
        {
            Services.Instance.IsConnected((connected) =>
            {
                if (connected)
                {
                    Services.Instance.GetTransactionStatus<T>(transactionHash, nodeURL, (response) =>
                    {
                        success?.Invoke(response);
                    },
                    (errorMessage, code) =>
                    {
                        error?.Invoke(errorMessage, code);
                    });
                }
                else
                {
                    Debug.LogError("Read contract wallet not connected");
                    error?.Invoke("Wallet not connected", 1);
                }
            },
            (errorMessage, code) =>
            {
                error?.Invoke(errorMessage, code);
            });
        }

    }
}