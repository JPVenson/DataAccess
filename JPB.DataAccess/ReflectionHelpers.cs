using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace JPB.DataAccess
{
#if !DEBUG
    [DebuggerStepThrough]
#endif
    internal static class ReflectionHelpers
    {
        static ReflectionHelpers()
        {
            ReflecionStore = new ReflecionStore();
        }

        public static Boolean IsAnonymousType(this Type type)
        {
            //Boolean hasCompilerGeneratedAttribute = type.GetCustomAttributes().Any(s => s is CompilerGeneratedAttribute);
            Boolean nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            //Boolean isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return nameContainsAnonymousType;
        }

        public static ReflecionStore ReflecionStore { get; set; }

        public static Attribute[] GetCustomAttributes(this Type type)
        {
            if (IsAnonymousType(type))
                return new Attribute[0]; //anonymos types does not have any Attributes

            return ReflecionStore.GetOrCreateClassInfoCache(type).AttributeInfoCaches.ToArray().Select(s => s.Attribute).ToArray();
        }

        public static Attribute[] GetCustomAttributes(this PropertyInfo type)
        {
            if (IsAnonymousType(type.DeclaringType))
                return new Attribute[0]; //anonymos types does not have any Attributes

            var deb = ReflecionStore.GetOrCreatePropertyInfoCache(type).AttributeInfoCaches.ToArray().Select(s => s.Attribute).ToArray();

            return deb;
        }

        public static Attribute[] GetCustomAttributes(this MethodInfo type)
        {
            if (IsAnonymousType(type.DeclaringType))
                return new Attribute[0]; //anonymos types does not have any Attributes

            var deb = ReflecionStore.GetOrCreateMethodInfoCache(type).AttributeInfoCaches.ToArray().Select(s => s.Attribute).ToArray();

            return deb;
        }

        public static PropertyInfo[] GetProperties(this Type @class)
        {
            if (IsAnonymousType(@class))
                return @class.GetProperties();

            return ReflecionStore.GetOrCreatePropertyInfoCache(@class).PropertyInfoCaches.ToArray().Select(s => s.PropertyInfo).ToArray();
        }
    }
}