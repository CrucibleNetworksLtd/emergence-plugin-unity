using System;

namespace EmergenceSDK.Implementations.Login.Types
{
    /// <summary>
    /// A container for all exceptions passed to <see cref="LoginManager.loginFailedEvent"/>
    /// </summary>
    public sealed class LoginExceptionContainer
    {
        /// <summary>
        /// The underlying exception
        /// </summary>
        public readonly Exception Exception;
        /// <summary>
        /// Whether the exception has been marked as handled
        /// </summary>
        public bool Handled { get; private set; }
        /// <summary>
        /// Marks this exception as handled, or it will be rethrown when exiting the event.
        /// </summary>
        public void HandleException()
        {
            Handled = true;
        }

        internal void ThrowIfUnhandled()
        {
            if (Handled) return;
            throw Exception;
        }

        internal LoginExceptionContainer(Exception exception)
        {
            Exception = exception;
            Handled = false;
        }
    }
}