using UnityEngine;

namespace Emergence
{
    public static class ContractHelper
    {
        public delegate void GenericError(string message, long code);

        public delegate void LoadContractSuccess();
        public static void LoadContract(string contractAddress, string ABI, LoadContractSuccess success, GenericError error)
        {
            NetworkManager.Instance.IsConnected((connected) =>
            {
                if (connected)
                {
                    NetworkManager.Instance.LoadContract(contractAddress, ABI, () =>
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
                    error?.Invoke("Wallet not connected", 0);
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
            NetworkManager.Instance.IsConnected((connected) =>
            {
                if (connected)
                {
                    NetworkManager.Instance.ReadContract<T, U>(contractAddress, methodName, body, (response) =>
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
                    error?.Invoke("Wallet not connected", 0);
                }
            },
            (errorMessage, code) =>
            {
                error?.Invoke(errorMessage, code);
            });
        }
    }
}