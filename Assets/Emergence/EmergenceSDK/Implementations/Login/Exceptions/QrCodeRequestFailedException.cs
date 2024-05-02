using System;
using EmergenceSDK.Types;
using UnityEngine;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    public sealed class QrCodeRequestFailedException : LoginStepRequestFailedException<Texture2D>
    {
        internal QrCodeRequestFailedException(Exception exception) : base(null, exception) { }
        internal QrCodeRequestFailedException(ServiceResponse<Texture2D> response) : base(response) { }
    }
}