using EmergenceSDK;
using Nethereum.Web3;

namespace EmergenceEVMLocalServer.Services
{
    public interface IWeb3WalletConnectSingleton
    {
        Web3 provider { get; }

        void Initialize(WalletConnect walletConnect, string nodeURI);
    }
}
