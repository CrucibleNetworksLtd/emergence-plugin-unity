using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Types;
using EmergenceSDK.Integrations.Futureverse.Types.Responses;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Inventory;

namespace EmergenceSDK.Integrations.Futureverse.Internal.Services
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