using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace EmergenceSDK.Implementations.Login.Types
{
    /// <summary>
    /// Class for QR codes requested by the <see cref="LoginManager"/>
    /// This provides easy ways of getting the remaining time on the code before it expires, as well as adding an event on 
    /// </summary>
    public sealed class EmergenceQrCode
    {
        /// <summary>
        /// The <see cref="Texture2D"/> containing the image of the QR code. It is recommended to set <see cref="Texture2D.filterMode"/> to <see cref="FilterMode.Point"/>
        /// </summary>
        public readonly Texture2D Texture;
        /// <summary>
        /// The time left on the QR code, as a float
        /// </summary>
        public float TimeLeft => Math.Max(0, LoginManager.QrCodeTimeout - (Time.realtimeSinceStartup - TimeIssued));
        /// <summary>
        /// The time left on the QR code, as a rounded up integer
        /// </summary>
        public int TimeLeftInt => Mathf.CeilToInt(TimeLeft);

        internal readonly float TimeIssued;
        internal readonly string DeviceId;
        internal readonly LoginManager LoginManager;

        private bool _ticking = false;


        internal EmergenceQrCode(LoginManager loginManager, Texture2D texture, string deviceId)
        {
            LoginManager = loginManager;
            Texture = texture;
            DeviceId = deviceId;
            TimeIssued = Time.realtimeSinceStartup;
                
            StartTicking();
        }

        ~EmergenceQrCode()
        {
            StopTicking();
        }
            
        internal void StartTicking()
        {
            if (_ticking) return;
            _ticking = true;
            LoginManager.StartCoroutine(DoTicks());
        }

        internal void StopTicking()
        {
            if (!_ticking) return;
            _ticking = false;
            
            if (LoginManager != null) { LoginManager.StopCoroutine(DoTicks()); }
        }

        private IEnumerator DoTicks()
        {
            while (_ticking)
            {
                LoginManager.qrCodeTickEvent.Invoke(LoginManager, this);
                yield return new WaitForSecondsRealtime(1);
            }
        }
    }
}