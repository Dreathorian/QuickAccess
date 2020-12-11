using System;
using System.Collections.Generic;
using System.Text;

namespace QuickAccess
{
    public static class Extensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key,
            Func<TKey, TValue> function)
        {
            if (!dict.TryGetValue(key, out TValue value))
            {
                value = function(key);
                dict.Add(key, value);
            }

            return value;
        }

        public static bool IsSameOrSubclassOf(this Type derivedClass, Type baseClass)
        {
            return derivedClass != null && baseClass != null &&
                   (derivedClass == baseClass || derivedClass.IsSubclassOf(baseClass));
        }

        public static bool HasBaseDefinition(this Type givenType, Type definition)
        {
            if (!definition.IsGenericTypeDefinition)
                throw new ArgumentException($"{nameof(HasBaseDefinition)}: definition is not a GenericTypeDefinition");
            while (givenType != null)
            {
                if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == definition)
                {
                    return true;
                }

                givenType = givenType.BaseType;
            }

            return false;
        }

        public static bool HasBaseDefinition(this Type givenType, Type definition, out Type[] genericArguments)
        {
            if (!definition.IsGenericTypeDefinition)
                throw new ArgumentException($"{nameof(HasBaseDefinition)}: definition is not a GenericTypeDefinition");
            while (givenType != null)
            {
                if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == definition)
                {
                    genericArguments = givenType.GenericTypeArguments;
                    return true;
                }

                givenType = givenType.BaseType;
            }

            genericArguments = Array.Empty<Type>();
            return false;
        }

        public static Type GetArrayOrListItemType(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return type.GetGenericArguments()[0];
            if (type.IsArray) return type.GetElementType();
            return type;
        }

        public static string NiceName(this Type targetType, bool includeNamespace = false)
        {
            if (targetType.IsGenericType)
            {
                StringBuilder sb = new StringBuilder(includeNamespace
                    ? $"{targetType.Namespace}."
                    : "");
                sb.Append(targetType.Name.Split('`')[0]);
                NiceNameRecursion(targetType, ref sb);
                return sb.ToString();
            }

            return includeNamespace
                ? $"{targetType.Namespace}.{targetType.Name}"
                : targetType.Name;
        }

        private static void NiceNameRecursion(Type type, ref StringBuilder sb)
        {
            sb.Append("<");
            Type[] genericArguments = type.GetGenericArguments();
            for (int i = 0; i < genericArguments.Length; i++)
            {
                Type param = genericArguments[i];
                if (param.IsGenericType)
                {
                    sb.Append(param.Name.Split('`')[0]);
                    NiceNameRecursion(param, ref sb);
                }
                else
                {
                    sb.Append(param.Name);
                }

                if (i < genericArguments.Length - 1) sb.Append(", ");
            }

            sb.Append(">");
        }
    }
}