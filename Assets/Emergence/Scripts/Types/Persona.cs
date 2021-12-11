using UnityEngine;

namespace Emergence
{
    public class Persona
    {
        public string id;
        public string avatar;

        private Texture2D avatarImage = null;

        public Texture2D AvatarImage
        {
            get;
            set;
        }
    }
}