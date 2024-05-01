using System;
using EmergenceSDK.Types;
using UnityEngine;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    internal class QrCodeRequestFailedException : Exception
    {
        public readonly ServiceResponse<Texture2D> Response;
        public QrCodeRequestFailedException(ServiceResponse<Texture2D> response)
        {
            Response = response;
        }

        public QrCodeRequestFailedException(Exception exception) : base(null, exception) { }
    }
}