namespace EmergenceSDK.Integrations.Futureverse.Types
{
    public class ArtmTransactionResponse
    {
        public readonly ArtmStatus? Status;
        public readonly string TransactionHash;

        public ArtmTransactionResponse(ArtmStatus status, string transactionHash)
        {
            Status = status;
            TransactionHash = transactionHash;
        }

        public ArtmTransactionResponse(string transactionHash)
        {
            TransactionHash = transactionHash;
        }
    }
}