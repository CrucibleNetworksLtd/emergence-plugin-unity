using EmergenceSDK.Internal.Utils;
using UnityEngine;

namespace EmergenceSDK.EmergenceDemo.Scripts
{
    public abstract class DemoStation<T> : SingletonComponent<T> where T : SingletonComponent<T>
    {
    }
}