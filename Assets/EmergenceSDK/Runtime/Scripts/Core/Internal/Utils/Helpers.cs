using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace EmergenceSDK.Runtime.Internal.Utils
{
    public static class Helpers
    {
        public static string InternalIPFSURLToHTTP(string IPFSURL, string defaultGateway = "")
        {
            if (IPFSURL.Contains("ipfs://") || IPFSURL.Contains("IPFS://"))
            {
                EmergenceLogger.LogInfo("Found IPFS URL, replacing with public node...");
        
                string IPFSNode = "http://ipfs.openmeta.xyz/ipfs/";
                string CustomIPFSNode = defaultGateway == ""
                    ? EmergenceSingleton.Instance.Configuration.defaultIpfsGateway
                    : defaultGateway;
                if (!string.IsNullOrEmpty(CustomIPFSNode))
                {
                    EmergenceLogger.LogInfo($"Found custom IPFS node in game config, replacing with \"{CustomIPFSNode}\"");
                    IPFSNode = CustomIPFSNode;
                }
                string NewURL = IPFSURL.Replace("ipfs://", IPFSNode);
                NewURL = NewURL.Replace("IPFS://", IPFSNode);
                EmergenceLogger.LogInfo($"New URL is \"{NewURL}\"");
                return NewURL;
            }
            return IPFSURL;
        }
    }
}
