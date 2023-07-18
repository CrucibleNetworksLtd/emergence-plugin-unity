using EmergenceSDK.Types;
using UnityEngine;

namespace EmergenceSDK.ScriptableObjects
{
    public enum Environment
    {
        Staging,
        Production
    }
    [CreateAssetMenu(fileName = "Configuration", menuName = "EmergenceConfiguration", order = 1)]
    public class EmergenceConfiguration : ScriptableObject
    {
        public string defaultIpfsGateway = "http://ipfs.openmeta.xyz/ipfs/";
        public EmergenceChain Chain;

        private string _avatarURLStaging = "https://dysaw5zhak.us-east-1.awsapprunner.com/AvatarSystem/";
        private string _avatarURLProduction = "https://dysaw5zhak.us-east-1.awsapprunner.com/AvatarSystem/";
        
        private string _inventoryURLStaging = "https://dysaw5zhak.us-east-1.awsapprunner.com/InventoryService/";
        private string _inventoryURLProduction = "https://dysaw5zhak.us-east-1.awsapprunner.com/InventoryService/";
        
        private string _personaURLStaging = "https://x8iq9e5fq1.execute-api.us-east-1.amazonaws.com/staging/";
        private string _personaURLProduction = "https://x8iq9e5fq1.execute-api.us-east-1.amazonaws.com/staging/";
        
        public string AvatarURL
        {
            get
            {
                if (EmergenceSingleton.Instance.Environment == Environment.Staging)
                {
                    return _avatarURLStaging;
                }
                else
                {
                    return _avatarURLProduction;
                }
            }
        }
        
        public string InventoryURL
        {
            get
            {
                if (EmergenceSingleton.Instance.Environment == Environment.Staging)
                {
                    return _inventoryURLStaging;
                }
                else
                {
                    return _inventoryURLProduction;
                }
            }
        }
        
        public string PersonaURL
        {
            get
            {
                if (EmergenceSingleton.Instance.Environment == Environment.Staging)
                {
                    return _personaURLStaging;
                }
                else
                {
                    return _personaURLProduction;
                }
            }
        }
    } 
}