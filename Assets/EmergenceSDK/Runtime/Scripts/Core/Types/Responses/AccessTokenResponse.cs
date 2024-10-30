using System;

namespace EmergenceSDK.Runtime.Types.Responses
{
    public class AccessTokenResponse
    {
        public AccessToken AccessToken { get; set; }
    }

    public class AccessToken
    {
        public string Token { get; set; }         
        public DateTime Expiration { get; set; }  
        public string RefreshToken { get; set; } 
    }
}