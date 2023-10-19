using UnityEngine;
using UnityEngine.Serialization;

namespace EmergenceSDK.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Configuration", menuName = "EmergenceChain", order = 1)]
    public class EmergenceChain : ScriptableObject
    {
        [Header ("Chain Configuration")]
        public string DefaultNodeURL = "https://polygon-mainnet.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134";
        public int ChainID;
        public string NetworkName;
        
        [Header ("Currency Configuration")]
        public string CurrencySymbol = "M";
        public string CurrencyName = "Matic";
    }
}
