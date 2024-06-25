using System;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace EmergenceSDK.Internal.Utils
{
    /// <summary>
    /// This class can be used for generating lists of all assembly members for documentation purposes.
    /// </summary>
    public class AssemblyInspector
    {
        public enum VisibilityLevel
        {
            Private = 0,
            Protected = 1,
            Public = 2
        }

        public static string DumpTypesAndMembers(string assemblyName,
            VisibilityLevel minVisibility = VisibilityLevel.Private, bool allowInternals = false,
            string[] blockedNamespacePatterns = null)
        {
            StringBuilder result = new StringBuilder();

            // Load the assembly
            Assembly assembly = Assembly.Load(assemblyName);

            // Get all types from the assembly
            Type[] types = assembly.GetTypes();

            var groupedByNamespace = types
                .Where(type => type.IsPublic)
                .GroupBy(type => type.Namespace)
                .OrderBy(group => group.Key);

            foreach (var namespaceGroup in groupedByNamespace)
            {
                if (blockedNamespacePatterns != null
                    && TryMatchPatternArray(blockedNamespacePatterns, namespaceGroup.Key)) continue;

                result.AppendLine(!string.IsNullOrEmpty(namespaceGroup.Key)
                    ? $"Namespace: {namespaceGroup.Key}"
                    : "Namespace: [Global]");

                foreach (Type type in namespaceGroup)
                {
                    if (!type.IsVisible && !allowInternals) continue;
                    if (GetTypeVisibilityLevel(type) < minVisibility) continue;

                    string typeCategory = GetTypeCategory(type);
                    string typeName = FormatTypeName(type);
                    string inheritance = FormatInheritance(type);
                    string implementations = FormatImplementations(type);
                    string @static = FormatStaticType(type);
                    var typeVisibility = GetTypeVisibilityLevel(type);
                    result.AppendLine(
                        $"\t{typeVisibility.ToString()} {@static}{typeCategory}: {typeName}{inheritance}{implementations}");

                    if (type.IsEnum)
                    {
                        Array enumValues = Enum.GetValues(type);
                        foreach (var value in enumValues)
                        {
                            result.AppendLine($"\t\t{Enum.GetName(type, value)} = {Convert.ToInt32(value)}");
                        }

                        continue;
                    }

                    FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                                        BindingFlags.Instance |
                                                        BindingFlags.Static | BindingFlags.DeclaredOnly);
                    foreach (FieldInfo field in fields)
                    {
                        var fieldVisibility = GetFieldVisibilityLevel(field);
                        if (field.IsAssembly && !allowInternals) continue;
                        if (fieldVisibility < minVisibility) continue;

                        result.AppendLine(
                            $"\t\t{fieldVisibility.ToString()} {(field.IsStatic ? "Static " : "")}Field: {FormatTypeName(field.FieldType)} {field.Name}");
                    }

                    PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic |
                                                                   BindingFlags.Instance |
                                                                   BindingFlags.Static | BindingFlags.DeclaredOnly);
                    foreach (PropertyInfo property in properties)
                    {
                        var isPropertyInternal = IsPropertyInternal(property);
                        if (isPropertyInternal && !allowInternals) continue;
                        var propertyVisibilityLevel = GetPropertyVisibilityLevel(property);
                        if (propertyVisibilityLevel < minVisibility) continue;
                        
                        var propertyAccessorsString = BuildPropertyAccessorsString(property);

                        result.Append(
                            $"\t\t{propertyVisibilityLevel.ToString()}{FormatPropertyStatic(property)} Property: {FormatTypeName(property.PropertyType)} {property.Name} {propertyAccessorsString}");

                        result.AppendLine();
                    }

                    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                           BindingFlags.Instance |
                                                           BindingFlags.Static | BindingFlags.DeclaredOnly);
                    foreach (MethodInfo method in methods)
                    {
                        if (method.IsSpecialName) continue;
                        var methodVisibilityLevel = GetMethodVisibilityLevel(method);
                        if (methodVisibilityLevel < minVisibility) continue;
                        if (method.IsAssembly && !allowInternals) continue;

                        string methodName = FormatMethodName(method);
                        result.AppendLine(
                            $"\t\t{methodVisibilityLevel.ToString()}{FormatMethodStatic(method)}{FormatMethodVirtualOverride(method)} Method: {methodName}");
                    }
                }
            }

            return result.ToString();
        }
        
        public static string DumpFunctionsToCsv(string assemblyName,
            VisibilityLevel minVisibility = VisibilityLevel.Private, bool allowInternals = false,
            string[] blockedNamespacePatterns = null)
        {
            StringBuilder result = new StringBuilder();

            // Load the assembly
            Assembly assembly = Assembly.Load(assemblyName);

            // Get all types from the assembly
            Type[] types = assembly.GetTypes();

            var groupedByNamespace = types
                .Where(type => type.IsPublic)
                .GroupBy(type => type.Namespace)
                .OrderBy(group => group.Key);

            foreach (var namespaceGroup in groupedByNamespace)
            {
                if (blockedNamespacePatterns != null
                    && TryMatchPatternArray(blockedNamespacePatterns, namespaceGroup.Key)) continue;

                foreach (Type type in namespaceGroup)
                {
                    if (!type.IsVisible && !allowInternals) continue;
                    if (GetTypeVisibilityLevel(type) < minVisibility) continue;
                    
                    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                           BindingFlags.Instance |
                                                           BindingFlags.Static | BindingFlags.DeclaredOnly);
                    foreach (MethodInfo method in methods)
                    {
                        if (method.IsSpecialName) continue;
                        var methodVisibilityLevel = GetMethodVisibilityLevel(method);
                        if (methodVisibilityLevel < minVisibility) continue;
                        if (method.IsAssembly && !allowInternals) continue;

                        string methodName = FormatMethodName(method);
                        result.AppendLine(
                            $"\"{method.Name}\",\"{namespaceGroup.Key} -> {type.Name}\"");
                    }
                }
            }

            return result.ToString();
        }

        private static bool TryMatchPatternArray(string[] patterns, string input)
        {
            foreach (var pattern in patterns)
            {
                if (Regex.IsMatch(input, pattern)) return true;
            }

            return false;
        }

        private static bool IsPropertyInternal(PropertyInfo property)
        {
            MethodInfo getMethod = property.GetGetMethod(true);
            MethodInfo setMethod = property.GetSetMethod(true);

            // Is property internal, there can't be zero accessors so if one is missing consider it internal for this comparison
            var isPropertyInternal = (getMethod?.IsAssembly ?? true) && (setMethod?.IsAssembly ?? true);
            return isPropertyInternal;
        }

        private static string FormatPropertyStatic(PropertyInfo property)
        {
            return ((property.GetSetMethod(true) ?? property.GetGetMethod(true)).IsStatic ? " Static" : "");
        }

        private static string FormatMethodStatic(MethodInfo method)
        {
            return (method.IsStatic ? " Static" : "");
        }

        private static string FormatMethodVirtualOverride(MethodInfo method)
        {
            if (method.IsVirtual)
            {
                return method.GetBaseDefinition().DeclaringType != method.DeclaringType ? " Override" : " Virtual";
            }

            return "";
        }

        private static string FormatStaticType(Type type)
        {
            if (type.IsClass)
            {
                if (type.IsSealed && type.IsAbstract)
                {
                    return "Static ";
                }

                if (type.IsSealed)
                {
                    return "Sealed ";
                }

                if (type.IsAbstract)
                {
                    return "Abstract ";
                }
            }

            return "";
        }

        private static string GetTypeCategory(Type type)
        {
            if (type.IsClass) return "Class";
            if (type.IsEnum) return "Enum";
            if (type.IsValueType && !type.IsEnum) return "Struct";
            if (type.IsInterface) return "Interface";
            return "Type";
        }

        private static string FormatTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                string name = type.Namespace + "." + type.GetGenericTypeDefinition().Name;
                name = name.Substring(0, name.IndexOf('`'));
                string genericArguments = string.Join(", ", type.GetGenericArguments().Select(arg => arg.Name));
                return $"{name}<{genericArguments}>";
            }
            else if (type.IsArray)
            {
                return $"{FormatTypeName(type.GetElementType())}[]";
            }
            else if (type.IsByRef)
            {
                return $"{FormatTypeName(type.GetElementType())}&";
            }
            else
            {
                return type.FullName;
            }
        }

        private static string FormatMethodName(MethodInfo method)
        {
            string name = method.Name;
            if (method.IsGenericMethod)
            {
                string genericArguments = string.Join(", ", method.GetGenericArguments().Select(arg => arg.Name));
                name += $"<{genericArguments}>";
            }

            string parameters = string.Join(", ", method.GetParameters().Select(FormatParameter));
            return $"{name}({parameters})";
        }

        private static string FormatParameter(ParameterInfo parameter)
        {
            string paramType = FormatTypeName(parameter.ParameterType);
            if (parameter.IsOut)
            {
                paramType = $"out {paramType.TrimEnd('&')}";
            }
            else if (parameter.ParameterType.IsByRef)
            {
                paramType = $"ref {paramType.TrimEnd('&')}";
            }
            else if (parameter.IsIn)
            {
                paramType = $"in {paramType.TrimEnd('&')}";
            }

            return $"{paramType} {parameter.Name}";
        }

        private static string FormatInheritance(Type type)
        {
            var baseType = type.BaseType;
            if (type.IsClass && baseType != null && baseType != typeof(Object))
            {
                return " inherits " + FormatTypeName(baseType);
            }

            return "";
        }

        private static string FormatImplementations(Type type)
        {
            var inheritedInterfaces = type.GetInterfaces();
            if ((type.IsClass || (type.IsValueType && !type.IsEnum)) && inheritedInterfaces.Length > 0)
            {
                string[] interfaceNames = new string[inheritedInterfaces.Length];
                for (int i = 0; i < inheritedInterfaces.Length; i++)
                {
                    interfaceNames[i] = FormatTypeName(inheritedInterfaces[i]);
                }

                return " implements " + string.Join(", ", interfaceNames);
            }

            return "";
        }

        static VisibilityLevel GetTypeVisibilityLevel(Type type, VisibilityLevel? nestedVisibilityOverride = null)
        {
            if (!type.IsNested)
            {
                return type.IsPublic
                    ? nestedVisibilityOverride ?? VisibilityLevel.Public
                    : VisibilityLevel.Private;
            }

            if (type.IsNestedPrivate)
            {
                return VisibilityLevel.Private;
            }

            if (type.IsNestedFamily)
            {
                return GetTypeVisibilityLevel(type.DeclaringType, VisibilityLevel.Protected);
            }

            if (type.IsNestedPublic)
            {
                return GetTypeVisibilityLevel(type.DeclaringType, nestedVisibilityOverride ?? VisibilityLevel.Public);
            }

            return VisibilityLevel.Private;
        }

        static VisibilityLevel GetMethodVisibilityLevel(MethodInfo method)
        {
            if (method?.IsPrivate ?? true)
            {
                return VisibilityLevel.Private;
            }

            return method.IsPublic ? VisibilityLevel.Public : VisibilityLevel.Protected;
        }

        static VisibilityLevel GetPropertyVisibilityLevel(PropertyInfo property)
        {
            MethodInfo getMethod = property.GetGetMethod(true);
            MethodInfo setMethod = property.GetSetMethod(true);
            return (VisibilityLevel)Math.Max((int)GetMethodVisibilityLevel(getMethod),
                (int)GetMethodVisibilityLevel(setMethod));
        }

        static VisibilityLevel GetFieldVisibilityLevel(FieldInfo field)
        {
            if (field?.IsPrivate ?? true)
            {
                return VisibilityLevel.Private;
            }

            return field.IsPublic ? VisibilityLevel.Public : VisibilityLevel.Protected;
        }

        static string BuildPropertyAccessorsString(PropertyInfo property)
        {
            MethodInfo getMethod = property.GetGetMethod(true);
            MethodInfo setMethod = property.GetSetMethod(true);

            var isPropertyInternal = IsPropertyInternal(property);
            var getVisibilityLevel = GetMethodVisibilityLevel(getMethod);
            var setVisibilityLevel = GetMethodVisibilityLevel(setMethod);
            bool internalGetter = getMethod?.IsAssembly ?? false;
            bool internalSetter = setMethod?.IsAssembly ?? false;

            var accessorsStringBuilder = new StringBuilder("{ ");
            var propertyVisibilityLevel = GetPropertyVisibilityLevel(property);

            if (getMethod != null)
            {
                if (getVisibilityLevel != propertyVisibilityLevel)
                {
                    accessorsStringBuilder.Append(getVisibilityLevel.ToString().ToLower());
                    accessorsStringBuilder.Append(" ");
                }

                if (internalGetter && !isPropertyInternal)
                {
                    accessorsStringBuilder.Append("internal ");
                }

                accessorsStringBuilder.Append("get;");
            }

            if (setMethod != null)
            {
                if (getMethod != null)
                {
                    accessorsStringBuilder.Append(" ");
                }

                if (setMethod != null)
                {
                    if (setVisibilityLevel != propertyVisibilityLevel)
                    {
                        accessorsStringBuilder.Append(setVisibilityLevel.ToString().ToLower());
                        accessorsStringBuilder.Append(" ");
                    }

                    if (internalSetter && !isPropertyInternal)
                    {
                        accessorsStringBuilder.Append("internal ");
                    }

                    accessorsStringBuilder.Append("set;");
                }
            }

            accessorsStringBuilder.Append(" }");
            return accessorsStringBuilder.ToString();
        }
    }
}