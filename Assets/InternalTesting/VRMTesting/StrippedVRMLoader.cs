using System.Collections;
using System.Collections.Generic;
using EmergenceSDK.EmergenceDemo;
using UnityEngine;

namespace EmergenceSDK
{
    public class StrippedVRMLoader : MonoBehaviour
    {
        public string VRMUrl;
        
        // Start is called before the first frame update
        void Start()
        {
            DemoAvatarManager.Instance.SwapAvatars(VRMUrl);
        }
    }
}
