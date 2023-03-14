using EmergenceSDK.Types;
using UnityEngine;

namespace EmergenceSDK.Internal.Utils
{
    public static class Helpers
    {
        public static string InternalIPFSURLToHTTP(string IPFSURL)
        {
            if (IPFSURL.Contains("ipfs://") || IPFSURL.Contains("IPFS://"))
            {
                Debug.Log("Found IPFS URL, replacing with public node...");
        
                string IPFSNode = "http://ipfs.openmeta.xyz/ipfs/";
                string CustomIPFSNode = EmergenceSingleton.Instance.Configuration.defaultIpfsGateway;
                if (!string.IsNullOrEmpty(CustomIPFSNode))
                {
                    Debug.Log($"Found custom IPFS node in game config, replacing with \"{CustomIPFSNode}\"");
                    IPFSNode = CustomIPFSNode;
                }
                string NewURL = IPFSURL.Replace("ipfs://", IPFSNode);
                NewURL = NewURL.Replace("IPFS://", IPFSNode);
                Debug.Log($"New URL is \"{NewURL}\"");
                return NewURL;
            }
            else
            {
                return IPFSURL;
            }
        }

    }
}
