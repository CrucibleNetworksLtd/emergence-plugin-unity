using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Services;
using EmergenceSDK.Internal.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UniVRM10;

namespace EmergenceSDK.EmergenceDemo
{
    public class DemoAvatarManager : SingletonComponent<DemoAvatarManager>
    {
        private Vrm10Instance vrm;
        private SkinnedMeshRenderer originalMesh;
        private Dictionary<Guid, CancellationTokenSource> cancellationTokenSources = new Dictionary<Guid, CancellationTokenSource>();

        public async void SwapAvatars(string vrmURL)
        {
            // Generate a unique operation ID
            var operationId = Guid.NewGuid();

            // Cancel all ongoing avatar swap operations
            CancelAllAvatarSwaps();

            // Set original mesh if it is not already set
            if (originalMesh == null)
            {
                originalMesh = GameObject.Find("PlayerArmature").GetComponentInChildren<SkinnedMeshRenderer>();
            }

            // Create a new cancellation token source for this operation
            var cts = new CancellationTokenSource();
            cancellationTokenSources[operationId] = cts;

            // Start the avatar swap task with the generated operation ID and token
            await SwapAvatarTask(operationId, vrmURL, cts.Token);
        }

        private void CancelAllAvatarSwaps()
        {
            foreach (var cts in cancellationTokenSources.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
            cancellationTokenSources.Clear();
        }

        private async UniTask SwapAvatarTask(Guid operationId, string vrmURL, CancellationToken ct)
        {
            try
            {
                var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, vrmURL, "");
                await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                byte[] response = request.downloadHandler.data;
                WebRequestService.CleanupRequest(request);

                ct.ThrowIfCancellationRequested();

                var newVRM = await Vrm10.LoadBytesAsync(response, true);
                if (newVRM.gameObject != null && vrm != null)
                {
                    Destroy(vrm.gameObject);
                }

                ct.ThrowIfCancellationRequested();

                vrm = newVRM;

                GameObject playerArmature = GameObject.Find("PlayerArmature");
                vrm.transform.position = playerArmature.transform.position;
                vrm.transform.rotation = playerArmature.transform.rotation;
                vrm.transform.parent = playerArmature.transform;
                vrm.name = "VRMAvatar";

                await UniTask.DelayFrame(1, cancellationToken: ct);

                UnityEngine.Avatar vrmAvatar = vrm.GetComponent<Animator>().avatar;
                playerArmature.GetComponent<Animator>().avatar = vrmAvatar;

                vrm.gameObject.GetComponent<Animator>().enabled = false;
                originalMesh.enabled = false;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Avatar swap operation was cancelled.");
            }
            finally
            {
                // Cleanup: Remove the CancellationTokenSource from the dictionary
                if (cancellationTokenSources.TryGetValue(operationId, out var source))
                {
                    cancellationTokenSources.Remove(operationId);
                    source.Dispose();
                }
            }
        }

        public void SetDefaultAvatar()
        {
            CancelAllAvatarSwaps();

            if (originalMesh == null)
            {
                originalMesh = GameObject.Find("PlayerArmature").GetComponentInChildren<SkinnedMeshRenderer>();
            }
            GameObject vrmAvatar = GameObject.Find("VRMAvatar");
            GameObject playerArmature = GameObject.Find("PlayerArmature");
            
            if (playerArmature == null)
            {
                playerArmature = Instantiate(Resources.Load<GameObject>("PlayerArmature"));
                playerArmature.name = "PlayerArmature";
            }

            originalMesh.enabled = true;
            playerArmature.GetComponent<Animator>().avatar = Resources.Load<UnityEngine.Avatar>("ArmatureAvatar");

            if (vrmAvatar != null)
            {
                Destroy(vrmAvatar);
            }
        }

#if UNITY_EDITOR
        // This method is called when the Unity Editor stops playing
        private void OnApplicationQuit()
        {
            CancelAllAvatarSwaps();
        }
#endif
    }
}
