using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Avatars;
using EmergenceSDK.EmergenceDemo.DemoStations;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using Newtonsoft.Json;
using UniGLTF;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using VRMShaders;
using Debug = UnityEngine.Debug;
using TouchPhase = UnityEngine.TouchPhase;

namespace EmergenceSDK.GltfLoaderPoc
{
    public class CycleGltf : DemoStation<CycleGltf>, IDemoStation
    {
        public Transform spawnPoint;
        public string[] assetsUris;
        public float networkSpeedMbps;
        private int currentIndex;
        private bool busy;
        
        public bool IsReady
        {
            get => true;
            set
            {
                InstructionsText.text = ActiveInstructions;
                isReady = true;
            }
        }

        private void Start()
        {
            instructionsGO.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            instructionsGO.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            instructionsGO.SetActive(false);
        }

        protected override bool HasBeenActivated()
        {
            return (Keyboard.current.eKey.wasPressedThisFrame && instructionsGO.activeSelf) || (Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended);
        }

        private void Update()
        {
            if (HasBeenActivated() && !busy)
            {
                EmergenceLogger.LogInfo($"Cycle glTF", alsoLogToScreen: true);
                UniTask.Void(async () =>
                {
                    busy = true;

                    foreach (var child in spawnPoint.GetChildren())
                    {
                        Destroy(child.gameObject);
                    }

                    var assetBytes = Resources.Load<TextAsset>(assetsUris[currentIndex]).bytes;
                    currentIndex++;
                    currentIndex %= assetsUris.Length;                                    

                    EmergenceLogger.LogInfo($"Loaded {assetBytes.Length/1000f/1000f:F2}MB", alsoLogToScreen: true);

                    if (networkSpeedMbps > 0)
                    {
                        var bits = assetBytes.Length * 8;
                        var bitrate = networkSpeedMbps * 1000000;
                        var delay = (int)(bits / bitrate * 1000);
                        EmergenceLogger.LogInfo($"Simulated net delay: {delay / 1000f:F2}s", alsoLogToScreen: true);
                        await UniTask.Delay(delay);
                    }
                    else
                    {
                        EmergenceLogger.LogInfo($"No net delay.", alsoLogToScreen: true);
                    }
                    
                    var sw = new Stopwatch();
                    sw.Start();
                    var parser = new GlbBinaryParser(assetBytes, null);
                    using var parsed = parser.Parse();
                    using var importer = new ImporterContext(parsed);
                    var instance = importer.Load();
                    var o = instance.gameObject;
                    o.transform.parent = spawnPoint;
                    o.transform.localPosition = Vector3.zero;
                    instance.ShowMeshes();
                    sw.Stop();
                    
                    EmergenceLogger.LogInfo($"Loading the mesh took {sw.ElapsedMilliseconds / 1000f:F2}s.", alsoLogToScreen: true);
                        
                    busy = false;
                });
            }
        }
    }
}