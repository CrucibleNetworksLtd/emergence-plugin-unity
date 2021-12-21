using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emergence
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class AccessToken
    {
        public string signedMessage;
        public string message;
        public string address;
    }

    public class Message
    {
        public AccessToken accessToken;
    }

    public class AccessTokenResponse
    {
        public int statusCode;
        public Message message;
    }
}
