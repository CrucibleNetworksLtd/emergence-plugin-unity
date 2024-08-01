using UnityEngine;

namespace EmergenceSDK.Runtime.ScriptableObjects
{
    [CreateAssetMenu(fileName = "DeployedSmartContract", menuName = "Deployed Smart Contract", order = 2)]
    public class DeployedSmartContract : ScriptableObject
    {
        public string contractAddress;
        public SmartContract contract;
        public EmergenceChain chain;
    }
}