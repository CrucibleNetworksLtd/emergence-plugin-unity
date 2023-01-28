using EmergenceSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmergenceSDK
{
    [CreateAssetMenu(fileName = "Configuration", menuName = "EmergenceConfiguration", order = 1)]
    public class EmergenceConfiguration : ScriptableObject
    {
        public string APIBase = "http://evm.openmeta.xyz/api/";
        //        public string DefaultNodeURL = "https://polygon-mainnet.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134";
        public EmergenceChain Chain;
        public string AvatarURL = "https://dysaw5zhak.us-east-1.awsapprunner.com/AvatarSystem/";
        public string InventoryURL = "https://dysaw5zhak.us-east-1.awsapprunner.com/InventoryService/";
        public string PersonaURL = "https://x8iq9e5fq1.execute-api.us-east-1.amazonaws.com/staging/";
        public string CustomEmergenceServerLocation = "C:\\Dev\\emergence-evm-server\\bin\\Debug\\net5.0\\EmergenceEVMLocalServer.exe";
        public string CustomEmergenceServerURL = "http://localhost:50733/";
    } 
}