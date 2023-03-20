using System;
using System.Collections;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    public class ContractService : MonoBehaviour, IContractService
    {
        public void LoadContract(string contractAddress, string ABI, string network, LoadContractSuccess success,
            ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineLoadContract(contractAddress, ABI, network, success, errorCallback));
        }

        public IEnumerator CoroutineLoadContract(string contractAddress, string ABI, string network,
            LoadContractSuccess success, ErrorCallback errorCallback)
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

            using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(dataString);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.uploadHandler.contentType = "application/json";
                request.downloadHandler = new DownloadHandlerBuffer();
                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("Load Contract", request);
                if (EmergenceUtils.ProcessRequest<LoadContractResponse>(request, errorCallback, out var response))
                {
                    success?.Invoke();
                }
            }
        }

        public void GetTransactionStatus<T>(string transactionHash, string nodeURL,
            GetTransactionStatusSuccess<T> success, ErrorCallback errorCallback)
        {

            StartCoroutine(CoroutineGetTransactionStatus<T>(transactionHash, nodeURL, success, errorCallback));
        }

        private IEnumerator CoroutineGetTransactionStatus<T>(string transactionHash, string nodeURL,
            GetTransactionStatusSuccess<T> success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "GetTransactionStatus?transactionHash=" +
                         transactionHash + "&nodeURL=" + nodeURL;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("Get Transaction Status", request);
                if (EmergenceUtils.ProcessRequest<T>(request, errorCallback, out var response))
                {
                    success?.Invoke(response); // Should we change this pattern?
                }
            }
        }

        public void GetBlockNumber<T, U>(string transactionHash, string nodeURL, U body,
            GetBlockNumberSuccess<T> success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineGetBlockNumber<T, U>(transactionHash, nodeURL, body, success, errorCallback));
        }

        private IEnumerator CoroutineGetBlockNumber<T, U>(string transactionHash, string nodeURL, U body,
            GetBlockNumberSuccess<T> success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "getBlockNumber?nodeURL=" + nodeURL;

            string dataString = SerializationHelper.Serialize(body, false);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("Get Block Number", request);
                if (EmergenceUtils.ProcessRequest<T>(request, errorCallback, out var response))
                {
                    success?.Invoke(response);
                }
            }
        }

        public void ReadMethod<T, U>(ContractInfo contractInfo, U body, ReadMethodSuccess<T> success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineReadMethod<T, U>(contractInfo, body, success, errorCallback));
        }

        public IEnumerator CoroutineReadMethod<T, U>(ContractInfo contractInfo, U body, ReadMethodSuccess<T> success, ErrorCallback errorCallback)
        {
            string url = contractInfo.ToReadUrl();

            string dataString = SerializationHelper.Serialize(body, false);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("Read Contract", request);
                if (EmergenceUtils.ProcessRequest<T>(request, errorCallback, out var response))
                {
                    success?.Invoke(response);
                }
            }
        }

        public void WriteMethod<T, U>(ContractInfo contractInfo, string localAccountName, string gasPrice, string value, U body, WriteMethodSuccess<T> success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineWriteMethod<T, U>(contractInfo, localAccountName, gasPrice, value, body, success, errorCallback));
        }

        public IEnumerator CoroutineWriteMethod<T, U>(ContractInfo contractInfo, string localAccountName, string gasPrice, string value, 
            U body, WriteMethodSuccess<T> success, ErrorCallback errorCallback)
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

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("Write Contract", request);
                if (EmergenceUtils.ProcessRequest<T>(request, errorCallback, out var response))
                {
                    success?.Invoke(response);
                }
            }
        }
    }
}