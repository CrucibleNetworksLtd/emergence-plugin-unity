using System;
using EmergenceSDK.Implementations.Login.Exceptions;
using EmergenceSDK.Implementations.Login.Types;
using UnityEngine.Events;

namespace EmergenceSDK.Implementations.Login.Events
{
    /// <summary>
    /// <list type="bullet">
    /// <listheader><term>Passed Parameters</term></listheader>
    /// <item><description><see cref="LoginManager"/> - The <see cref="LoginManager"/> that fired this event</description></item>
    /// <item><description><see cref="LoginExceptionContainer"/> - The <see cref="LoginExceptionContainer"/> wrapping the exception that caused the failure</description></item>
    /// </list>
    /// <seealso cref="LoginExceptionContainer"/>
    /// <para>In the listeners for this event one should carefully check the <see cref="LoginExceptionContainer"/>, especially its <see cref="LoginExceptionContainer.Exception"/> as well as its <see cref="Exception.InnerException"/>.</para>
    /// <para>The presence of an <see cref="Exception.InnerException"/> implies that the step failed due to another exception, which might or might not be expected.</para>
    /// <para>For example, the <see cref="LoginStep.HandshakeRequest"/> <see cref="LoginStep"/> can fail with a <see cref="HandshakeRequestFailedException"/> wrapping a <see cref="TimeoutException"/>, (usually) implying the user hasn't scanned the QR code in time.</para>
    /// <para>Otherwise, any <see cref="LoginStepRequestFailedException"/> without an <see cref="Exception.InnerException"/> implies that the underlying web request has completed successfully with a failure HTTP response, for example a 404 code.</para>
    /// <para>Once the exception has been managed, you MUST call <see cref="LoginExceptionContainer.HandleException"/> on the passed <see cref="LoginExceptionContainer"/>, otherwise it will simply be rethrown</para>
    /// </summary>
    [Serializable] public class LoginFailedEvent : UnityEvent<LoginManager, LoginExceptionContainer> {}
}