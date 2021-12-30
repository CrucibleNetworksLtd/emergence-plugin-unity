using UnityEngine;

namespace Emergence
{
    public class Ejemplo : MonoBehaviour
    {
        public MeshRenderer mr;
        private Material copy;

        void Start()
        {
            copy = mr.material;
        }

        private bool isRequesting = false;
        void Update()
        {
            if (!isRequesting && Input.GetKeyDown(KeyCode.N))
            {
                isRequesting = true;

                // TODO mover esto a script accesible por usuarios del SDK
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
            }
        }
    }
}