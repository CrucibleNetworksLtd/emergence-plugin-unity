using EmergenceSDK;
using Nethereum.Web3;
using System;

namespace EmergenceEVMLocalServer.Services
{
    public class Web3WalletConnectSingleton : IWeb3WalletConnectSingleton
    {
        private Web3 web3Provider { get; set; }

        public void Initialize(WalletConnect _walletConnect, string nodeURI)
        {
            web3Provider = new Web3(_walletConnect.CreateProvider(new Uri(nodeURI)));
        }

        public Web3 provider { get => web3Provider; }
    }
}
