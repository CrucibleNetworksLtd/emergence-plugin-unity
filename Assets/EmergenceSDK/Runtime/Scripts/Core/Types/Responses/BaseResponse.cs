namespace EmergenceSDK.Runtime.Types.Responses
{
    public class BaseResponse<T>
    {
        public StatusCode statusCode;
        public T message;
    }
}
