using System;
using EmergenceSDK.Implementations.Login.Types;
using UnityEngine.Events;

namespace EmergenceSDK.Implementations.Login.Events
{
    [Serializable] public class LoginEndedEvent : UnityEvent<LoginManager> {}
}