namespace EmergenceSDK.Types.Responses
{
    public class WriteContractResponse : BaseResponse<string>
    {
        public string transactionHash { get; set; }

        public WriteContractResponse(string transactionHash)
        {
            this.transactionHash = transactionHash;
        }
    }
}
