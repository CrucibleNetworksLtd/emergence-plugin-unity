using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace EmergenceSDK.Runtime.Futureverse.Types
{
    [Obsolete]
    public struct AssetTreeObjectLegacy
    {
        public readonly string ID;
        public readonly Dictionary<string, JToken> AdditionalData;

        public AssetTreeObjectLegacy(string id, Dictionary<string, JToken> additionalData)
        {
            AdditionalData = additionalData ?? throw new ArgumentNullException();
            ID = id ?? throw new ArgumentNullException();
        }
    }
}