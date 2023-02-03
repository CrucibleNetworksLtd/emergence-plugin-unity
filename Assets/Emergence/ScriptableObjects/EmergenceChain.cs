using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmergenceSDK
{
    [CreateAssetMenu(fileName = "Configuration", menuName = "EmergenceChain", order = 1)]
    public class EmergenceChain : ScriptableObject
    {
        public string DefaultNodeURL = "https://polygon-mainnet.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134";
        public int ChainID;
        public string networkName;
    }
}
