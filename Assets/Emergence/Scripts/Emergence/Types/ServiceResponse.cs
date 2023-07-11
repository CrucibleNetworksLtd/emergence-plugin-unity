namespace EmergenceSDK.Types
{
    public enum ServiceResponseCode
    {
        Success,
        Failure
    }
    
    public class ServiceResponse<T>
    {
        public bool Success => Code == ServiceResponseCode.Success;
        public readonly T Result;
        
        public ServiceResponseCode Code => code;
        private readonly ServiceResponseCode code;

        public ServiceResponse(bool success, T result = default)
        {
            code = success ? ServiceResponseCode.Success : ServiceResponseCode.Failure;
            Result = result;
        }
    }
}