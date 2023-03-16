using EmergenceSDK.Types;

namespace EmergenceSDK.Internal.Utils
{
    public class ContractInfo
    {
        public string ContractAddress { get; }
        public string MethodName { get; }
        public string Network { get; }
        public string NodeUrl { get; }
        
        public ContractInfo(string contractAddress, string methodName, string network, string nodeUrl)
        {
            ContractAddress = contractAddress;
            MethodName = methodName;
            Network = network;
            NodeUrl = nodeUrl;
        }
        
        public string ToReadUrl() => EmergenceSingleton.Instance.Configuration.APIBase + "readMethod?contractAddress=" + 
                                 ContractAddress + "&methodName=" + MethodName + "&nodeUrl=" + NodeUrl + "&network=" + Network;
        
        public string ToWriteUrl(string localAccountName, string gasPrice, string value) => 
            EmergenceSingleton.Instance.Configuration.APIBase + "writeMethod?contractAddress=" +
            ContractAddress + "&methodName=" + MethodName + localAccountName + gasPrice +
            "&network=" + Network + "&nodeUrl=" + NodeUrl + "&value=" + value;
    }
}