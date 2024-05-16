using System;
using System.Collections.Generic;

namespace EmergenceSDK.Integrations.Futureverse.Types
{
    [Obsolete]
    public struct AssetTreePathLegacy
    {
        public readonly string ID;
        public readonly string RdfType;
        public readonly Dictionary<string, AssetTreeObjectLegacy> Objects;

        public AssetTreePathLegacy(string id, string rdfType,
            Dictionary<string, AssetTreeObjectLegacy> assetTreeObjects)
        {
            ID = id ?? throw new ArgumentNullException();
            RdfType = rdfType ?? throw new ArgumentNullException();
            Objects = assetTreeObjects ?? throw new ArgumentNullException();
        }
    }
}