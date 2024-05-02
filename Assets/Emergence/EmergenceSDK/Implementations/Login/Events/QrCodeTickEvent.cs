using System;
using EmergenceSDK.Implementations.Login.Types;
using UnityEngine.Events;

namespace EmergenceSDK.Implementations.Login.Events
{
    /// <summary>
    /// <list type="bullet">
    /// <listheader><term>Passed Parameters</term></listheader>
    /// <item><description><see cref="LoginManager"/> - The <see cref="LoginManager"/> that owns the <see cref="EmergenceQrCode"/> that fired this event</description></item>
    /// <item><description><see cref="EmergenceQrCode"/> - The <see cref="EmergenceQrCode"/> that fired this event</description></item>
    /// </list>
    /// </summary>
    [Serializable] public class QrCodeTickEvent : UnityEvent<LoginManager, EmergenceQrCode> {}
}