namespace EmergenceEVMLocalServer.ViewModels
{
    public class BaseResponse
    {

        public enum StatusCode : ushort
        {
            Ok = 0,
            NotConnected = 1,
            AlreadyConnected = 2,
            Error = 3,
            FileAlreadyExists = 4,
            FileNotFound = 5,
            IncorrectPassword = 6,
        }

        public StatusCode statusCode { get; set; }

        public object message { get; set; }

    }
}
