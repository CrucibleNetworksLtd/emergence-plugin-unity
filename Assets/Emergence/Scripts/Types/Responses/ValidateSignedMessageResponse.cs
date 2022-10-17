using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergenceSDK
{
    public class ValidateSignedMessageResponse : BaseResponse<ValidateSignedMessageResponse>
    {
        public bool valid { get; set; }
        public string signer { get; set; }
    }
}
