using UnityEngine;

namespace Emergence
{
    public class Persona
    {
        public struct PersonaSettings
        {
            public bool availableOnSearch;
            public bool receiveContactRequest;
            public bool showStatus;
        }

        public struct Avatar
        {
            public string id;
            public string url;
        }

        public string id;
        public string name;
        public string bio;
        public Avatar avatar;
        public PersonaSettings settings;

        public Texture2D AvatarImage
        {
            get;
            set;
        }
    }
}