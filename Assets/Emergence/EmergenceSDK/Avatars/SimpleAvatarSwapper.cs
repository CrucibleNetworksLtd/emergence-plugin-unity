using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UniVRM10;

namespace EmergenceSDK.Avatars
{
    public class SimpleAvatarSwapper : SingletonComponent<SimpleAvatarSwapper>
    {
        private readonly Dictionary<GameObject, SkinnedMeshRenderer> _originalMeshes = new();
        private readonly Dictionary<Guid, CancellationTokenSource> _cancellationTokenSources = new();
        private readonly Dictionary<GameObject, List<Guid>> _armatureOperationGuids = new();

        public async UniTask SwapAvatars(GameObject playerArmature, string vrmURL)
        {
            // Cancel all ongoing avatar swap operations
            CancelAvatarSwaps(playerArmature);

            // Set original mesh if it is not already set
            if (!_originalMeshes.TryGetValue(playerArmature, out _))
            {
                _originalMeshes[playerArmature] = playerArmature.GetComponentInChildren<SkinnedMeshRenderer>();
            }

            var cts = CreateCancellationToken(playerArmature, out var operationId);

            // Start the avatar swap task with the generated operation ID and token
            await SwapAvatarTask(playerArmature, operationId, vrmURL, cts.Token);
        }

        private CancellationTokenSource CreateCancellationToken(GameObject playerArmature, out Guid operationId)
        {
            // Generate a unique operation ID
            operationId = Guid.NewGuid();
            // Create a new cancellation token source for this operation
            var cts = new CancellationTokenSource();
            _cancellationTokenSources[operationId] = cts;
            if (_armatureOperationGuids.TryGetValue(playerArmature, out var guids))
            {
                guids.Add(operationId);
            }
            else
            {
                _armatureOperationGuids[playerArmature] = new List<Guid> { operationId };
            }

            return cts;
        }

        private void RemoveAllCancellationTokens(bool cancel = false)
        {
            foreach (var cts in _cancellationTokenSources.Values)
            {
                if (cancel && !cts.IsCancellationRequested)
                {
                    cts.Cancel();
                }
                cts.Dispose();
            }
            _cancellationTokenSources.Clear();
            _armatureOperationGuids.Clear();
        }

        private void CancelAvatarSwaps(GameObject playerArmature)
        {
            if (_armatureOperationGuids.TryGetValue(playerArmature, out var guids))
            {
                for (var i = guids.Count - 1; i >= 0; i--)
                {
                    var guid = guids[i];
                    RemoveCancellationToken(guid, true);
                }
            }
        }

        private async UniTask SwapAvatarTask(GameObject playerArmature, Guid operationId, string vrmURL, CancellationToken ct)
        {
            try
            {
                var request = UnityWebRequest.Get(vrmURL);
                byte[] response;
                using (request.uploadHandler)
                {
                    await request.SendWebRequest().ToUniTask(cancellationToken: ct);
                    response = request.downloadHandler.data;
                }

                ct.ThrowIfCancellationRequested();

                var oldVrm = playerArmature.GetComponentInChildren<Vrm10Instance>();
                var newVrm = await Vrm10.LoadBytesAsync(response, ct: ct);
                
                ct.ThrowIfCancellationRequested();
                
                if (newVrm.gameObject != null)
                {
                    if (oldVrm != null)
                    {
                        Destroy(oldVrm.gameObject);
                    }
                }
                
                var vrmTransform = newVrm.transform;
                vrmTransform.position = playerArmature.transform.position;
                vrmTransform.rotation = playerArmature.transform.rotation;
                vrmTransform.parent = playerArmature.transform;
                newVrm.name = "VRMAvatar";

                await UniTask.DelayFrame(1, cancellationToken: ct);

                Avatar vrmAvatar = newVrm.GetComponent<Animator>().avatar;
                playerArmature.GetComponent<Animator>().avatar = vrmAvatar;

                newVrm.gameObject.GetComponent<Animator>().enabled = false;
                _originalMeshes[playerArmature].enabled = false;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Avatar swap operation was cancelled.");
            }
            finally
            {
                // Cleanup: Remove the CancellationTokenSource from the dictionary
                RemoveCancellationToken(operationId);
            }
        }

        private void RemoveCancellationToken(Guid guid, bool cancel = false)
        {
            if (_cancellationTokenSources.Remove(guid, out var source))
            {
                if (cancel && !source.IsCancellationRequested)
                {
                    source.Cancel();
                }
                source.Dispose();
                foreach (var guids in _armatureOperationGuids.Values)
                {
                    guids.Remove(guid);
                }
            }
        }

        public void SetDefaultAvatar(GameObject playerArmature = null)
        {
            CancelAvatarSwaps(playerArmature);

            SkinnedMeshRenderer originalMesh = null;
            if (playerArmature != null && !_originalMeshes.TryGetValue(playerArmature, out originalMesh))
            {
                originalMesh = _originalMeshes[playerArmature] = playerArmature.GetComponentInChildren<SkinnedMeshRenderer>();
            }
            
            if (playerArmature == null)
            {
                playerArmature = Instantiate(Resources.Load<GameObject>("PlayerArmature"));
                playerArmature.name = "PlayerArmature";
            }
            
            Assert.IsNotNull(originalMesh, "playerArmature must contain a SkinnedMeshRenderer");

            originalMesh.enabled = true;
            playerArmature.GetComponent<Animator>().avatar = Resources.Load<Avatar>("ArmatureAvatar");
            
            GameObject FindChild(GameObject parent, string childName)
            {
                foreach (var child in parent.transform.GetChildren())
                {
                    if (child.name == childName)
                    {
                        return child.gameObject;
                    }

                    if (child.childCount > 0)
                    {
                        FindChild(child.gameObject, childName);
                    }
                }

                return null;
            }

            GameObject vrmAvatar = FindChild(playerArmature, "VRMAvatar");
            if (vrmAvatar != null)
            {
                Destroy(vrmAvatar);
            }
        }

#if UNITY_EDITOR
        // This method is called when the Unity Editor stops playing
        private void OnApplicationQuit()
        {
            RemoveAllCancellationTokens(true);
        }
#endif
    }
}
