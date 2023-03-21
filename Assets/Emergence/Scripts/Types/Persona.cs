using Newtonsoft.Json;
using UnityEngine;

namespace EmergenceSDK.Types
{
    public class Persona
    {
        public string avatarId;
        // {Chain}:{Address}:{Token}:{GUID}

        public string id;
        public string name;
        public string bio;
        
        [JsonIgnore]
        private Avatar _avatar;
        //TODO: Remove
        [JsonIgnore]
        public Avatar avatar
        {
            get => _avatar;
            set
            {
                _avatar = value;
                // avatarId = value?.chain + ":" + value?.contractAddress + ":" + value?.tokenId + ":" + value?.GUID;
            }
        }

        [JsonIgnore]
        public Texture2D AvatarImage
        {
            get;
            set;
        }
    }
}