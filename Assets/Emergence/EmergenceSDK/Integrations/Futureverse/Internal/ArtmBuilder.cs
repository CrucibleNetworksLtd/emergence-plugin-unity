using System;
using System.Collections.Generic;
using EmergenceSDK.Integrations.Futureverse.Types;

namespace EmergenceSDK.Integrations.Futureverse.Internal
{
    internal class ArtmBuilder
    {
        public static string GenerateArtm(string message, List<ArtmOperation> artmOperations,
            string address, int nonce)
        {
            Dictionary<ArtmOperationType, string> operationTypeStrings = new(){
                {ArtmOperationType.CreateLink, "asset-link create"},
                {ArtmOperationType.DeleteLink, "asset-link delete"}
            };

            var artm = "Asset Registry transaction\n\n";
            artm += message + "\n\n";
            artm += "Operations:\n\n";
            foreach (ArtmOperation operation in artmOperations) {
                if (operation.OperationType == ArtmOperationType.None) {
                    continue;
                }
                var array = operation.Slot.Split(":");
                if((array.Length == 0) || array[^1].Trim() == "" || operation.LinkA.Trim() == "" || operation.LinkB.Trim() == ""){
                    throw new Exception("Error parsing ARTM Operation!");
                }
                
                artm += operationTypeStrings[operation.OperationType] + "\n";
                artm += "- " + array[^1] + "\n";
                artm += "- " + operation.LinkA + "\n";
                artm += "- " + operation.LinkB + "\n";
                artm += "end\n\n";
            }
            artm += "Operations END\n\n";
            artm += "Address: " + address + "\n";
            artm += "Nonce: " + nonce;
            return artm;
        }
    }
}