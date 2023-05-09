using System;
using Newtonsoft.Json;
using UnityEngine;

namespace EmergenceSDK.Internal.Utils
{
    public static class SerializationHelper
    {
        public static string Serialize<T>(T value, bool pretty = true)
        {
            try
            {
                return JsonConvert.SerializeObject(value, pretty ? Formatting.Indented : Formatting.None);
            }
            catch (Exception e)
            {
                EmergenceLogger.LogError($"Error serializing {typeof(T).Name}: {e.Message}");
                throw;
            }
        }

        public static T Deserialize<T>(string serializedState)
        {
            try
            {  
                return JsonConvert.DeserializeObject<T>(serializedState);
            }
            catch (Exception e)
            {
                EmergenceLogger.LogError($"Error deserializing {typeof(T).Name}: {e.Message}");
                throw;
            }
        }
    }
}