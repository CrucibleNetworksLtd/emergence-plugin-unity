namespace EmergenceSDK.Types.Responses
{
    public class BaseResponse<T>
    {
        public StatusCode statusCode;
        public T message;
    }
}
