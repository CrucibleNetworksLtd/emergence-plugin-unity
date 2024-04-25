using Cysharp.Threading.Tasks;
using EmergenceSDK.Avatars;
using EmergenceSDK.Internal.Utils;
using UnityEngine;

namespace EmergenceSDK
{
    public class StrippedVRMLoader : MonoBehaviour
    {
        public GameObject playerPrefab;
        public string[] vrmUrls;
        public float offset = 3f;
        
        // Start is called before the first frame update
        void Start()
        {
            for (var i = 0; i < vrmUrls.Length; i++)
            {
                var vrmUrl = vrmUrls[i];
                GameObject playerArmature = Instantiate(playerPrefab, new Vector3(offset * -vrmUrls.Length / 2 + offset / 2 + i * offset, 0, 0), Quaternion.identity);
                playerArmature.name = "Model " + i;
                SimpleAvatarSwapper.Instance.SwapAvatars(playerArmature, Helpers.InternalIPFSURLToHTTP(vrmUrl,
                    "http://ipfs.openmeta.xyz/ipfs/")).Forget();
            }
        }
    }
}
