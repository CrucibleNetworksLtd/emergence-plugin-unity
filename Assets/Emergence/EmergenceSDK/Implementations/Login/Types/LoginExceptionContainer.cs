using System;

namespace EmergenceSDK.Implementations.Login.Types
{
    public struct LoginExceptionContainer
    {
        public readonly Exception Exception;
        public bool Handled { get; private set; }
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