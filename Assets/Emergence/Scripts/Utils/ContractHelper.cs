using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Emergence
{
    public static class ContractHelper
    {
        static bool isRequesting = false;
        public static bool ReadContract(Material copy)
        {
            NetworkManager.Instance.LoadContract(() =>
            {
                NetworkManager.Instance.ReadContract<ReadContractTokenURIResponse>("tokenURI", (response) =>
                {
                    Debug.Log("NTF URL" + response.message.response);
                    NetworkManager.Instance.GetNFTMetadata(response.message.response, (textureURL) =>
                    {
                        RequestImage.Instance.AskForImage(textureURL, (url, texture) =>
                        {
                            copy.mainTexture = texture;
                            isRequesting = false;
                        },
                        (url, error, errorCode) =>
                        {
                                // TODO merge con el mensaje de error del otro branch
                                Debug.LogError("[" + errorCode + "] " + error);
                            isRequesting = false;
                        });
                    },
                    (error, code) =>
                    {
                        Debug.LogError("[" + code + "] " + error);
                        isRequesting = false;
                    });
                },
                (error, code) =>
                {
                    Debug.LogError("[" + code + "] " + error);
                    isRequesting = false;
                });

            },
          (error, code) =>
          {
              Debug.LogError("[" + code + "] " + error);
              isRequesting = false;
          });

            return isRequesting;
        }
    }
}