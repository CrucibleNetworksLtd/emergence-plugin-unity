using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static EmergenceSDK.Services;

namespace EmergenceSDK
{
    public class BlockchainService : SingletonComponent<BlockchainService>
    {
        public delegate void SuccessLoadContract();

        public void LoadContract(string contractAddress, string aBI)
        {

        }
        private IEnumerator CoroutineLoadContract(string contractAddress, string aBI, SuccessLoadContract success, GenericError error)
        {
            Debug.Log("Load Contract request started");
            string jsonPersona = SerializationHelper.Serialize(persona);

            string url = envValues.databaseAPIPrivate + "loadContract";

            using (UnityWebRequest request = UnityWebRequest.Post(url, string.Empty))
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPersona));
                request.uploadHandler.contentType = "application/json";

                request.SetRequestHeader("Authorization", currentAccessToken);

                yield return request.SendWebRequest();
                PrintRequestResult("Load Contract", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    success?.Invoke();
                }
            }
        }
    }
}
