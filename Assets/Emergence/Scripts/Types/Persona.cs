using Newtonsoft.Json;
using UnityEngine;

namespace EmergenceSDK
{
    public class Persona
    {
        public class PersonaSettings
        {
            public bool availableOnSearch;
            public bool receiveContactRequest;
            public bool showStatus;
        }

        public string avatarId;
        // {Chain}:{Address}:{Token}:{GUID}

        public string id;
        public string name;
        public string bio;

        private Avatar _avatar;
        public Avatar avatar
        {
            get => _avatar;
            set
            {
                _avatar = value;
                // avatarId = value?.chain + ":" + value?.contractAddress + ":" + value?.tokenId + ":" + value?.GUID;
            }
        }

        public PersonaSettings settings;

        [JsonIgnore]
        public Texture2D AvatarImage
        {
            get;
            set;
        }
    }
}