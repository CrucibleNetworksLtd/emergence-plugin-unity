using Newtonsoft.Json;
using UnityEngine;

namespace EmergenceSDK.Runtime.Types
{
    public class Persona
    {
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
            var isAvatarValid = 
                    avatar is { chain: not null } // Pattern matching syntax, matches the pattern where avatar has a not null chain field, also fails if avatar is null
                    && avatar.chain.Trim() != ""
                    && avatar.contractAddress.Trim() != ""
                    && avatar.tokenId.Trim() != ""
                ;
            
            return isAvatarValid ? $"{avatar.chain}:{avatar.contractAddress}:{avatar.tokenId}:{avatar.GUID}" : "";
        }

        public override string ToString()
        {
            return $"Persona: {name} ({id})";
        }
    }
}