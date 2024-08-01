using System;
using EmergenceSDK.Runtime.Types;
using UnityEngine.Events;

namespace EmergenceSDK.Runtime.Events.Login
{
    /// <summary>
    /// <list type="bullet">
    /// <listheader><term>Passed Parameters</term></listheader>
    /// <item><description><see cref="LoginManager"/> - The <see cref="LoginManager"/> that fired this event</description></item>
    /// <item><description><see cref="LoginStep"/> - The current <see cref="LoginStep"/></description></item>
    /// <item><description><see cref="StepPhase"/> - The current <see cref="StepPhase"/> of the current <see cref="LoginStep"/></description></item>
    /// </list>
    /// <seealso cref="LoginStep"/>
    /// <seealso cref="StepPhase"/>
    /// </summary>
    [Serializable] public sealed class LoginStepUpdatedEvent : UnityEvent<LoginManager, LoginStep, StepPhase> {}
}