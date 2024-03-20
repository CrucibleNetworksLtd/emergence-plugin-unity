using System;
using System.Collections.Generic;

namespace EmergenceSDK.Futureverse.Types
{
    public struct FutureverseAssetTreePath
    {
        public readonly string ID;
        public readonly string RdfType;
        public readonly Dictionary<string, FutureverseAssetTreeObject> Objects;

        public FutureverseAssetTreePath(string id, string rdfType,
            Dictionary<string, FutureverseAssetTreeObject> assetTreeObjects)
        {
            ID = id ?? throw new ArgumentNullException();
            RdfType = rdfType ?? throw new ArgumentNullException();
            Objects = assetTreeObjects ?? throw new ArgumentNullException();
        }
    }
}