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

        public async void SwapAvatars(string vrmURL)
        {
            //Set original mesh if it is not already set
            if (originalMesh == null)
            {
                originalMesh = GameObject.Find("PlayerArmature").GetComponentInChildren<SkinnedMeshRenderer>();
            }
            
            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, vrmURL, "");
            await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
            byte[] response = request.downloadHandler.data;
            WebRequestService.CleanupRequest(request);
            
            var newVRM = await Vrm10.LoadBytesAsync(response, true);
            if(newVRM.gameObject != null && vrm != null)
            {
                Destroy(vrm.gameObject);
            }
            vrm = newVRM;
            
            GameObject playerArmature = GameObject.Find("PlayerArmature");
            vrm.transform.position = playerArmature.transform.position;
            vrm.transform.rotation = playerArmature.transform.rotation;
            vrm.transform.parent = playerArmature.transform;
            vrm.name = "VRMAvatar";
            
            await UniTask.DelayFrame(1); 
            
            UnityEngine.Avatar vrmAvatar = vrm.GetComponent<Animator>().avatar;
            playerArmature.GetComponent<Animator>().avatar = vrmAvatar;

            vrm.gameObject.GetComponent<Animator>().enabled = false;
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

            originalMesh.enabled = true;
            playerArmature.GetComponent<Animator>().avatar = Resources.Load<UnityEngine.Avatar>("ArmatureAvatar");

            if (vrmAvatar != null)
            {
                Destroy(vrmAvatar);
            }
        }
    }
}
