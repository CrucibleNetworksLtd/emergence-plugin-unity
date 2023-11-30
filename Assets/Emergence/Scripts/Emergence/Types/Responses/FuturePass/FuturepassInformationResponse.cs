using System.Collections.Generic;
using System.Linq;

namespace EmergenceSDK.Types.Responses.FuturePass
{
    /*
       {
        "futurepass": "7668:root:0xffffffff0000000000000000000000000000136e",
        "ownerEoa": "1:evm:0x238678df4f2ceecc8b09c2b49eb94458682e6a4c",
        "linkedEoas": [
        {
          "eoa": "1:evm:0x5d132022cc202cd683182c211030f9a31d34417c",
          "proxyType": 1
        }
        ],
        "invalidEoas": []
        }
     */
    
    public class FuturepassInformationResponse
    {
        public string futurepass { get; set; }
        public string ownerEoa { get; set; }
        public List<LinkedEoa> linkedEoas { get; set; }
        public List<object> invalidEoas { get; set; }

        public List<string> GetCombinedAddresses()
        {
            var ret = new List<string>();
            ret.Add($"{futurepass.Split(':').Last()}");
            ret.Add($"{ownerEoa.Split(':').Last()}");
            foreach (var linkedEoa in linkedEoas)
            {
                ret.Add($"{linkedEoa.eoa.Split(':').Last()}");
            }
            return ret;
        }
    }
    
    public class LinkedEoa
    {
        public string eoa { get; set; }
        public int proxyType { get; set; }
    }
}