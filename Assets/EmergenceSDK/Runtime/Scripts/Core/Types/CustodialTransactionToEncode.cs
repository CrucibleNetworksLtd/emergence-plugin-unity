using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EmergenceSDK.Runtime.Types
{
    public class CustodialTransactionToEncode
    {
        public string ABI { get; set; }

        public string ContractAddress { get; set; }
        
        public string MethodName { get; set; }
        
        public JArray CallInputs { get; set; } // The inputs are polymorphic, handled dynamically with JArray
    }
}