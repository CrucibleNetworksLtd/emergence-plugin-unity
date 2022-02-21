using Nethereum.Web3;

namespace EmergenceEVMLocalServer.Services
{
    public interface IWeb3Service
    {
        Web3 TempWeb3Instance(string nodeURI);
    }
}
