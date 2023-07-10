using System;

namespace EmergenceSDK.Types
{
    public class ServiceResponse<T>
    {
        public readonly bool success;
        public readonly T result;

        public ServiceResponse(bool success, T result = default)
        {
            this.success = success;
            this.result = result;
        }
    }
}