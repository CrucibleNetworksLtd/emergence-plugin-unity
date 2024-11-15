using UnityEngine;

namespace EmergenceSDK.Runtime.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CustodialLoginConfiguration", menuName = "Custodial Login Configuration", order = 2)]
    public class CustodialLoginConfiguration : ScriptableObject
    {
        [Header("Client ID's for custodial service")]
        public string DevelopmentClientID;
        public string StagingClientID;
        public string ProductionClientID;
        
        [Header("Base URL's for Custodial Login Service")]
        public string ProductionLoginBaseUrl;
        public string StagingLoginBaseUrl;
        
        [Header("Base URL's for Custodial Signer Service")]
        public string ProductionSigningBaseUrl;
        public string StagingSigningBaseUrl;
    }
}