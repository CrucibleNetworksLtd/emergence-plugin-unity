using System;
using UnityEngine.Events;

namespace EmergenceSDK.Runtime.Events.Login
{
    /// <summary>
    /// <list type="bullet">
    /// <listheader><term>Passed Parameters</term></listheader>
    /// <item><description><see cref="LoginManager"/> - The <see cref="LoginManager"/> that fired this event</description></item>
    /// </list>
    /// </summary>
    [Serializable] public sealed class LoginStartedEvent : UnityEvent<LoginManager> {}
}