#if UNITY_EDITOR

using System.Collections.Generic;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEditor;
using UnityEngine;
using Avatar = EmergenceSDK.Types.Avatar;

namespace EmergenceSDK
{
    public class AvatarTesting : BaseTestWindow
    {
        
        List<Avatar> avatars = new List<Avatar>();

        private void OnGUI()
        {
            if (!ReadyToTest(out var msg))
            {
                EditorGUILayout.LabelField(msg);
                return;
            }
            
            EditorGUILayout.LabelField("Test Avatar Service");

            if (GUILayout.Button("TestAvatarsByOwner"))
            {
                EmergenceServices.GetService<IAvatarService>().AvatarsByOwner(EmergenceSingleton.Instance.GetCachedAddress(), (avatarsIn) => avatars = avatarsIn, EmergenceLogger.LogError);
            }
            EditorGUILayout.LabelField("Retrieved Avatars:");
            foreach (var avatar in avatars)
            {
                EditorGUILayout.LabelField("Avatar: " + avatar.meta.name);
                EditorGUILayout.LabelField("Contract: " + avatar.contractAddress);
            }
        }

        protected override void CleanUp()
        {
            avatars.Clear();
        }
    }
}

#endif