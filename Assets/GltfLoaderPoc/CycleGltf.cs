using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Avatars;
using EmergenceSDK.EmergenceDemo.DemoStations;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using GLTFast;
using Newtonsoft.Json;
using UniGLTF;
using UniGLTF.MeshUtility;
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
        [Serializable]
        public struct GltfModel
        {
            public TextAsset textAsset;
            public GltfLibrary compatibleLibraries;
        }
        
        [Flags]
        public enum GltfLibrary
        {
            UniGltf = 1,
            GltFast = 2
        }

        public GltfLibrary gltfLibrary = GltfLibrary.UniGltf;
        public Transform spawnPoint;
        public GltfModel[] binaryModels;
        public float networkSpeedMbps;
        private int currentIndex;
        private string loadedMeshName;
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
            return (Keyboard.current.eKey.wasPressedThisFrame && instructionsGO.activeSelf) || (Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended) || Input.GetKeyDown(KeyCode.Return);
        }

        private void Update()
        {
            if (HasBeenActivated() && !busy)
            {
                EmergenceLogger.LogInfo($"Cycle glTF", alsoLogToScreen: true);
                UniTask.Void(async () =>
                {
                    busy = true;

                    ClearSpawnedGltf();

                    var assetBytes = LoadGltfFile();
                    EmergenceLogger.LogInfo($"Loaded {assetBytes.Length/1000f/1000f:F2}MB", alsoLogToScreen: true);

                    await NetworkDownloadDelay(assetBytes);
                    
                    await SpawnGltf(assetBytes);
                        
                    busy = false;
                });
            }
        }

        private byte[] LoadGltfFile()
        {
            GltfModel binaryModel;
            int maxAttempts = binaryModels.Length;
            while (((binaryModel = binaryModels[currentIndex]).compatibleLibraries & gltfLibrary) != gltfLibrary && maxAttempts > 0)
            {
                currentIndex++;
                currentIndex %= binaryModels.Length;
                maxAttempts--;
            }

            var textAssetBytes = binaryModel.textAsset.bytes;
            currentIndex++;
            currentIndex %= binaryModels.Length;
            return textAssetBytes;
        }

        private void ClearSpawnedGltf()
        {
            foreach (var child in spawnPoint.GetChildren())
            {
                Destroy(child.gameObject);
            }
        }

        private async UniTask NetworkDownloadDelay(byte[] assetBytes)
        {
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
        }

        private async UniTask SpawnGltf(byte[] gltfBytes)
        {
            var sw = new Stopwatch();
            sw.Start();

            switch (gltfLibrary)
            {
                case GltfLibrary.UniGltf:
                    LoadWithUniGltf(gltfBytes);
                    break;
                case GltfLibrary.GltFast:
                    await LoadWithGltFast(gltfBytes);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            sw.Stop();
            EmergenceLogger.LogInfo($"Loading the mesh '{loadedMeshName}' took {sw.ElapsedMilliseconds / 1000f:F2}s with {gltfLibrary.ToString()}", alsoLogToScreen: true);
        }

        private async UniTask LoadWithGltFast(byte[] gltfBytes)
        {
            var gltf = new GltfImport();
            await gltf.LoadGltfBinary(
                gltfBytes
            ).AsUniTask();
            await gltf.InstantiateMainSceneAsync(spawnPoint).AsUniTask();
            foreach (var meshFilter in spawnPoint.GetComponentsInChildren<MeshFilter>())
            {
                var meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilter.sharedMesh;
            }
        }

        private void LoadWithUniGltf(byte[] gltfBytes)
        {
            var parser = new GlbBinaryParser(gltfBytes, null);
            using var parsed = parser.Parse();
            using var importer = new ImporterContext(parsed);
            var instance = importer.Load();
            var o = instance.gameObject;
            o.transform.parent = spawnPoint;
            o.transform.localPosition = Vector3.zero;
            foreach (var meshRenderer in instance.MeshRenderers)
            {
                var meshCollider = meshRenderer.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshRenderer.GetMesh();
            }
            instance.ShowMeshes();
        }
    }
}