namespace EmergenceSDK.Types
{
    public enum ServiceResponseCode
    {
        Success,
        Failure
    }

    public class ServiceResponse
    {
        public bool Success => Code == ServiceResponseCode.Success;

        public ServiceResponseCode Code => code;
        private readonly ServiceResponseCode code;

        public ServiceResponse(bool success)
        {
            code = success ? ServiceResponseCode.Success : ServiceResponseCode.Failure;
        }
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

    public class ServiceResponse<T, U>
    {
        public bool Success => Code == ServiceResponseCode.Success;
        public readonly T Result0;
        public readonly U Result1;

        public ServiceResponseCode Code => code;
        private readonly ServiceResponseCode code;

        public ServiceResponse(bool success, T result0 = default, U result1 = default)
        {
            code = success ? ServiceResponseCode.Success : ServiceResponseCode.Failure;
            Result0 = result0;
            Result1 = result1;
        }
    }
}