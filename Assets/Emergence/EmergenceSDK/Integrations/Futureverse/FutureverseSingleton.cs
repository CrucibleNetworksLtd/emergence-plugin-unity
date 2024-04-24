using EmergenceSDK.Internal.Utils;

namespace EmergenceSDK.Integrations.Futureverse
{
    public class FutureverseSingleton : SingletonComponent<FutureverseSingleton>
    {
        public enum Environment
        {
            Development,
            Staging,
            Production
        }
        
        public Environment selectedEnvironment;
        public int requestTimeout = 60;
    }
}