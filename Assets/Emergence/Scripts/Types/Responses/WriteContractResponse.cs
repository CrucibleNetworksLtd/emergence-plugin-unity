namespace EmergenceSDK.Types.Responses
{
    public class WriteContractResponse : BaseResponse<string>
    {
        public string transactionHash { get; set; }
    }
}
