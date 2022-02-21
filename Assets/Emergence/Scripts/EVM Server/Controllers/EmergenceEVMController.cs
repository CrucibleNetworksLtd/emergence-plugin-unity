using EmergenceEVMLocalServer.ViewModels;
using EmergenceSDK;

namespace EmergenceEVMLocalServer.Controllers
{
    public class EmergenceEVMController
    {
        internal string NotConnected()
        {
            var response = new BaseResponse()
            {
                statusCode = BaseResponse.StatusCode.NotConnected,
                message = null
            };

            return SerializationHelper.Serialize(response);
        }

        internal string AlreadyConnected()
        {
            var response = new BaseResponse()
            {
                statusCode = BaseResponse.StatusCode.AlreadyConnected,
                message = null
            };

            return SerializationHelper.Serialize(response);
        }

        internal string SuccessResponse(object message = null)
        {
            var response = new BaseResponse()
            {
                statusCode = BaseResponse.StatusCode.Ok,
                message = message,
            };

            return SerializationHelper.Serialize(response);
        }

        internal string ErrorResponse(string message, BaseResponse.StatusCode statusCode = BaseResponse.StatusCode.Error)
        {
            // HttpContext.Response.StatusCode = 512;

            var response = new BaseResponse()
            {
                statusCode = statusCode,
                message = message,
            };

            return SerializationHelper.Serialize(response);
        }
    }
}
