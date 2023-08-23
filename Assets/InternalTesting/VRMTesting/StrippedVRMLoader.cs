using System.Collections;
using System.Collections.Generic;
using EmergenceSDK.EmergenceDemo;
using EmergenceSDK.Internal.Utils;
using UnityEngine;

namespace EmergenceSDK
{
    public class StrippedVRMLoader : MonoBehaviour
    {
        public string VRMUrl;
        
        // Start is called before the first frame update
        void Start()
        {
            DemoAvatarManager.Instance.SwapAvatars(Helpers.InternalIPFSURLToHTTP(VRMUrl, "http://ipfs.openmeta.xyz/ipfs/"));
        }
    }
}
