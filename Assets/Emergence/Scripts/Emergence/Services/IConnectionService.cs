namespace EmergenceSDK.Services
{
    public interface IConnectionService
    {
        void IsConnected(IsConnectedSuccess success, ErrorCallback errorCallback);
    }
}
