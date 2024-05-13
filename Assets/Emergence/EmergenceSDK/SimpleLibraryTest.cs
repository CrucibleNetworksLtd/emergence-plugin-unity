using System.Runtime.InteropServices;

namespace EmergenceSDK
{
    public class SimpleLibraryTest
    {
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport ("SimpleLibrary")]
        private static extern int aotsample_add(int a, int b);
        
        [DllImport ("SimpleLibrary")]
        private static extern string aotsample_sumstring(string a, string b);

        public static int Add(int a, int b)
        {
            return aotsample_add(a, b);
        }

        public static string Concat(string a, string b)
        {
            return aotsample_sumstring(a, b);
        }
#endif
    }
}