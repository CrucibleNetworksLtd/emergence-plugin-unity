using UnityEngine;
using Newtonsoft.Json;

namespace Emergence
{
    public static class SerializationHelper
    {
        public static string Serialize<T>(T value, bool pretty = true)
        {
            // UnityEngine
            return JsonUtility.ToJson(value, pretty);

            // Newtonsoft Json.NET
            //return JsonConvert.SerializeObject(value, pretty ? Formatting.Indented : Formatting.None);
        }

        public static T Deserialize<T>(string serializedState)
        {
            // UnityEngine
            //return JsonUtility.FromJson<T>(serializedState);

            // Newtonsoft Json.NET
            return JsonConvert.DeserializeObject<T>(serializedState);
        }
    }
}