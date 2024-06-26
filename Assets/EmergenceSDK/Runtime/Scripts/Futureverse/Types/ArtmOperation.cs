using System;

namespace EmergenceSDK.Runtime.Futureverse.Types
{
    public struct ArtmOperation
    {
        public readonly ArtmOperationType OperationType;
        public readonly string Slot;
        public readonly string LinkA;
        public readonly string LinkB;

        public ArtmOperation(ArtmOperationType operationType, string slot, string linkA,
            string linkB)
        {
            OperationType = operationType;
            Slot = slot ?? throw new ArgumentNullException(nameof(slot));
            LinkA = linkA ?? throw new ArgumentNullException(nameof(linkA));
            LinkB = linkB ?? throw new ArgumentNullException(nameof(linkB));
        }
    }
}