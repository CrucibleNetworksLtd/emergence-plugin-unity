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

        public string id;
        public string name;
        public string bio;
        public string avatar;
        public PersonaSettings settings;

        private Texture2D avatarImage = null;

        public Texture2D AvatarImage
        {
            get;
            set;
        }
    }
}