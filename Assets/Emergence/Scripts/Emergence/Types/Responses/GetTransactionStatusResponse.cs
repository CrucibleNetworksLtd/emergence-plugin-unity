namespace EmergenceSDK.Types.Responses
{
    public class GetTransactionStatusResponse : BaseResponse<GetTransactionStatusResponse>
    {
        public TransactionStatus transaction { get; set; }
    }


    public class TransactionStatus
    {
        public string To { get; set; }
        public int Type { get; set; }
        public string Logs { get; set; }
        public bool Status { get; set; }
        public string ContractAddress { get; set; }
        public string EffectiveGasPrice { get; set; }
        public string GasUsed { get; set; }
        public string CumulativeGasUsed { get; set; }
        public string Root { get; set; }
        public string From { get; set; }
        public string BlockNumber { get; set; }
        public string BlockHash { get; set; }
        public string TransactionIndex { get; set; }
        public string TransactionHash { get; set; }
        public string LogsBloom { get; set; }
        public int Confirmations { get; set; }
    }
}
