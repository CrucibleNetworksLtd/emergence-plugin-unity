using UnityEngine;

namespace EmergenceSDK
{
    public class EmergenceServiceProviderInitializer : MonoBehaviour
    {
        private void Awake()
        {
            EmergenceServiceProvider.Load();
        }
    }
}
