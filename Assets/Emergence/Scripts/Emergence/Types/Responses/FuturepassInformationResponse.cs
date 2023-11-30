using System.Collections.Generic;

namespace EmergenceSDK.Types.Responses
{
    public class FuturepassInformationResponse
    {
        public string futurepass { get; set; }
        public string ownerEoa { get; set; }
        public List<LinkedEoa> linkedEoas { get; set; }
        public List<object> invalidEoas { get; set; }
    }
    
    public class LinkedEoa
    {
        public string eoa { get; set; }
        public int proxyType { get; set; }
    }
}