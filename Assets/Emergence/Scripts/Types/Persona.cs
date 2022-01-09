using Newtonsoft.Json;
using UnityEngine;

namespace Emergence
{
    public class Persona
    {
        public class PersonaSettings
        {
            public bool availableOnSearch;
            public bool receiveContactRequest;
            public bool showStatus;
        }

        public class Avatar
        {
            public string id;
            public string url;
        }

        public string id;
        public string name;
        public string bio;
        public Avatar avatar;
        public PersonaSettings settings;

        [JsonIgnore]
        public Texture2D AvatarImage
        {
            get;
            set;
        }
    }
}