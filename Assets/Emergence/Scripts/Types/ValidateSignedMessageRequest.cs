using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergenceSDK
{
    public class ValidateSignedMessageRequest
    {
        public string message { get; set; }
        public string signedMessage { get; set; }
        public string address { get; set; }
    }
}
