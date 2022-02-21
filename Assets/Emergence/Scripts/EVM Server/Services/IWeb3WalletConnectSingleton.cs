using Nethereum.Web3;
using WalletConnectSharp.Unity;

namespace EmergenceEVMLocalServer.Services
{
    public interface IWeb3WalletConnectSingleton
    {
        Web3 provider { get; }

        void Initialize(WalletConnect walletConnect, string nodeURI);
    }
}
