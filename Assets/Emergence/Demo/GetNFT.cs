using UnityEngine;

namespace Emergence
{
    public class GetNFT : MonoBehaviour
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
                if (NetworkManager.Instance.HasAccessToken)
                {
                    isRequesting = true;
                    ContractHelper.ReadContract(copy);
                }
            }
        }
    }
}