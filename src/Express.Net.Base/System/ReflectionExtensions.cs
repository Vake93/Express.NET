using System;
using System.Linq;
using System.Reflection;

namespace Express.Net.System
{
    internal static class ReflectionExtensions
    {
        public static CustomAttributeData? GetCustomAttributeData(this MemberInfo memberInfo, Type type)
        {
            return memberInfo.CustomAttributes.FirstOrDefault(a => type.IsAssignableFrom(a.AttributeType));
        }

        public static CustomAttributeData? GetCustomAttributeData(this ParameterInfo paramterInfo, Type type)
        {
            return paramterInfo.CustomAttributes.FirstOrDefault(a => type.IsAssignableFrom(a.AttributeType));
        }

        public static TValue? GetConstructorArgument<TValue>(this CustomAttributeData customAttributeData, int index)
        {
            return index < customAttributeData.ConstructorArguments.Count ? (TValue?)customAttributeData.ConstructorArguments[index].Value : default;
        }

        public static TValue? GetNamedArgument<TValue>(this CustomAttributeData customAttributeData, string name)
        {
            return (TValue?)customAttributeData.NamedArguments.FirstOrDefault(a => a.MemberName == name).TypedValue.Value;
        }

        public static Type GetType<T>(this Assembly assembly)
        {
            var fullName = typeof(T).FullName;

            if (string.IsNullOrEmpty(fullName))
            {
                throw new NullReferenceException("Type FullName is null or empty");
            }

            var type = assembly.GetType(fullName);

            if (type is null)
            {
                throw new NullReferenceException($"Type of {fullName} is null");
            }

            return type;
        }
    }
}
