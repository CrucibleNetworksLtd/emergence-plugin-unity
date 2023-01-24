using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmergenceSDK
{
    [CreateAssetMenu(fileName = "DeployedSmartContract", menuName = "Deployed Smart Contract", order = 2)]
    public class DeployedSmartContract : ScriptableObject
    {
        public string contractAddress;
        // public string ABI;
        public SmartContract contract;
        public EmergenceChain chain;
    }
}