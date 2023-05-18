using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class ContractService : IContractService
    {
        private List<string> loadedAddresses = new List<string>();

        private async UniTask<bool> LoadContract(string contractAddress, string ABI, string network, LoadContractSuccess success, ErrorCallback errorCallback)
        {
            Contract data = new Contract()
            {
                contractAddress = contractAddress,
                ABI = ABI,
                network = network,
            };

            WWWForm form = new WWWForm();
            form.AddField("contractAddress", contractAddress);
            form.AddField("ABI", ABI);
            form.AddField("network", network);

            string dataString = SerializationHelper.Serialize(data, false);
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "loadContract";

            using (UnityWebRequest request = UnityWebRequest.Post(url, ""))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(dataString);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.uploadHandler.contentType = "application/json";
                request.downloadHandler = new DownloadHandlerBuffer();

                try
                {
                    await request.SendWebRequest().ToUniTask();
                }
                catch (Exception e)
                {
                    errorCallback?.Invoke(e.Message, e.HResult);
                }

                EmergenceUtils.PrintRequestResult("Load Contract", request);

                if (EmergenceUtils.ProcessRequest<LoadContractResponse>(request, errorCallback, out var response))
                {
                    loadedAddresses.Add(contractAddress);
                    success?.Invoke();
                }
            }

            return loadedAddresses.Contains(contractAddress);
        }

        private bool CheckForNewContract(ContractInfo contractInfo) => !loadedAddresses.Contains(contractInfo.ContractAddress);

        public async UniTask ReadMethod<T>(ContractInfo contractInfo, string body, ReadMethodSuccess<T> success, ErrorCallback errorCallback)
        {
            if (CheckForNewContract(contractInfo))
            {
                bool loadedSuccessfully = await LoadContract(contractInfo.ContractAddress, contractInfo.ABI, contractInfo.Network, null, errorCallback);
                if (!loadedSuccessfully)
                {
                    errorCallback?.Invoke("Error loading contract", -1);
                    return;
                }
            }

            string url = contractInfo.ToReadUrl();

            using (UnityWebRequest request = UnityWebRequest.Post(url, ""))
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
                request.uploadHandler.contentType = "application/json";

                try
                {
                    await request.SendWebRequest().ToUniTask();
                }
                catch (Exception e)
                {
                    errorCallback?.Invoke(e.Message, e.HResult);
                }

                EmergenceUtils.PrintRequestResult("Read Contract", request);

                if (EmergenceUtils.ProcessRequest<T>(request, errorCallback, out var response))
                {
                    success?.Invoke(response);
                }
            }
        }

        public async UniTask WriteMethod<T>(ContractInfo contractInfo, string localAccountName, string gasPrice, string value, string body, WriteMethodSuccess<T> success, ErrorCallback errorCallback)
        {
            if (CheckForNewContract(contractInfo))
            {
                bool loadedSuccessfully = await LoadContract(contractInfo.ContractAddress, contractInfo.ABI, contractInfo.Network, null, errorCallback);
                if (!loadedSuccessfully)
                {
                    errorCallback?.Invoke("Error loading contract", -1);
                    return;
                }
            }

            string gasPriceString = String.Empty;
            string localAccountNameString = String.Empty;

            if (!string.IsNullOrEmpty(gasPrice) && !string.IsNullOrEmpty(localAccountName))
            {
                gasPriceString = "&gasPrice=" + gasPrice;
                localAccountNameString = "&localAccountName=" + localAccountName;
            }

            string url = contractInfo.ToWriteUrl(localAccountNameString, gasPriceString, value);

            using (UnityWebRequest request = UnityWebRequest.Post(url, ""))
            {
                request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
                request.uploadHandler.contentType = "application/json";

                try
                {
                    await request.SendWebRequest().ToUniTask();
                }
                catch (Exception e)
                {
                    errorCallback?.Invoke(e.Message, e.HResult);
                }

                EmergenceUtils.PrintRequestResult("Write Contract", request);

                if (EmergenceUtils.ProcessRequest<T>(request, errorCallback, out var response))
                {
                    success?.Invoke(response);
                }
            }
        }
    }
}