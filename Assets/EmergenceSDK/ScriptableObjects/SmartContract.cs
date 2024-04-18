using UnityEngine;

namespace EmergenceSDK.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SmartContract", menuName = "Smart Contract", order = 1)]
    public class SmartContract : ScriptableObject
    {
        public string ABI;
    }
}