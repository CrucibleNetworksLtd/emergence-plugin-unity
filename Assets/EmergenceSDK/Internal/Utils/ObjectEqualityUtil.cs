using System;
using System.Reflection;
using Newtonsoft.Json;

namespace EmergenceSDK.Internal.Utils
{
    public static class ObjectEqualityUtil
    {
        public static bool AreObjectsEqual(object obj1, object obj2)
        {
            if (obj1 == null || obj2 == null)
            {
                return obj1 == obj2;
            }

            Type type = obj1.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (!field.IsDefined(typeof(JsonIgnoreAttribute), false))
                {
                    object value1 = field.GetValue(obj1);
                    object value2 = field.GetValue(obj2);

                    if (!FieldValuesAreEqual(value1, value2))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool FieldValuesAreEqual(object value1, object value2)
        {
            if (!Equals(value1, value2))
            {
                return false;
            }

            // If the field is a reference type, recursively check for equality
            if (value1 != null && !value1.GetType().IsValueType && !value1.GetType().IsPrimitive)
            {
                return AreObjectsEqual(value1, value2);
            }

            return true;
        }
    }
}