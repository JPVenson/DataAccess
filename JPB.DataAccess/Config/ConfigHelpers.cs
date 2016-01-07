using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.Config.Model;
#if !DEBUG
using System.Diagnostics;
#endif

namespace JPB.DataAccess.Config
{
#if !DEBUG
    [DebuggerStepThrough]
#endif
    /// <summary>
    /// 
    /// </summary>
    public static class ConfigHelper
    {
        static ConfigHelper()
        {
            ReflecionStore = new Config();
        }

        /// <summary>
        /// Anonymous type check by naming convention
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Boolean IsAnonymousType(this Type type)
        {
            //http://stackoverflow.com/questions/1650681/determining-whether-a-type-is-an-anonymous-type
            //awesome!
            return type.Namespace == null;
            //Boolean hasCompilerGeneratedAttribute = type.GetCustomAttributes().Any(s => s is CompilerGeneratedAttribute);
            //Boolean nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            //Boolean isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;
            //return nameContainsAnonymousType;
        }

        internal static Config ReflecionStore { get; set; }

        /// <summary>
        /// Get the ClassInfoCache object for the type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ClassInfoCache GetClassInfo(this Type type)
        {
            if (IsAnonymousType(type))
                return new ClassInfoCache(type, true); //Anonymous types does not have any Attributes

            return ReflecionStore.GetOrCreateClassInfoCache(type);

        }

        internal static IEnumerable<Attribute> GetCustomAttributes(this Type type)
        {
            if (IsAnonymousType(type))
                return new Attribute[0]; //Anonymous types does not have any Attributes

            return ReflecionStore.GetOrCreateClassInfoCache(type).AttributeInfoCaches.Select(s => s.Attribute);
        }

        internal static IEnumerable<Attribute> GetCustomAttributes(this PropertyInfo type)
        {
            if (IsAnonymousType(type.DeclaringType))
                return new Attribute[0]; //Anonymous types does not have any Attributes

            var deb = ReflecionStore.GetOrCreatePropertyInfoCache(type).AttributeInfoCaches.Select(s => s.Attribute);

            return deb;
        }

        internal static IEnumerable<Attribute> GetCustomAttributes(this MethodInfo type)
        {
            if (IsAnonymousType(type.DeclaringType))
                return new Attribute[0]; //Anonymous types does not have any Attributes

            var deb = ReflecionStore.GetOrCreateMethodInfoCache(type).AttributeInfoCaches.Select(s => s.Attribute);

            return deb;
        }

        internal static string GetLocalToDbSchemaMapping(this Type type, string name)
        {
            if (IsAnonymousType(type))
                return name;

            return ReflecionStore.GetOrCreateClassInfoCache(type).SchemaMappingLocalToDatabase(name);
        }

        internal static string GetDbToLocalSchemaMapping(this Type type, string name)
        {
            if (IsAnonymousType(type))
                return name;

            return ReflecionStore.GetOrCreateClassInfoCache(type).SchemaMappingDatabaseToLocal(name);
        }

        internal static IEnumerable<PropertyInfo> GetPropertiesEx(this Type type)
        {
            if (IsAnonymousType(type))
                return type.GetProperties();

            return ReflecionStore
                .GetOrCreateClassInfoCache(type)
                .PropertyInfoCaches
                .Select(s => s.Value.PropertyInfo);
        }

        internal static string[] GetSchemaMapping(this Type type)
        {
            if (IsAnonymousType(type))
                return type.GetPropertiesEx().Select(s => s.Name).ToArray();

            return ReflecionStore.GetOrCreateClassInfoCache(type).LocalToDbSchemaMapping();
        }

        internal static IEnumerable<MethodInfo> GetMethods(this Type type)
        {
            if (IsAnonymousType(type))
                return type.GetMethods();

            return ReflecionStore.GetOrCreateClassInfoCache(type).MethodInfoCaches.ToArray().Select(s => s.MethodInfo);
        }

        public static string GetPropertyInfoFromLabda<TSource, TProperty>(
    Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            return propInfo.Name;
        }

        public static string GetMehtodInfoFromLabda<TSource, TProperty>(
 Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member != null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property, not a method.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            return propInfo.Name;
        }

        //internal static string GetPropertyInfoFromLabda<T>(Expression<Func<T>> exp)
        //{
        //    Type paramType = exp.Parameters[0].Type;  // first parameter of expression
        //    var d = paramType.GetMember((exp.Body as MemberExpression).Member.Name)[0];
        //    return d.Name;
        //}
    }
}