using System.Collections.Generic;

namespace EmergenceSDK.Types.Responses
{
    public class LinkedFuturepassResponse
    {
        public string eoa { get; set; }
        public string ownedFuturepass { get; set; }
        public object linkedFuturepass { get; set; }
        public List<object> invalidFuturepass { get; set; }
    }
}