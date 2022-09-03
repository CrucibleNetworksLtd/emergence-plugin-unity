namespace EmergenceSDK
{
    public class BaseResponse<T>
    {
        public StatusCode statusCode;
        public T message;
    }
}
