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
        
        /// <summary>
        /// This function exists mostly to provide an exact function used for deserialization which can be reused in autotests.
        /// </summary>
        /// <param name="json">JSON encoded object to deserialize</param>
        /// <returns></returns>
        List<AssetTreePath> DeserializeGetAssetTreeResponseJson(string json);
    }
}