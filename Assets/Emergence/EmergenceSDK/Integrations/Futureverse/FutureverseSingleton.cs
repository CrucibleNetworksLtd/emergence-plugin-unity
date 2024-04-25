using EmergenceSDK.Internal.Utils;

namespace EmergenceSDK.Integrations.Futureverse
{
    public class FutureverseSingleton : SingletonComponent<FutureverseSingleton>
    {
        public int requestTimeout = 60;
    }
}