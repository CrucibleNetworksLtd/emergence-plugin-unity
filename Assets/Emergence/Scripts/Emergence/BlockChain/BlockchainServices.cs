using EmergenceSDK.Internal.Utils;

namespace EmergenceSDK.BlockChain
{
    public class BlockchainService : SingletonComponent<BlockchainService>
    {


        // #region Get Block Number
        //
        // public delegate void GetBlockNumberSuccess<T>(T response);
        // public void GetBlockNumber<T>(string nodeURL, GetBlockNumberSuccess<T> success, GenericError error)
        // {
        //     StartCoroutine(CoroutineGetBlockNumber(nodeURL, success, error));
        // }
        //
        // public IEnumerator CoroutineGetBlockNumber<T>(string nodeURL, GetBlockNumberSuccess<T> success, GenericError error)
        // {
        //     Debug.Log("GetBlockNumber request started");
        //
        //     // string url = LocalEmergenceServer.Instance.Environment().APIBase + "getBlockNumber?nodeURL=" + nodeURL;
        //     string url = EmergenceSingleton.Instance.Configuration.APIBase + "getBlockNumber?nodeURL=" + nodeURL;
        //
        //     using (UnityWebRequest request = UnityWebRequest.Get(url))
        //     {
        //
        //         yield return request.SendWebRequest();
        //         PrintRequestResult("Get Block Number", request);
        //         if (Services.Instance.ProcessRequest<T>(request, error, out var response))
        //         {
        //             success?.Invoke(response);
        //         }
        //     }
        // }
        //
        // #endregion Get Block Number




        // #region Contracts

        // #region Load Contract
        // public delegate void LoadContractSuccess();
        // public void LoadContract(string contractAddress, string ABI, LoadContractSuccess success, GenericError error)
        // {
        //     StartCoroutine(CoroutineLoadContract(contractAddress, ABI, success, error));
        // }
        //
        // public IEnumerator CoroutineLoadContract(string contractAddress, string ABI, LoadContractSuccess success, GenericError error)
        // {
        //     Debug.Log("LoadContract request started");
        //
        //     Contract data = new Contract()
        //     {
        //         contractAddress = contractAddress,
        //         ABI = ABI
        //     };
        //
        //     string dataString = SerializationHelper.Serialize(data, false);
        //     // string url = LocalEmergenceServer.Instance.Environment().APIBase + "loadContract";
        //     string url = EmergenceSingleton.Instance.Configuration.APIBase + "loadContract";
        //
        //     using (UnityWebRequest request = UnityWebRequest.Get(url))
        //     {
        //         request.method = "POST";
        //         request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
        //         request.uploadHandler.contentType = "application/json";
        //
        //         yield return request.SendWebRequest();
        //         PrintRequestResult("Load Contract", request);
        //         if (Services.Instance.ProcessRequest<LoadContractResponse>(request, error, out var response))
        //         {
        //             success?.Invoke();
        //         }
        //     }
        // }
        //
        // #endregion Load Contract
        //
        //
        // #region Read Contract
        //
        // public delegate void ReadContractSuccess<T>(T response);
        // public void ReadContract<T, U>(string contractAddress, string methodName, U body, ReadContractSuccess<T> success, GenericError error)
        // {
        //     StartCoroutine(CoroutineReadContract<T, U>(contractAddress, methodName, body, success, error));
        // }
        //
        // public IEnumerator CoroutineReadContract<T, U>(string contractAddress, string methodName, U body, ReadContractSuccess<T> success, GenericError error)
        // {
        //     Debug.Log("ReadContract request started [" + contractAddress + "] / " + methodName);
        //     
        //     string url = LocalEmergenceServer.Instance.Environment().APIBase + "readMethod?contractAddress=" + contractAddress + "&methodName=" + methodName;
        //     url += "&nodeUrl=" + LocalEmergenceServer.Instance.Environment().DefaultNodeURL;
        //     
        //     string dataString = SerializationHelper.Serialize(body, false);
        //     if (body is string bodystr)
        //     {
        //         if (string.IsNullOrWhiteSpace(bodystr))
        //         {
        //             dataString = "[]";
        //         }
        //     }
        //
        //     using (UnityWebRequest request = UnityWebRequest.Get(url))
        //     {
        //         request.method = "POST";
        //         request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
        //         request.uploadHandler.contentType = "application/json";
        //
        //         yield return request.SendWebRequest();
        //         PrintRequestResult("Read Contract", request);
        //         if (Services.Instance.ProcessRequest<T>(request, error, out var response))
        //         {
        //             success?.Invoke(response);
        //         }
        //     }
        // }
        //
        // #endregion Read Contract
        //
        // #region Write Contract
        //
        // public delegate void WriteContractSuccess<T>(T response);
        // public void WriteContract<T, U>(string contractAddress, string methodName, string localAccountName, string gasPrice, U body, WriteContractSuccess<T> success, GenericError error)
        // {
        //     StartCoroutine(CoroutineWriteContract<T, U>(contractAddress, methodName, localAccountName, gasPrice, body, success, error));
        // }
        //
        // public IEnumerator CoroutineWriteContract<T, U>(string contractAddress, string methodName, string localAccountName, string gasPrice, U body, WriteContractSuccess<T> success, GenericError error)
        // {
        //     Debug.Log("WriteContract request started [" + contractAddress + "] / " + methodName);
        //
        //     string gasPriceString = String.Empty;
        //     string localAccountNameString = String.Empty;
        //
        //     if (gasPrice != String.Empty && localAccountName != String.Empty)
        //     {
        //         gasPriceString = "&gasPrice=" + gasPrice;
        //         localAccountNameString = "&localAccountName=" + localAccountName;
        //     }
        //
        //     string url = LocalEmergenceServer.Instance.Environment().APIBase + "writeMethod?contractAddress=" + contractAddress + "&methodName=" + methodName + localAccountNameString + gasPriceString;
        //
        //     string dataString = SerializationHelper.Serialize(body, false);
        //     if (body is string bodystr)
        //     {
        //         if (string.IsNullOrWhiteSpace(bodystr))
        //         {
        //             dataString = "[]";
        //         }
        //     }
        //
        //     using (UnityWebRequest request = UnityWebRequest.Get(url))
        //     {
        //         request.method = "POST";
        //         request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
        //         request.uploadHandler.contentType = "application/json";
        //
        //         yield return request.SendWebRequest();
        //         PrintRequestResult("Write Contract", request);
        //         if (Services.Instance.ProcessRequest<T>(request, error, out var response))
        //         {
        //             success?.Invoke(response);
        //         }
        //     }
        // }
        //
        // #endregion Write Contract
        //
        // #endregion Contracts

        // #region Wallets
        // public void CreateWalletAndKeystoreFile(string path, string password, CreateWalletSuccess success, GenericError error)
        // {
        //     Services.Instance.CreateWallet(path, password, success, error);
        // }
        //
        // public void CreateKeystoreFile(string privateKey, string password, string publicKey, string path, CreateKeyStoreSuccess success, GenericError error)
        // {
        //     Services.Instance.CreateKeyStore(privateKey, password, publicKey, path, success, error);
        // }
        //
        //
        // public void LoadAccountFromKeyStoreFile(string name, string password, string path, string nodeurl, string chainID, LoadAccountSuccess success, GenericError error)
        // {
        //     Services.Instance.LoadAccount(name, password, path, nodeurl, chainID, success, error);
        // }
        //
        //
        //
        // #endregion

        //
        // #region Signing
        // public void RequestToSign(string message, RequestToSignSuccess success, GenericError error)
        // {
        //     Services.Instance.RequestToSignWalletConnect(message, success, error);
        //
        // }
        //
        //
        //
        // #endregion Signing

    }
}
