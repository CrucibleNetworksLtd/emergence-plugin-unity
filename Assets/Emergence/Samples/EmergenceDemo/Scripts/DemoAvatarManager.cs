using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UniVRM10;

namespace EmergenceSDK.EmergenceDemo
{
    public class DemoAvatarManager : SingletonComponent<DemoAvatarManager>
    {
        private SkinnedMeshRenderer originalMesh;
        private Dictionary<Guid, CancellationTokenSource> cancellationTokenSources = new();
        private Dictionary<GameObject, List<Guid>> armatureOperationGuids = new();

        public async void SwapAvatars(GameObject playerArmature, string vrmURL)
        {
            // Cancel all ongoing avatar swap operations
            CancelAvatarSwaps(playerArmature);

            // Set original mesh if it is not already set
            if (originalMesh == null)
            {
                originalMesh = playerArmature.GetComponentInChildren<SkinnedMeshRenderer>();
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
            cancellationTokenSources[operationId] = cts;
            if (armatureOperationGuids.TryGetValue(playerArmature, out var guids))
            {
                guids.Add(operationId);
            }
            else
            {
                armatureOperationGuids[playerArmature] = new List<Guid> { operationId };
            }

            return cts;
        }

        private void RemoveAllCancellationTokens(bool cancel = false)
        {
            foreach (var cts in cancellationTokenSources.Values)
            {
                if (cancel && !cts.IsCancellationRequested)
                {
                    cts.Cancel();
                }
                cts.Dispose();
            }
            cancellationTokenSources.Clear();
            armatureOperationGuids.Clear();
        }

        private void CancelAvatarSwaps(GameObject playerArmature)
        {
            if (armatureOperationGuids.TryGetValue(playerArmature, out var guids))
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
                var newVrm = await Vrm10.LoadBytesAsync(response, true, ct: ct);
                
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
                originalMesh.enabled = false;
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
            if (cancellationTokenSources.Remove(guid, out var source))
            {
                if (cancel && !source.IsCancellationRequested)
                {
                    source.Cancel();
                }
                source.Dispose();
                foreach (var guids in armatureOperationGuids.Values)
                {
                    guids.Remove(guid);
                }
            }
        }

        public void SetDefaultAvatar(GameObject playerArmature = null)
        {
            CancelAvatarSwaps(playerArmature);

            if (playerArmature != null && originalMesh == null)
            {
                originalMesh = playerArmature.GetComponentInChildren<SkinnedMeshRenderer>();
            }
            
            if (playerArmature == null)
            {
                playerArmature = Instantiate(Resources.Load<GameObject>("PlayerArmature"));
                playerArmature.name = "PlayerArmature";
            }

            originalMesh.enabled = true;
            playerArmature.GetComponent<Animator>().avatar = Resources.Load<UnityEngine.Avatar>("ArmatureAvatar");
            
            GameObject FindChild(GameObject parent, string name)
            {
                foreach (var child in parent.transform.GetChildren())
                {
                    if (child.name == name)
                    {
                        return child.gameObject;
                    }

                    if (child.childCount > 0)
                    {
                        FindChild(child.gameObject, name);
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
