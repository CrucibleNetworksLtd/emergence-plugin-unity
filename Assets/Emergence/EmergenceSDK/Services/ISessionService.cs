using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;
using UnityEngine;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Service for interacting with the current Wallet Connect Session.
    /// </summary>
    public interface ISessionService : IEmergenceService
    {
        bool IsLoggedIn();
    }
}
