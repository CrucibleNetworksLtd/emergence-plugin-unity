using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace EmergenceSDK.Types
{
    public class Avatar
    {
        public class AvatarMeta
        {
            public class AvatarMetaAttributes
            {
                
            }

            public class AvatarMetaContent
            {
                public string url;
                public string representation;
                public string mimeType;
                public int size;
                public int width;
                public int height;
            }

            public class AvatarMetaRestrictions
            {
                
            }
            
            public string name;
            public string description;
            public List<AvatarMetaAttributes> attributes;
            public List<AvatarMetaContent> content;
            public List<AvatarMetaRestrictions> restrictions;
            public string dynamicMetadata;
        }
        
        public string avatarId;
        public string contractAddress;
        public string tokenId;
        public string tokenURI;
        public string lastUpdated;
        public string chain;
        public AvatarMeta meta;
        public string GUID;

        [JsonIgnore]
        public Texture2D AvatarImage
        {
            get;
            set;
        }
    }
}