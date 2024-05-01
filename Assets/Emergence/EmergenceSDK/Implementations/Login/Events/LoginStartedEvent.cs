using System;
using UnityEngine.Events;

namespace EmergenceSDK.Implementations.Login.Events
{
    [Serializable] public class LoginStartedEvent : UnityEvent<LoginManager> {}
}