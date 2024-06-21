namespace EmergenceSDK.Types.Responses
{
    public class ValidateSignedMessageResponse : BaseResponse<ValidateSignedMessageResponse>
    {
        public bool valid { get; set; }
        public string signer { get; set; }
    }
}
