using System.Collections.Generic;
using System.Linq;

namespace EmergenceSDK.Integrations.Futureverse.Types.Responses
{
    public class FuturepassInformationResponse
    {
        public string futurepass { get; set; }
        public string ownerEoa { get; set; }
        public List<LinkedEoa> linkedEoas { get; set; }
        public List<object> invalidEoas { get; set; }

        internal List<string> GetCombinedAddresses()
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