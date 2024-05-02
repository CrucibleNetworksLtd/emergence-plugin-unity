using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace EmergenceSDK.Implementations.Login.Types
{
    public class EmergenceQrCode
    {
        public readonly Texture2D Texture;
        public float TimeLeft => Math.Max(0, LoginManager.QrCodeTimeout - (Time.realtimeSinceStartup - TimeIssued));
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
            LoginManager.StopCoroutine(DoTicks());
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