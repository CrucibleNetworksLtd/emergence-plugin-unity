using Newtonsoft.Json;
using UnityEngine;

namespace EmergenceSDK.Types
{
    public class Persona
    {
        // {Chain}:{Address}:{Token}:{GUID}
        public string avatarId;

        public string id;
        public string name;
        public string bio;
        
        [JsonIgnore]
        private Avatar _avatar;
        
        [JsonIgnore]
        public Avatar avatar
        {
            get => _avatar;
            set
            {
                _avatar = value;
                avatarId = GenerateAvatarId(value);
            }
        }

        [JsonIgnore]
        public Texture2D AvatarImage
        {
            get;
            set;
        }
        
        private string GenerateAvatarId(Avatar avatar)
        {
            if(avatar == null)
                return "";
            return $"{avatar.chain}:{avatar.contractAddress}:{avatar.tokenId}:{avatar.GUID}";
        }

        public override string ToString()
        {
            return $"Persona: {name} ({id})";
        }
    }
}