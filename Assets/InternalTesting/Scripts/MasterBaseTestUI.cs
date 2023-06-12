#if UNITY_EDITOR

using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK
{
    public class MasterBaseTestUI : BaseTestWindow
    {
        private Texture2D qrcode;
        private bool displayQR = false;
        private bool displayFullTestUI = false;
        
        [MenuItem("Window/Emergence Internal/In Game Test Panel")]
        private static void OpenWindow()
        {
            MasterBaseTestUI window = GetWindow<MasterBaseTestUI>("Sign In", typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
            window.Show();

            AvatarTesting avatarTesting = GetWindow<AvatarTesting>("Avatar Test Panel", typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
            avatarTesting.Show();
            
            ContractTesting contractTesting = GetWindow<ContractTesting>("Contract Test Panel", typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
            contractTesting.Show();
            
            ChainTesting chainTesting = GetWindow<ChainTesting>("Chain Test Panel", typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
            chainTesting.Show();
        }

        private void OnGUI()
        {
            if (!ReadyToTest(out var msg))
            {
                EditorGUILayout.LabelField(msg);
                return;
            }

            if (!displayFullTestUI)
            {
                EditorGUILayout.LabelField("QR");
                if (GUILayout.Button("Generate QR"))
                {
                    EmergenceServices.GetService<ISessionService>().GetQRCode(OnGetQRCodeSuccess, EmergenceLogger.LogError);
                }
    
                if (displayQR)
                {
                    GUILayout.Label(qrcode);
                }
            }
        }
        
        private void OnGetQRCodeSuccess(Texture2D qrcodeIn)
        {
            qrcode = qrcodeIn;
            displayQR = true;
            EmergenceServices.GetService<IWalletService>().Handshake((walletAddress) =>
            {
                EmergenceLogger.LogInfo("Hand shook with wallet: " + walletAddress);
                displayQR = false;
                displayFullTestUI = true;
                Repaint();
            },EmergenceLogger.LogError);
            Repaint();
        }
            
        protected override void CleanUp()
        {
            displayQR = false;
            displayFullTestUI = false;
        }
    }
}

#endif