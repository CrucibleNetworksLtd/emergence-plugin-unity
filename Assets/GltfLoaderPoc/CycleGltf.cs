using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using EmergenceSDK.EmergenceDemo.DemoStations;
using EmergenceSDK.Internal.Utils;
using GLTFast;
using UniGLTF;
using UniGLTF.MeshUtility;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.InputSystem;
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

#if UNITY_EDITOR
        [SerializeField,SingleEnumFlagSelect(EnumType = typeof(GltfLibrary))]
#endif
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
    
#if UNITY_EDITOR
    public class SingleEnumFlagSelectAttribute : PropertyAttribute
    {
        private Type enumType;

        public Type EnumType
        {
            get => enumType;
            set
            {
                if (value == null)
                {
                    EmergenceLogger.LogError($"{GetType().Name}: EnumType cannot be null");
                    return;
                }
                if (!value.IsEnum)
                {
                    EmergenceLogger.LogError($"{GetType().Name}: EnumType is {value.Name} this is not an enum");
                    return;
                }
                enumType = value;
                IsValid = true;
            }
        }
    
        public bool IsValid { get; private set; }
    }

    [CustomPropertyDrawer(typeof(SingleEnumFlagSelectAttribute))]
    public class SingleEnumFlagSelectAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, 
            SerializedProperty property, GUIContent label)
        {
            var singleEnumFlagSelectAttribute = 
                (SingleEnumFlagSelectAttribute)attribute;
            if (!singleEnumFlagSelectAttribute.IsValid)
            {
                return;
            }
            var displayTexts = new List<GUIContent>();
            var enumValues = new List<int>();
            foreach (var displayText in 
                     Enum.GetValues(singleEnumFlagSelectAttribute.EnumType))
            {
                displayTexts.Add(new GUIContent(displayText.ToString()));
                enumValues.Add((int)displayText);
            }

            property.intValue = EditorGUI.IntPopup(position, label, property.intValue,
                displayTexts.ToArray(), enumValues.ToArray());
        }
    }
#endif
}