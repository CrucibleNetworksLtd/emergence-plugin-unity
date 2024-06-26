using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EmergenceSDK.Runtime.Internal.Utils
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
                if (!typeof(T).IsDefined(typeof(StoreOriginalJTokensAttribute)))
                {
                    return JsonConvert.DeserializeObject<T>(serializedState);
                }
                
                return DeserializeObjectWithOriginalJTokens<T>(serializedState);
            }
            catch (Exception e)
            {
                EmergenceLogger.LogError($"Error deserializing {typeof(T).Name}: {e.Message}");
                throw;
            }
        }

        public static T Deserialize<T>(JToken jToken)
        {
            try
            {
                if (!typeof(T).IsDefined(typeof(StoreOriginalJTokensAttribute)))
                {
                    return jToken.ToObject<T>();
                }
                
                return DeserializeObjectWithOriginalJTokens<T>(jToken);
            }
            catch (Exception e)
            {
                EmergenceLogger.LogError($"Error deserializing {typeof(T).Name}: {e.Message}");
                throw;
            }
        }

        public static JToken Parse(string jsonString)
        {
            try
            {  
                return JToken.Parse(jsonString);
            }
            catch (Exception e)
            {
                EmergenceLogger.LogError($"Error parsing string: {e.Message}");
                throw;
            }
        }
        
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
        public class OriginalJTokenAttribute : Attribute { }
        
        [AttributeUsage(AttributeTargets.Class)]
        public class StoreOriginalJTokensAttribute : Attribute { }
        
        private static T DeserializeObjectWithOriginalJTokens<T>(string serializedState)
        {
            return DeserializeObjectWithOriginalJTokens<T>(JToken.Parse(serializedState));
        }
        
        private static T DeserializeObjectWithOriginalJTokens<T>(JToken jToken)
        {
            try
            {  
                var result = jToken.ToObject<T>();

                PopulateOriginalJsonPropertiesAndFields(typeof(T), result, jToken);

                return result;
            }
            catch (Exception e)
            {
                EmergenceLogger.LogError($"Error deserializing {typeof(T).Name}: {e.Message}");
                throw;
            }
        }
        
        private static void PopulateOriginalJsonPropertiesAndFields(IReflect type, object obj, JToken token)
        {
            var propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var property in propertyInfos)
            {
                if (IsPropertyOriginalJToken(property))
                {
                    property.SetValue(obj, token, null);
                }
                else if (IsPropertyDeserializable(property) && token is JObject)
                {
                    var propertyType = property.PropertyType;
                    var propertyValue = property.GetValue(obj);
                    var childToken = token[property.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? property.Name];

                    // Handle lists
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        HandleListPropertyOrField(propertyType, propertyValue, childToken);
                    }
                    // Handle dictionaries
                    else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>) && propertyValue != null)
                    {
                        HandleDictionaryPropertyOrField(propertyType, childToken, propertyValue);
                    }
                    // Recursive call for nested properties
                    else if (propertyType.IsClass && childToken != null && propertyValue != null)
                    {
                        PopulateOriginalJsonPropertiesAndFields(propertyType, propertyValue, childToken);
                    }
                }
            }

            var fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fieldInfos)
            {
                if (IsFieldOriginalJToken(field))
                {
                    field.SetValue(obj, token);
                }
                else if (IsFieldDeserializable(field) && token is JObject)
                {
                    var fieldType = field.FieldType;
                    var fieldValue = field.GetValue(obj);
                    var childToken = token[field.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? field.Name];

                    // Handle lists
                    if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        HandleListPropertyOrField(fieldType, fieldValue, childToken);
                    }
                    // Handle dictionaries
                    else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>) && fieldValue != null)
                    {
                        HandleDictionaryPropertyOrField(fieldType, childToken, fieldValue);
                    }
                    // Recursive call for nested properties
                    else if (fieldType.IsClass && childToken != null && fieldValue != null)
                    {
                        PopulateOriginalJsonPropertiesAndFields(fieldType, fieldValue, childToken);
                    }
                }
            }
        }

        private static bool IsPropertyDeserializable(PropertyInfo property)
        {
            return !property.IsDefined(typeof(JsonIgnoreAttribute)) && property.SetMethod != null && (property.SetMethod.IsPublic || property.IsDefined(typeof(JsonPropertyAttribute)));
        }

        private static bool IsFieldDeserializable(FieldInfo field)
        {
            return !field.IsDefined(typeof(JsonIgnoreAttribute)) && (field.IsPublic || field.IsDefined(typeof(JsonPropertyAttribute)));
        }

        private static bool IsFieldOriginalJToken(FieldInfo field)
        {
            return field.IsDefined(typeof(OriginalJTokenAttribute)) && field.FieldType.IsAssignableFrom(typeof(JToken));
        }

        private static bool IsPropertyOriginalJToken(PropertyInfo property)
        {
            return property.IsDefined(typeof(OriginalJTokenAttribute)) && property.CanWrite && property.PropertyType.IsAssignableFrom(typeof(JToken));
        }

        private static void HandleDictionaryPropertyOrField(Type propertyType, JToken childToken, object propertyValue)
        {
            var keyType = propertyType.GetGenericArguments()[0];
            var valueType = propertyType.GetGenericArguments()[1];
            if (keyType == typeof(string) && valueType.IsClass && childToken is JObject jObject)
            {
                foreach (var kvp in (IDictionary)propertyValue)
                {
                    var key = kvp.GetType().GetProperty("Key")?.GetValue(kvp, null);
                    var value = kvp.GetType().GetProperty("Value")?.GetValue(kvp, null);
                    if (key is string keyString && jObject[keyString] is { } valueToken)
                    {
                        PopulateOriginalJsonPropertiesAndFields(valueType, value, valueToken);
                    }
                }
            }
        }

        private static void HandleListPropertyOrField(Type propertyType, object propertyValue, JToken childToken)
        {
            if (propertyType.GetGenericArguments()[0].IsClass && propertyValue != null && childToken is JArray jArray)
            {
                var itemType = propertyType.GetGenericArguments()[0];
                int index = 0;
                foreach (var item in (IList)propertyValue)
                {
                    if (index < jArray.Count)
                    {
                        PopulateOriginalJsonPropertiesAndFields(itemType, item, jArray[index]);
                    }
                    index++;
                }
            }
        }
    }
}