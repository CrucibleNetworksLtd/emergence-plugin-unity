#if UNITY_EDITOR

using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK.InternalTesting
{
    public class MasterBaseTestUI : BaseTestWindow
    {
        private Texture2D qrcode;
        private bool displayQR = false;
        private bool needToGenerateQR = true;

        [MenuItem("Window/Emergence Internal/In Game Test Panel")]
        private static void OpenWindow()
        {
            var desiredDockNextTo = typeof(Editor).Assembly.GetType("UnityEditor.ConsoleWindow");
            
            MasterBaseTestUI window = GetWindow<MasterBaseTestUI>("Sign In", desiredDockNextTo);
            window.Show();

            AvatarTesting avatarTesting = GetWindow<AvatarTesting>("Avatar Test Panel", desiredDockNextTo);
            avatarTesting.Show();
            
            ContractTesting contractTesting = GetWindow<ContractTesting>("Contract Test Panel", desiredDockNextTo);
            contractTesting.Show();
            
            ChainTesting chainTesting = GetWindow<ChainTesting>("Chain Test Panel", desiredDockNextTo);
            chainTesting.Show();
            
            PersonaTesting personaTesting = GetWindow<PersonaTesting>("Persona Test Panel", desiredDockNextTo);
            personaTesting.Show();
            
            WalletTesting walletTesting = GetWindow<WalletTesting>("Wallet Test Panel", desiredDockNextTo);
            walletTesting.Show();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("Please run the game to test EmergenceSDK");
                return;
            }
            needsCleanUp = true;

            if (IsLoggedIn())
            {
                EditorGUILayout.LabelField("Signed in as: " + EmergenceSingleton.Instance.GetCachedAddress());
                return;
            }

            EditorGUILayout.LabelField("QR");
            if (needToGenerateQR)
            {
                needToGenerateQR = false;
                EmergenceServiceProvider.GetService<ISessionServiceInternal>().GetQRCode(OnGetQRCodeSuccess, EmergenceLogger.LogError);
            }

            if (displayQR)
            {
                GUILayout.Label(qrcode);
            }
        }
        
        private void OnGetQRCodeSuccess(Texture2D qrcodeIn)
        {
            qrcode = qrcodeIn;
            displayQR = true;
            EmergenceServiceProvider.GetService<IWalletServiceInternal>().Handshake((walletAddress) =>
            {
                EmergenceLogger.LogInfo("Hand shook with wallet: " + walletAddress);
                displayQR = false;
                Repaint();
            },EmergenceLogger.LogError);

            Repaint();
        }
            
        protected override void CleanUp()
        {
            displayQR = false;
            needToGenerateQR = true;
        }
    }
}

#endif