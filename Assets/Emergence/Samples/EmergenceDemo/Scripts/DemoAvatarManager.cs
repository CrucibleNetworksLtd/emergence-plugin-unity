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
        public async void SwapAvatars(string vrmURL)
        {
            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, vrmURL, "");
            await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
            byte[] response = request.downloadHandler.data;
            WebRequestService.CleanupRequest(request);

            var vrm10 = await Vrm10.LoadBytesAsync(response, true);
            GameObject playerArmature = GameObject.Find("PlayerArmature");
            
            if (playerArmature == null)
            {
                playerArmature = Instantiate(Resources.Load<GameObject>("PlayerArmature"));
                playerArmature.name = "PlayerArmature";
            }
            
            var originalMesh = playerArmature.GetComponentInChildren<SkinnedMeshRenderer>();
            vrm10.transform.position = playerArmature.transform.position;
            vrm10.transform.rotation = playerArmature.transform.rotation;
            vrm10.transform.parent = playerArmature.transform;
            vrm10.name = "VRMAvatar";
            
            await UniTask.DelayFrame(1);
            
            Avatar vrmAvatar = vrm10.GetComponent<Animator>().avatar;
            playerArmature.GetComponent<Animator>().avatar = vrmAvatar;

            vrm10.gameObject.GetComponent<Animator>().enabled = false;

            originalMesh.enabled = false;
        }
        
        public void SetDefaultAvatar()
        {
            GameObject vrmAvatar = GameObject.Find("VRMAvatar");
            GameObject playerArmature = GameObject.Find("PlayerArmature");
            
            if (playerArmature == null)
            {
                playerArmature = Instantiate(Resources.Load<GameObject>("PlayerArmature"));
                playerArmature.name = "PlayerArmature";
            }
            
            var originalMesh = playerArmature.GetComponentInChildren<SkinnedMeshRenderer>();

            originalMesh.enabled = true;
            playerArmature.GetComponent<Animator>().avatar = Resources.Load<UnityEngine.Avatar>("ArmatureAvatar");

            if (vrmAvatar != null)
            {
                Destroy(vrmAvatar);
            }
        }
    }
}
