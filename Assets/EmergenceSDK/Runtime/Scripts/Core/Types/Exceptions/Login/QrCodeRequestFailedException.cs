using System;
using UnityEngine;

namespace EmergenceSDK.Runtime.Types.Exceptions.Login
{
    public sealed class QrCodeRequestFailedException : LoginStepRequestFailedException<Texture2D>
    {
        internal QrCodeRequestFailedException(string message, ServiceResponse<Texture2D> response = null, Exception exception = null) : base(message, response, exception) {}
    }
}