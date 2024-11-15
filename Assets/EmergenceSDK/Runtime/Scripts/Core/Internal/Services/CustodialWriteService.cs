using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Futureverse;
using EmergenceSDK.Runtime.Internal.Services;
using EmergenceSDK.Runtime.Internal.Types;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Device;

namespace EmergenceSDK.Runtime.Types
{
    /// <summary>
    /// Service for performing custodial write operations (transaction encoding, signing and sending).
    /// </summary>
    public class CustodialWriteService : ICustodialWriteService
    {
        private Dictionary<string, ContractInfo> rawTransactionWithoutSignature;

        private static string fvHelperServiceUrl = "https://fvhelperservice.openmeta.xyz/";
        private static string getEncodedDataEndpoint = "getEncodedData";
        private static string encodeTransactionEndpoint = "encode-transaction";
        private static string sendTransactionEndpoint = "send-transaction";
        
        private string signerServiceUrl => FutureverseSingleton.Instance.Environment switch
        {
            EmergenceEnvironment.Development => "Staging",
            EmergenceEnvironment.Staging => "Staging",
            EmergenceEnvironment.Production => "Production",
            _ => throw new ArgumentOutOfRangeException(nameof(EmergenceSingleton.Instance.Environment), "Unknown environment")
        };

        /// <summary>
        /// Perform the custodial write method by encoding data, requesting a transaction signature, and sending the transaction.
        /// </summary>
        /// <param name="contractInfo">The contract information.</param>
        /// <param name="inputValue">The value to send with the transaction.</param>
        /// <param name="dataToEncode">The data to encode for the transaction.</param>
        /// <returns>A task representing the asynchronous operation, with a WebResponse as the result.</returns>
        public async UniTask<WebResponse> PerformCustodialWriteMethod(ContractInfo contractInfo, string inputValue, string dataToEncode)
        {
            var eoa = EmergenceServiceProvider.GetService<CustodialLoginService>().CachedAccessTokenResponse.DecodedToken.Eoa; // Pull the cached EOA
            
            string encodedData = await EncodedData(contractInfo, dataToEncode);
            var helperResponse = await EncodeTransaction(eoa, encodedData, contractInfo, inputValue);
            return await RequestTransactionSignature(helperResponse, contractInfo, eoa);
        }

        /// <summary>
        /// Encodes the transaction data to be used in the write operation.
        /// </summary>
        /// <param name="contractInfo">The contract information.</param>
        /// <param name="data">The data to encode.</param>
        /// <returns>A task representing the asynchronous operation, with the encoded data as a string.</returns>
        private async UniTask<string> EncodedData(ContractInfo contractInfo, string data)
        {
            JArray dataJArray = JArray.Parse(data);
            
            CustodialTransactionToEncode jsonToSend = new CustodialTransactionToEncode
            {
                ABI = contractInfo.ABI,
                ContractAddress = contractInfo.ContractAddress,
                MethodName = contractInfo.MethodName,
                CallInputs = dataJArray
            };
            
            string outputString = JsonConvert.SerializeObject(jsonToSend);
            string url = StaticConfig.APIBase + getEncodedDataEndpoint;
            var headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Post, url, outputString, headers);
            return JsonConvert.DeserializeObject<EncodedCustodialTransactionData>(response.ResponseText).Message["Data"];
        }

        /// <summary>
        /// Encodes the transaction by sending data to the FV helper service.
        /// </summary>
        /// <param name="eoa">The externally owned account (EOA) of the user.</param>
        /// <param name="encodedData">The encoded data for the transaction.</param>
        /// <param name="contractInfo">The contract information.</param>
        /// <param name="inputValue">The value for the transaction.</param>
        /// <returns>A task representing the asynchronous operation, with the FVHelperResponse as the result.</returns>
        private async UniTask<FVHelperResponse> EncodeTransaction(string eoa, string encodedData, ContractInfo contractInfo, string inputValue)
        {
            CustodialTransaction transaction = new CustodialTransaction
            {
                Eoa = eoa,
                ChainId = contractInfo.ChainId.ToString(),
                ToAddress = contractInfo.ContractAddress,
                Value = inputValue,
                Data = encodedData,
                RpcUrl = contractInfo.NodeUrl,
                Environment = signerServiceUrl
            };
            string stringifiedTransaction = JsonConvert.SerializeObject(transaction);
            string url = fvHelperServiceUrl + encodeTransactionEndpoint;
            var headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            
            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Post, url, stringifiedTransaction, headers);
            return JsonConvert.DeserializeObject<FVHelperResponse>(response.ResponseText);
        }

        /// <summary>
        /// Requests the transaction signature from the FV helper service and processes the result.
        /// </summary>
        /// <param name="fvHelperResponse">The response from the FV helper service containing the signing URL.</param>
        /// <param name="contractInfo">The contract information.</param>
        /// <param name="eoa">The externally owned account (EOA) of the user.</param>
        /// <returns>A task representing the asynchronous operation, with a WebResponse as the result.</returns>
        private async UniTask<WebResponse> RequestTransactionSignature(FVHelperResponse fvHelperResponse, ContractInfo contractInfo, string eoa)
        {
            WebResponse sendTransactionResponse = null;
            var tcs = new UniTaskCompletionSource<bool>();// Used to hold thread whilst we wait for callback server
            
            // We assign a lambda callback function to our signing http listener which will consume the response and use it to send our transaction once signed.
            CustodialLocalWebServerHelper.StartSigningServer("write-callback",
                async (string transactionCallbackResponse) =>
                {
                    sendTransactionResponse = await HandleTransactionCallback(contractInfo, transactionCallbackResponse, fvHelperResponse.RawTransactionWithoutSignature,eoa);
                    tcs.TrySetResult(true);
                });
            Application.OpenURL(fvHelperResponse.FullSignerUrl);
            await tcs.Task;
            return sendTransactionResponse; // Web response produced by our HandleTransactionCallback
        }

        /// <summary>
        /// Handles the transaction callback after the transaction has been signed.
        /// </summary>
        /// <param name="contractInfo">The contract information.</param>
        /// <param name="transactionCallbackDetails">The callback response details from the signing service.</param>
        /// <param name="rawTransaction">The raw transaction data.</param>
        /// <param name="eoa">The externally owned account (EOA) of the user.</param>
        /// <returns>A task representing the asynchronous operation, with a WebResponse as the result.</returns>
        private async UniTask<WebResponse> HandleTransactionCallback(ContractInfo contractInfo, string transactionCallbackDetails, RawTransaction rawTransaction, string eoa)
        {
            CustodialSignerServiceResponse response = JsonConvert.DeserializeObject<CustodialSignerServiceResponse>(transactionCallbackDetails);
            return await SendTransaction(contractInfo, response, rawTransaction, eoa);
        }

        /// <summary>
        /// Sends the signed transaction to the network.
        /// </summary>
        /// <param name="contractInfo">The contract information.</param>
        /// <param name="custodialSignerServiceResponse">The response from the signing service containing the transaction signature.</param>
        /// <param name="rawTransaction">The raw transaction data.</param>
        /// <param name="eoa">The externally owned account (EOA) of the user.</param>
        /// <returns>A task representing the asynchronous operation, with a WebResponse as the result.</returns>
        private async UniTask<WebResponse> SendTransaction(ContractInfo contractInfo, CustodialSignerServiceResponse custodialSignerServiceResponse, RawTransaction rawTransaction, string eoa)
        {
            CustodialTransactionData custodialTransactionData = new CustodialTransactionData
            {
                RawTransactionWithoutSignature = rawTransaction,
                TransactionSignature = custodialSignerServiceResponse.Result.Data.Signature,
                FromEoa = eoa,
                RpcUrl = contractInfo.NodeUrl
            };
            
            string stringifiedTransaction = JsonConvert.SerializeObject(custodialTransactionData);
            string url = fvHelperServiceUrl + sendTransactionEndpoint;
            
            var headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            await UniTask.SwitchToMainThread();
            return await WebRequestService.SendAsyncWebRequest(RequestMethod.Post, url, stringifiedTransaction, headers);
        }
    }
}
