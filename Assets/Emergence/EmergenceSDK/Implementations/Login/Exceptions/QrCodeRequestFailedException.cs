using System;
using EmergenceSDK.Types;
using UnityEngine;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    public sealed class QrCodeRequestFailedException : LoginStepRequestFailedException<Texture2D>
    {
        internal QrCodeRequestFailedException(string message, ServiceResponse<Texture2D> response = null, Exception exception = null) : base(message, response, exception) {}
    }
}