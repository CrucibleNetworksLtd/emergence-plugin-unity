using System;
using EmergenceSDK.Internal.Types;

namespace EmergenceSDK.Types
{
    public enum ServiceResponseCode
    {
        Success,
        Failure
    }

    public class ServiceResponse : IDisposable
    {
        public readonly WebResponse Response;
        public bool Successful => Code == ServiceResponseCode.Success;
        public ServiceResponseCode Code { get; }

        public ServiceResponse(WebResponse response, bool successful)
        {
            Response = response;
            Code = successful ? ServiceResponseCode.Success : ServiceResponseCode.Failure;
        }

        public ServiceResponse(ServiceResponse response, bool successful) : this(response.Response, successful) {}
        public ServiceResponse(WebResponse response) : this(response, response?.Successful ?? false) {}
        public ServiceResponse(ServiceResponse response) : this(response, response?.Successful ?? false) {}
        public ServiceResponse(bool successful) : this((WebResponse)null, successful) {}

        ~ServiceResponse()
        {
            Dispose();
        }
        
        public void Dispose()
        {
            Response?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class ServiceResponse<T1> : ServiceResponse
    {
        public readonly T1 Result1;
        
        public ServiceResponse(WebResponse response, bool successful, T1 result1 = default) : base(response, successful)
        {
            Result1 = result1;
        }
        
        public ServiceResponse(ServiceResponse response, bool successful, T1 result1 = default) : this(response.Response, successful, result1) { }
        public ServiceResponse(WebResponse response, T1 result1 = default) : this(response, response?.Successful ?? false, result1) { }
        public ServiceResponse(ServiceResponse response, T1 result1 = default) : this(response, response?.Successful ?? false, result1) { }
        public ServiceResponse(bool successful, T1 result1 = default) : this((WebResponse)null, successful, result1) { }
    }

    public class ServiceResponse<T1, T2> : ServiceResponse<T1>
    {
        public readonly T2 Result2;
        
        public ServiceResponse(WebResponse response, bool successful, T1 result1 = default, T2 result2 = default) : base(response, successful, result1)
        {
            Result2 = result2;
        }

        public ServiceResponse(ServiceResponse response, bool successful, T1 result1 = default, T2 result2 = default) : this(response.Response, successful, result1, result2) { }
        public ServiceResponse(WebResponse response, T1 result1 = default, T2 result2 = default) : this(response, response?.Successful ?? false, result1, result2) { }
        public ServiceResponse(ServiceResponse response, T1 result1 = default, T2 result2 = default) : this(response, response?.Successful ?? false, result1, result2) { }
        public ServiceResponse(bool successful, T1 result1 = default, T2 result2 = default) : this((WebResponse)null, successful, result1, result2) { }
    }
}