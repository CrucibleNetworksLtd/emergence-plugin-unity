using System;
using EmergenceSDK.Runtime.Types;
using UnityEngine;

namespace EmergenceSDK.Runtime.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Configuration", menuName = "EmergenceConfiguration", order = 1)]
    public class EmergenceConfiguration : ScriptableObject
    {
        public string defaultIpfsGateway = "http://ipfs.openmeta.xyz/ipfs/";
        [SerializeField] private EmergenceChain DevelopmentChain;
        [SerializeField] private EmergenceChain StagingChain;
        [SerializeField] private EmergenceChain ProductionChain;

        public EmergenceChain Chain
        {
            get
            {
                return EmergenceSingleton.Instance.Environment switch
                {
                    EmergenceEnvironment.Development => DevelopmentChain,
                    EmergenceEnvironment.Staging => StagingChain,
                    EmergenceEnvironment.Production => ProductionChain,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        private string _avatarURLStaging = "https://dysaw5zhak.us-east-1.awsapprunner.com/AvatarSystem/";
        private string _avatarURLProduction = "https://dysaw5zhak.us-east-1.awsapprunner.com/AvatarSystem/";
        
        private string _inventoryURLStaging = "https://dysaw5zhak.us-east-1.awsapprunner.com/InventoryService/";
        private string _inventoryURLProduction = "https://dysaw5zhak.us-east-1.awsapprunner.com/InventoryService/";
        
        private string _personaURLStaging = "https://x8iq9e5fq1.execute-api.us-east-1.amazonaws.com/staging/";
        private string _personaURLProduction = "https://x8iq9e5fq1.execute-api.us-east-1.amazonaws.com/staging/";
        
        public string AvatarURL => EmergenceSingleton.Instance.Environment == EmergenceEnvironment.Production ? _avatarURLProduction : _avatarURLStaging;
        public string InventoryURL => EmergenceSingleton.Instance.Environment == EmergenceEnvironment.Development ? _inventoryURLProduction : _inventoryURLStaging;
        public string PersonaURL => EmergenceSingleton.Instance.Environment == EmergenceEnvironment.Staging ? _personaURLProduction : _personaURLStaging;
    } 
}