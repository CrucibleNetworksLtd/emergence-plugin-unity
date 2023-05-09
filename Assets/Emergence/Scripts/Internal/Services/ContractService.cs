using System;
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
        public async UniTask LoadContract(string contractAddress, string ABI, string network, LoadContractSuccess success, ErrorCallback errorCallback)
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
                    success?.Invoke();
                }
            }
        }

        public async UniTask ReadMethod<T, U>(ContractInfo contractInfo, U body, ReadMethodSuccess<T> success, ErrorCallback errorCallback)
        {
            string url = contractInfo.ToReadUrl();
            
            string dataString = SerializationHelper.Serialize(body, false);
            
            using (UnityWebRequest request = UnityWebRequest.Post(url, ""))
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
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

        public async UniTask WriteMethod<T, U>(ContractInfo contractInfo, string localAccountName, string gasPrice, string value, U body, WriteMethodSuccess<T> success, ErrorCallback errorCallback)
        {
            string gasPriceString = String.Empty;
            string localAccountNameString = String.Empty;

            if (gasPrice != String.Empty && localAccountName != String.Empty)
            {
                gasPriceString = "&gasPrice=" + gasPrice;
                localAccountNameString = "&localAccountName=" + localAccountName;
            }

            string url = contractInfo.ToWriteUrl(localAccountNameString, gasPriceString, value);

            string dataString = SerializationHelper.Serialize(body, false);

            using (UnityWebRequest request = UnityWebRequest.Post(url, ""))
            {
                request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
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