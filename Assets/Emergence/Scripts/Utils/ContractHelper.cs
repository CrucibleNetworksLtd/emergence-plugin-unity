using UnityEngine;

namespace EmergenceSDK
{
    public static class ContractHelper
    {
        public delegate void GenericError(string message, long code);

        public delegate void LoadContractSuccess();
        public static void LoadContract(string contractAddress, string ABI, LoadContractSuccess success, GenericError error)
        {
            Services.Instance.IsConnected((connected) =>
            {
                if (connected)
                {
                    Services.Instance.LoadContract(contractAddress, ABI, () =>
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
        public static void ReadContract<T, U>(string contractAddress, string methodName, U body, ReadContractSuccess<T> success, GenericError error)
        {
            Services.Instance.IsConnected((connected) =>
            {
                if (connected)
                {
                    Services.Instance.ReadContract<T, U>(contractAddress, methodName, body, (response) =>
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

        public delegate void WriteContractSuccess<T>(T response);
        public static void WriteContract<T, U>(string contractAddress, string methodName, string localAccountName, string gasprice, U body, WriteContractSuccess<T> success, GenericError error)
        {
            Services.Instance.IsConnected((connected) =>
            {
                if (connected)
                {
                    Services.Instance.WriteContract<T, U>(contractAddress, methodName, localAccountName, gasprice, body, (response) =>
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
                    Debug.LogError("Write contract wallet not connected");
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