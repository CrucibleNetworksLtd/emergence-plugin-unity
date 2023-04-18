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
        //TODO: Remove
        [JsonIgnore]
        public Avatar avatar
        {
            get => _avatar;
            set
            {
                _avatar = value;
            }
        }

        [JsonIgnore]
        public Texture2D AvatarImage
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"Persona: {name} ({id})";
        }
    }
}