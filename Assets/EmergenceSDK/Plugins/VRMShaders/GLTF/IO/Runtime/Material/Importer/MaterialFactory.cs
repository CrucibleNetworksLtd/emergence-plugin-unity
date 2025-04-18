﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace VRMShaders
{
    public delegate UniTask<Texture> GetTextureAsyncFunc(TextureDescriptor texDesc, IAwaitCaller awaitCaller);

    public class MaterialFactory : IResponsibilityForDestroyObjects
    {
        private readonly IReadOnlyDictionary<SubAssetKey, Material> m_externalMap;
        private readonly IReadOnlyDictionary<string, string> m_fallbackShaders;

        public MaterialFactory(IReadOnlyDictionary<SubAssetKey, Material> externalMaterialMap, IReadOnlyDictionary<string, string> fallbackShaders)
        {
            m_externalMap = externalMaterialMap;
            m_fallbackShaders = fallbackShaders;
        }

        public struct MaterialLoadInfo
        {
            public SubAssetKey Key;
            public readonly Material Asset;
            public readonly bool UseExternal;

            public bool IsSubAsset => !UseExternal;

            public MaterialLoadInfo(SubAssetKey key, Material asset, bool useExternal)
            {
                Key = key;
                Asset = asset;
                UseExternal = useExternal;
            }
        }

        List<MaterialLoadInfo> m_materials = new List<MaterialLoadInfo>();
        public IReadOnlyList<MaterialLoadInfo> Materials => m_materials;
        void Remove(Material material)
        {
            var index = m_materials.FindIndex(x => x.Asset == material);
            if (index >= 0)
            {
                m_materials.RemoveAt(index);

            }
        }

        public void Dispose()
        {
            foreach (var x in m_materials)
            {
                if (!x.UseExternal)
                {
                    // 外部の '.asset' からロードしていない
                    UnityObjectDestroyer.DestroyRuntimeOrEditor(x.Asset);
                }
            }
        }

        /// <summary>
        /// 所有権(Dispose権)を移譲する
        ///
        /// 所有権を移動する関数。
        ///
        /// * 所有権が移動する。return true => ImporterContext.Dispose の対象から外れる
        /// * 所有権が移動しない。return false => Importer.Context.Dispose でDestroyされる
        ///
        /// </summary>
        /// <param name="take"></param>
        public void TransferOwnership(TakeResponsibilityForDestroyObjectFunc take)
        {
            foreach (var x in m_materials.ToArray())
            {
                if (!x.UseExternal)
                {
                    // 外部の '.asset' からロードしていない
                    take(x.Key, x.Asset);
                    m_materials.Remove(x);
                }
            }
        }

        public Material GetMaterial(int index)
        {
            if (index < 0) return null;
            if (index >= m_materials.Count) return null;
            return m_materials[index].Asset;
        }

        public async UniTask<Material> LoadAsync(MaterialDescriptor matDesc, GetTextureAsyncFunc getTexture, IAwaitCaller awaitCaller)
        {
            if (m_externalMap.TryGetValue(matDesc.SubAssetKey, out Material material))
            {
                m_materials.Add(new MaterialLoadInfo(matDesc.SubAssetKey, material, true));
                return material;
            }

            if (getTexture == null)
            {
                getTexture = (x, y) => UniTask.FromResult<Texture>(null);
            }

            var shaderName = matDesc.ShaderName;
            if (String.IsNullOrEmpty(shaderName))
            {
                throw new Exception("no shader name");
            }
            if (m_fallbackShaders.TryGetValue(shaderName, out string fallback))
            {
                Debug.LogWarning($"fallback: {shaderName} => {fallback}");
                shaderName = fallback;
            }

            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                throw new Exception($"shader: {shaderName} not found");
            }

            material = new Material(shader);
            material.name = matDesc.SubAssetKey.Name;

            foreach (var kv in matDesc.TextureSlots)
            {
                var texture = await getTexture(kv.Value, awaitCaller);
                if (texture != null)
                {
                    material.SetTexture(kv.Key, texture);
                    SetTextureOffsetAndScale(material, kv.Key, kv.Value.Offset, kv.Value.Scale);
                }
            }

            foreach (var kv in matDesc.Colors)
            {
                material.SetColor(kv.Key, kv.Value);
            }

            foreach (var kv in matDesc.Vectors)
            {
                material.SetVector(kv.Key, kv.Value);
            }

            foreach (var kv in matDesc.FloatValues)
            {
                material.SetFloat(kv.Key, kv.Value);
            }

            if (matDesc.RenderQueue.HasValue)
            {
                material.renderQueue = matDesc.RenderQueue.Value;
            }

            foreach (var action in matDesc.Actions)
            {
                action(material);
            }

            m_materials.Add(new MaterialLoadInfo(matDesc.SubAssetKey, material, false));

            return material;
        }

        public static void SetTextureOffsetAndScale(Material material, string propertyName, Vector2 offset, Vector2 scale)
        {
            material.SetTextureOffset(propertyName, offset);
            material.SetTextureScale(propertyName, scale);
        }
    }
}
