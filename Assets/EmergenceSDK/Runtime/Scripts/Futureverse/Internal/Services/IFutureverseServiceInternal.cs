using System;
using System.Collections.Generic;
using EmergenceSDK.Runtime.Futureverse.Types;
using EmergenceSDK.Runtime.Futureverse.Types.Responses;
using EmergenceSDK.Runtime.Services;

namespace EmergenceSDK.Runtime.Futureverse.Internal.Services
{
    internal interface IFutureverseServiceInternal : IEmergenceService
    {
        FuturepassInformationResponse CurrentFuturepassInformation { set; }
        [Obsolete] List<AssetTreePathLegacy> ParseGetAssetTreeResponseJsonLegacy(string json);
        List<AssetTreePath> DeserializeGetAssetTreeResponseJson(string json);
    }
}