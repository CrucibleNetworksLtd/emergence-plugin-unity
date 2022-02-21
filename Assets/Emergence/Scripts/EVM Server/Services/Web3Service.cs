using Nethereum.Web3;

namespace EmergenceEVMLocalServer.Services
{
    public class Web3Service : IWeb3Service
    {
        public Web3 TempWeb3Instance(string nodeURI)
        {
            return new Web3(nodeURI);
        }
    }
}
