using System.Collections.Generic;

namespace EmergenceEVMLocalServer.Services
{
    public class ContractsService
    {

        public class Contract
        {
            public string address { get; set; }
            public string ABI { get; set; }
        }

        public List<Contract> contracts = new List<Contract>();

        public void LoadContract(string _address, string _ABI)
        {
            if (contracts.Exists(x => x.address == _address))
                contracts.Remove(contracts.Find(x => x.address == _address));

            contracts.Add(new Contract() { address = _address, ABI = _ABI });
        }

        public Contract GetContract(string address)
        {
            return contracts.Find(x => x.address == address);
        }

    }
}
