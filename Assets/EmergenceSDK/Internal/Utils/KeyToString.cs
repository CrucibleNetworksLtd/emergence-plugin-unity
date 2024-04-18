using UnityEngine;

namespace EmergenceSDK.Internal.Utils
{
    public static class KeyToString
    {
        private static KeyCode[] keyCodes;

        static KeyToString()
        {
            keyCodes = System.Enum.GetValues(typeof(KeyCode)) as KeyCode[];
        }

        public static string GetCurrentAlphaKey()
        {
            string result = string.Empty;
            for (int i = 0; i < keyCodes.Length; i++)
            {
                // Only alpha chars
                if (keyCodes[i] >= KeyCode.A && keyCodes[i] <= KeyCode.Z)
                {
                    if (Input.GetKeyDown(keyCodes[i]))
                    {
                        result = keyCodes[i].ToString().ToLower();
                        break;
                    }
                }
            }

            return result;
        }
    }
}