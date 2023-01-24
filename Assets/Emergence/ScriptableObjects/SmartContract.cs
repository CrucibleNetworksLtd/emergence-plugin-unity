using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmergenceSDK
{
    [CreateAssetMenu(fileName = "SmartContract", menuName = "Smart Contract", order = 1)]
    public class SmartContract : ScriptableObject
    {
        // public string contractAddress;
        public string ABI;
    }
}