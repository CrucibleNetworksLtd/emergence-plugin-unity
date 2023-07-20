using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Types;

namespace EmergenceSDK.Internal.Utils
{
    public class ContractInfo
    {
        public string ContractAddress { get; }
        public string MethodName { get; }
        public string Network { get; }
        public string NodeUrl { get; }
        public string ABI { get; }
        public int ChainId { get; }

        public ContractInfo(string contractAddress, string methodName, string network, string nodeUrl, string abi, int chainId)
        {
            ContractAddress = contractAddress;
            MethodName = methodName;
            Network = network;
            NodeUrl = nodeUrl;
            ABI = abi;
            ChainId = chainId;
        }


        public ContractInfo(DeployedSmartContract deployedSmartContract, string methodName)
        {
            ContractAddress = deployedSmartContract.contractAddress;
            MethodName = methodName;
            Network = deployedSmartContract.chain.networkName;
            NodeUrl = deployedSmartContract.chain.DefaultNodeURL;
            ABI = deployedSmartContract.contract.ABI;
            ChainId = deployedSmartContract.chain.ChainID;
        }
        
        public string ToReadUrl() => StaticConfig.APIBase + "readMethod?contractAddress=" + 
                                 ContractAddress + "&methodName=" + MethodName + "&nodeUrl=" + NodeUrl + "&network=" + Network;
        
        public string ToWriteUrl(string localAccountName, string gasPrice, string value) => 
            StaticConfig.APIBase + "writeMethod?contractAddress=" +
            ContractAddress + "&methodName=" + MethodName + localAccountName + gasPrice +
            "&network=" + Network + "&nodeUrl=" + NodeUrl + "&value=" + value;
    }
}