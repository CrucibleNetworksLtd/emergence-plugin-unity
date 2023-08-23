using System;
using EmergenceSDK.Types;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace EmergenceSDK.Internal.Utils
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

        public static async UniTask<bool> IsWebsiteAlive(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Head(url))
            {
                request.timeout = 5; // set timeout to 5 seconds

                try
                {
                    await request.SendWebRequest().ToUniTask();
            
                    // Check for successful response
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        return true; // website is alive
                    }

                    return false; // website is down
                }
                catch (Exception e)
                {
                    EmergenceLogger.LogWarning("Error in IsWebsiteAlive: " + e.Message);
                    return false; // website is down or error occurred
                }
            }
        }
    }
}
