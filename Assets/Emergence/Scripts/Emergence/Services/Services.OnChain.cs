using System;
using System.Collections;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace EmergenceSDK.Services
{
    public partial class EmergenceServices
    {
        public void IsConnected(IsConnectedSuccess success, ErrorCallback errorCallback) => AccountService.IsConnected(success, errorCallback);
        
        public void ReinitializeWalletConnect(ReinitializeWalletConnectSuccess success, ErrorCallback errorCallback) => WalletService.ReinitializeWalletConnect(success, errorCallback);

        public void RequestToSign(string messageToSign, RequestToSignSuccess success, ErrorCallback errorCallback) => WalletService.RequestToSign(messageToSign, success, errorCallback);

        public void GetQRCode(QRCodeSuccess success, ErrorCallback errorCallback) => QRCodeService.GetQRCode(success, errorCallback);

        public void Handshake(HandshakeSuccess success, ErrorCallback errorCallback) => WalletService.Handshake(success, errorCallback);

        public void CreateWallet(string path, string password, CreateWalletSuccess success, ErrorCallback errorCallback) => WalletService.CreateWallet(path, password, success, errorCallback);

        public void CreateKeyStore(string privateKey, string password, string publicKey, string path,
            CreateKeyStoreSuccess success, ErrorCallback errorCallback) 
            => AccountService.CreateKeyStore(privateKey, password, publicKey, path, success, errorCallback);

        public void LoadAccount(string name, string password, string path, string nodeURL, string chainId,
            LoadAccountSuccess success, ErrorCallback errorCallback)
        {
            AccountService.LoadAccount(name, password, path, nodeURL, chainId, success, errorCallback);
        }

        public void GetBalance(BalanceSuccess success, ErrorCallback errorCallback)
        {
            if (skipWallet)
            {
                success?.Invoke("No wallet");
                return;
            }
            WalletService.GetBalance(success, errorCallback);
        }

        public void GetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback) => AccountService.GetAccessToken(success, errorCallback);

        public void ValidateAccessToken(ValidateAccessTokenSuccess success, ErrorCallback errorCallback) => AccountService.ValidateAccessToken(success, errorCallback);

        public void ValidateSignedMessage(string message, string signedMessage, string address,
            ValidateSignedMessageSuccess success, ErrorCallback errorCallback)
            => AccountService.ValidateSignedMessage(message, signedMessage, address, success, errorCallback);

        public void Disconnect(DisconnectSuccess success, ErrorCallback errorCallback)
        {
            if (skipWallet)
            {
                success?.Invoke();
                return;
            }
            AccountService.Disconnect(success, errorCallback);
        }

        public void Finish(SuccessFinish success, ErrorCallback errorCallback) => AccountService.Finish(success, errorCallback);

        #region Contracts

        #region Load Contract

        public delegate void LoadContractSuccess();

        public void LoadContract(string contractAddress, string ABI, string network, LoadContractSuccess success,
            ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineLoadContract(contractAddress, ABI, network, success, errorCallback));
        }

        public IEnumerator CoroutineLoadContract(string contractAddress, string ABI, string network,
            LoadContractSuccess success, ErrorCallback errorCallback)
        {
            Debug.Log("LoadContract request started");

            Debug.Log("ABI: " + ABI);

            Contract data = new Contract()
            {
                contractAddress = contractAddress,
                ABI = ABI,
                network = network,
            };

            /// {"Categories":["Weapons","Clothing"]}

            WWWForm form = new WWWForm();
            form.AddField("contractAddress", contractAddress);
            form.AddField("ABI", ABI);
            form.AddField("network", network);

            string dataString = SerializationHelper.Serialize(data, false);
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "loadContract";

            Debug.Log("payload: " + System.Text.RegularExpressions.Regex.Unescape(dataString));
            Debug.Log("Load contract url: " + url);

            using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(dataString);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.uploadHandler.contentType = "application/json";
                request.downloadHandler = new DownloadHandlerBuffer();
                yield return request.SendWebRequest();
                PrintRequestResult("Load Contract", request);
                if (ProcessRequest<LoadContractResponse>(request, errorCallback, out var response))
                {
                    success?.Invoke();
                }
            }
        }

        #endregion Load Contract

        #region GetTransactionStatus

        public delegate void GetTransactionStatusSuccess<T>(T response);

        public void GetTransactionStatus<T>(string transactionHash, string nodeURL,
            GetTransactionStatusSuccess<T> success, ErrorCallback errorCallback)
        {

            StartCoroutine(CoroutineGetTransactionStatus<T>(transactionHash, nodeURL, success, errorCallback));
        }

        private IEnumerator CoroutineGetTransactionStatus<T>(string transactionHash, string nodeURL,
            GetTransactionStatusSuccess<T> success, ErrorCallback errorCallback)
        {
            Debug.Log("Get Transaction Status request started [" + transactionHash + "] / " + nodeURL);

            string url = EmergenceSingleton.Instance.Configuration.APIBase + "GetTransactionStatus?transactionHash=" +
                         transactionHash + "&nodeURL=" + nodeURL;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                PrintRequestResult("Get Transaction Status", request);
                if (ProcessRequest<T>(request, errorCallback, out var response))
                {
                    success?.Invoke(response); // Should we change this pattern?
                }
            }
        }

        #endregion

        #region GetBlockNumber

        public delegate void GetBlockNumberSuccess<T>(T response);

        public void GetBlockNumber<T, U>(string transactionHash, string nodeURL, U body,
            GetBlockNumberSuccess<T> success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineGetBlockNumber<T, U>(transactionHash, nodeURL, body, success, errorCallback));
        }

        private IEnumerator CoroutineGetBlockNumber<T, U>(string transactionHash, string nodeURL, U body,
            GetBlockNumberSuccess<T> success, ErrorCallback errorCallback)
        {
            Debug.Log("Get Block Number request started [" + transactionHash + "] / " + nodeURL);

            string url = EmergenceSingleton.Instance.Configuration.APIBase + "getBlockNumber?nodeURL=" + nodeURL;

            string dataString = SerializationHelper.Serialize(body, false);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                PrintRequestResult("Get Block Number", request);
                if (ProcessRequest<T>(request, errorCallback, out var response))
                {
                    success?.Invoke(response);
                }
            }
        }

        #endregion


        #region Read Method

        public delegate void ReadMethodSuccess<T>(T response);

        public void ReadMethod<T, U>(string contractAddress, string methodName, string network, string nodeUrl, U body, ReadMethodSuccess<T> success,
            ErrorCallback errorCallback)
        {
            // if (!LocalEmergenceServer.Instance.CheckEnv())
            // {
            //     return;
            // }

            StartCoroutine(CoroutineReadMethod<T, U>(contractAddress, methodName,  network, nodeUrl, body, success, errorCallback));
        }

        public IEnumerator CoroutineReadMethod<T, U>(string contractAddress, string methodName, string network, string nodeUrl, U body,
            ReadMethodSuccess<T> success, ErrorCallback errorCallback)
        {
            Debug.Log("ReadMethod request started [" + contractAddress + "] / " + methodName);

            string url = EmergenceSingleton.Instance.Configuration.APIBase + "readMethod?contractAddress=" + contractAddress + "&methodName=" + methodName + "&nodeUrl=" + nodeUrl + "&network=" + network;

            Debug.Log("ReadMethod url: " + url);

            string dataString = SerializationHelper.Serialize(body, false);

            Debug.Log("Data string: " + dataString);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                PrintRequestResult("Read Contract", request);
                if (ProcessRequest<T>(request, errorCallback, out var response))
                {
                    success?.Invoke(response);
                }
            }
        }

        #endregion Read Contract

        #region Write Method

        public delegate void WriteMethodSuccess<T>(T response);

        public void WriteMethod<T, U>(string contractAddress, string methodName, string localAccountName,
            string gasPrice, string network, string nodeUrl, string value, U body, WriteMethodSuccess<T> success, ErrorCallback errorCallback)
        {
            // if (!LocalEmergenceServer.Instance.CheckEnv())
            // {
            //     return;
            // }

            StartCoroutine(CoroutineWriteMethod<T, U>(contractAddress, methodName, localAccountName, gasPrice, network, nodeUrl, value, body,
                success, errorCallback));
        }

        public IEnumerator CoroutineWriteMethod<T, U>(string contractAddress, string methodName,
            string localAccountName, string gasPrice, string network, string nodeUrl, string value, U body, WriteMethodSuccess<T> success, ErrorCallback errorCallback)
        {
            Debug.Log("WriteContract request started [" + contractAddress + "] / " + methodName);

            string gasPriceString = String.Empty;
            string localAccountNameString = String.Empty;

            if (gasPrice != String.Empty && localAccountName != String.Empty)
            {
                gasPriceString = "&gasPrice=" + gasPrice;
                localAccountNameString = "&localAccountName=" + localAccountName;
            }

            string url = EmergenceSingleton.Instance.Configuration.APIBase + "writeMethod?contractAddress=" +
                         contractAddress + "&methodName=" + methodName + localAccountNameString + gasPriceString +
                         "&network=" + network + "&nodeUrl=" + nodeUrl + "&value=" + value;

            Debug.Log("WriteMethod url: " + url);

            string dataString = SerializationHelper.Serialize(body, false);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
                request.uploadHandler.contentType = "application/json";

                yield return request.SendWebRequest();
                PrintRequestResult("Write Contract", request);
                if (ProcessRequest<T>(request, errorCallback, out var response))
                {
                    success?.Invoke(response);
                }
            }
        }

        #endregion Write Contract

        #endregion Contracts
    }
}