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
        // Dictionary to keep track of original meshes for each game object
        private readonly Dictionary<GameObject, SkinnedMeshRenderer> originalMeshes = new();
        // Dictionary to manage cancellation tokens by operation IDs
        private readonly Dictionary<Guid, CancellationTokenSource> cancellationTokenSources = new();
        // Dictionary to keep track of operations associated with each armature
        private readonly Dictionary<GameObject, List<Guid>> armatureOperationGuids = new();

        // Public method to start avatar swapping
        public async UniTask SwapAvatars(GameObject playerArmature, string vrmURL)
        {
            // Cancel all ongoing avatar swap operations for the player armature
            CancelAvatarSwaps(playerArmature);

            // Set original mesh if it has not already been set
            if (!originalMeshes.TryGetValue(playerArmature, out _))
            {
                originalMeshes[playerArmature] = playerArmature.GetComponentInChildren<SkinnedMeshRenderer>();
            }

            // Create a cancellation token for the new swap operation
            var cts = CreateCancellationToken(playerArmature, out var operationId);

            // Perform the swap operation
            await SwapAvatarTask(playerArmature, operationId, vrmURL, cts.Token);
        }

        // Helper method to create a cancellation token for an operation
        private CancellationTokenSource CreateCancellationToken(GameObject playerArmature, out Guid operationId)
        {
            // Generate a unique operation ID
            operationId = Guid.NewGuid();
            // Create and store the cancellation token source
            var cts = new CancellationTokenSource();
            cancellationTokenSources[operationId] = cts;
            // Track the operation ID under the associated armature
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

        // Method to remove all cancellation tokens, optionally canceling them
        private void RemoveAllCancellationTokens(bool cancel = false)
        {
            // Iterate through all tokens and dispose them
            foreach (var cts in cancellationTokenSources.Values)
            {
                if (cancel && !cts.IsCancellationRequested)
                {
                    cts.Cancel();
                }
                cts.Dispose();
            }
            // Clear the collections after disposing the tokens
            cancellationTokenSources.Clear();
            armatureOperationGuids.Clear();
        }

        // Method to cancel all swap operations for a specific armature
        private void CancelAvatarSwaps(GameObject playerArmature)
        {
            // If there are ongoing operations for the armature, cancel them
            if (armatureOperationGuids.TryGetValue(playerArmature, out var guids))
            {
                for (var i = guids.Count - 1; i >= 0; i--)
                {
                    var guid = guids[i];
                    RemoveCancellationToken(guid, true);
                }
            }
        }

        // Asynchronous task to handle the avatar swapping logic
        private async UniTask SwapAvatarTask(GameObject playerArmature, Guid operationId, string vrmURL, CancellationToken ct)
        {
            try
            {
                // Start downloading the VRM file
                var request = UnityWebRequest.Get(vrmURL);
                byte[] response;
                using (request.uploadHandler)
                {
                    await request.SendWebRequest().ToUniTask(cancellationToken: ct);
                    response = request.downloadHandler.data;
                }

                // Check for cancellation
                ct.ThrowIfCancellationRequested();

                // Remove old VRM and load new one
                var oldVrm = playerArmature.GetComponentInChildren<Vrm10Instance>();
                var newVrm = await Vrm10.LoadBytesAsync(response, ct: ct);

                // Check again for cancellation
                ct.ThrowIfCancellationRequested();

                // Replace the old VRM with the new one
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

                // Set the new avatar to the animator
                Avatar vrmAvatar = newVrm.GetComponent<Animator>().avatar;
                playerArmature.GetComponent<Animator>().avatar = vrmAvatar;

                // Disable the animator on the new VRM game object
                newVrm.gameObject.GetComponent<Animator>().enabled = false;
                originalMeshes[playerArmature].enabled = false;
            }
            catch (OperationCanceledException)
            {
                // Log cancellation
                Debug.Log("Avatar swap operation was cancelled.");
            }
            finally
            {
                // Cleanup after operation is done or cancelled
                RemoveCancellationToken(operationId);
            }
        }

        // Helper method to remove a cancellation token
        private void RemoveCancellationToken(Guid guid, bool cancel = false)
        {
            if (cancellationTokenSources.Remove(guid, out var source))
            {
                if (cancel && !source.IsCancellationRequested)
                {
                    source.Cancel();
                }
                source.Dispose();
                // Clean up operation ID tracking
                foreach (var guids in armatureOperationGuids.Values)
                {
                    guids.Remove(guid);
                }
            }
        }

        // Method to reset avatar to default state
        public void SetDefaultAvatar(GameObject playerArmature = null)
        {
            // Cancel any ongoing swaps
            CancelAvatarSwaps(playerArmature);

            // Ensure original mesh is set
            SkinnedMeshRenderer originalMesh = null;
            if (playerArmature != null && !originalMeshes.TryGetValue(playerArmature, out originalMesh))
            {
                originalMesh = originalMeshes[playerArmature] = playerArmature.GetComponentInChildren<SkinnedMeshRenderer>();
            }

            // If no player armature is specified, instantiate a new one
            if (playerArmature == null)
            {
                playerArmature = Instantiate(Resources.Load<GameObject>("PlayerArmature"));
                playerArmature.name = "PlayerArmature";
            }

            // Assert that the original mesh is not null
            Assert.IsNotNull(originalMesh, "playerArmature must contain a SkinnedMeshRenderer");

            // Enable the original mesh and set the default avatar
            originalMesh.enabled = true;
            playerArmature.GetComponent<Animator>().avatar = Resources.Load<Avatar>("ArmatureAvatar");

            // Find and destroy the VRM avatar game object
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
            // Remove all cancellation tokens upon application quit
            RemoveAllCancellationTokens(true);
        }
#endif
    }
}
