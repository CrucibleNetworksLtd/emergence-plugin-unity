using System;

namespace EmergenceSDK.Integrations.Futureverse.Types
{
    public struct FutureverseArtmOperation
    {
        public readonly FutureverseArtmOperationType OperationType;
        public readonly string Slot;
        public readonly string LinkA;
        public readonly string LinkB;

        public FutureverseArtmOperation(FutureverseArtmOperationType operationType, string slot, string linkA,
            string linkB)
        {
            OperationType = operationType;
            Slot = slot ?? throw new ArgumentNullException(nameof(slot));
            LinkA = linkA ?? throw new ArgumentNullException(nameof(linkA));
            LinkB = linkB ?? throw new ArgumentNullException(nameof(linkB));
        }
    }
}