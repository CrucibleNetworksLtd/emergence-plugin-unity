using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace EmergenceSDK.Futureverse.Types
{
    public struct FutureverseAssetTreeObject
    {
        public readonly string ID;
        public readonly Dictionary<string, JToken> AdditionalData;

        public FutureverseAssetTreeObject(string id, Dictionary<string, JToken> additionalData)
        {
            AdditionalData = additionalData ?? throw new ArgumentNullException();
            ID = id ?? throw new ArgumentNullException();
        }
    }
}